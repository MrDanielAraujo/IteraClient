using IteraClient.Data.Entities;
using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IteraClient.Services.Implementations;

/// <summary>
/// Serviço responsável pelo processamento em lote de documentos.
/// Segue os princípios SOLID:
/// - SRP: Orquestra o fluxo de processamento de documentos
/// - DIP: Depende de abstrações (interfaces)
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentExportResultRepository _exportResultRepository;
    private readonly IIteraApiClient _iteraApiClient;
    private readonly ILogger<DocumentProcessingService> _logger;

    // Status que indicam conclusão bem-sucedida
    private static readonly string[] SuccessStatuses = { "Concluido", "Concluded", "Success", "Completed" };
    
    // Status que indicam erro
    private static readonly string[] ErrorStatuses = { "Erro", "Error", "Failed", "Rejected" };

    public DocumentProcessingService(
        IDocumentRepository documentRepository,
        IDocumentExportResultRepository exportResultRepository,
        IIteraApiClient iteraApiClient,
        ILogger<DocumentProcessingService> logger)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _exportResultRepository = exportResultRepository ?? throw new ArgumentNullException(nameof(exportResultRepository));
        _iteraApiClient = iteraApiClient ?? throw new ArgumentNullException(nameof(iteraApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<BatchProcessingResult> ProcessDocumentsAsync(
        IEnumerable<Guid> documentIds,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchProcessingResult();
        var idList = documentIds.ToList();
        result.TotalDocuments = idList.Count;

        _logger.LogInformation("Iniciando processamento de {Count} documentos", idList.Count);

        // Busca os documentos no banco
        var documents = (await _documentRepository.GetByIdsAsync(idList, cancellationToken)).ToList();

        if (documents.Count == 0)
        {
            result.Message = "Nenhum documento encontrado com os IDs fornecidos.";
            return result;
        }

        // Processa cada documento
        foreach (var document in documents)
        {
            var status = new DocumentProcessingStatus
            {
                DocumentId = document.Id,
                FileName = document.FileName
            };

            try
            {
                // 1. Upload para Itera
                _logger.LogInformation("Enviando documento {DocumentId} para Itera", document.Id);
                
                var uploadResponse = await UploadDocumentToIteraAsync(document, cancellationToken);
                
                if (!string.IsNullOrEmpty(uploadResponse.Uid) && Guid.TryParse(uploadResponse.Uid, out var iteraId))
                {
                    document.IteraDocumentId = iteraId;
                    status.IteraDocumentId = iteraId;
                    
                    await _documentRepository.UpdateIteraStatusAsync(
                        document.Id, 
                        iteraId, 
                        "Uploaded", 
                        cancellationToken);

                    _logger.LogInformation("Documento {DocumentId} enviado com sucesso. Itera ID: {IteraId}", 
                        document.Id, iteraId);

                    status.Status = "Uploaded";
                    status.IsProcessing = true;
                }
                else
                {
                    // Upload retornou sucesso mas sem ID - pode ser que a API retorne de forma diferente
                    status.Status = uploadResponse.Status;
                    status.IsProcessing = true;
                    
                    await _documentRepository.UpdateIteraStatusAsync(
                        document.Id, 
                        null, 
                        uploadResponse.Status, 
                        cancellationToken);
                }

                result.ProcessingCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar documento {DocumentId}", document.Id);
                
                status.Status = "Error";
                status.IsSuccess = false;
                status.ErrorMessage = ex.Message;
                
                await _documentRepository.MarkAsErrorAsync(document.Id, ex.Message, cancellationToken);
                
                result.ErrorCount++;
            }

            result.DocumentStatuses.Add(status);
        }

        result.Message = $"Processamento iniciado: {result.ProcessingCount} em processamento, {result.ErrorCount} com erro.";
        return result;
    }

    /// <inheritdoc />
    public async Task<DocumentProcessingStatus> CheckAndUpdateStatusAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var status = new DocumentProcessingStatus { DocumentId = documentId };

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            status.Status = "NotFound";
            status.ErrorMessage = "Documento não encontrado.";
            return status;
        }

        status.FileName = document.FileName;
        status.IteraDocumentId = document.IteraDocumentId;

        if (!document.IteraDocumentId.HasValue)
        {
            status.Status = "NotUploaded";
            status.ErrorMessage = "Documento ainda não foi enviado para Itera.";
            return status;
        }

        try
        {
            // Consulta status na Itera
            var iteraStatus = await _iteraApiClient.GetStatusAsync(document.IteraDocumentId.Value, cancellationToken);
            
            status.Status = iteraStatus.status;
            
            // Atualiza status no banco
            await _documentRepository.UpdateIteraStatusAsync(
                documentId, 
                document.IteraDocumentId, 
                iteraStatus.status, 
                cancellationToken);

            // Verifica se concluiu com sucesso
            if (SuccessStatuses.Any(s => iteraStatus.status.Contains(s, StringComparison.OrdinalIgnoreCase)))
            {
                status.IsSuccess = true;
                status.IsProcessing = false;

                // Obtém e salva os resultados exportados
                var exportResult = await GetAndSaveExportResultsAsync(document, cancellationToken);
                status.ExportedResultsCount = exportResult.RecordsCount;

                await _documentRepository.MarkAsProcessedAsync(documentId, cancellationToken);
                
                _logger.LogInformation("Documento {DocumentId} processado com sucesso. {Count} registros exportados.", 
                    documentId, exportResult.RecordsCount);
            }
            else if (ErrorStatuses.Any(s => iteraStatus.status.Contains(s, StringComparison.OrdinalIgnoreCase)))
            {
                status.IsSuccess = false;
                status.IsProcessing = false;
                status.ErrorMessage = $"Processamento falhou na Itera: {iteraStatus.status}";
                
                await _documentRepository.MarkAsErrorAsync(documentId, status.ErrorMessage, cancellationToken);
            }
            else
            {
                status.IsProcessing = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status do documento {DocumentId}", documentId);
            status.Status = "Error";
            status.ErrorMessage = ex.Message;
        }

        return status;
    }

    /// <inheritdoc />
    public async Task<ExportResult> GetExportResultsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var result = new ExportResult { DocumentId = documentId };

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            result.ErrorMessage = "Documento não encontrado.";
            return result;
        }

        result.Cnpj = document.Cnpj;

        // Busca resultados já salvos no banco
        var savedResults = await _exportResultRepository.GetByDocumentIdAsync(documentId, cancellationToken);
        var resultList = savedResults.ToList();

        if (resultList.Any())
        {
            result.IsSuccess = true;
            result.RecordsCount = resultList.Count;
            result.Data = resultList.Select(MapToControladoraResponse).ToList();
        }
        else
        {
            result.ErrorMessage = "Nenhum resultado encontrado para este documento.";
        }

        return result;
    }

    /// <summary>
    /// Faz upload de um documento para a Itera.
    /// </summary>
    private async Task<UploadDocumentResponse> UploadDocumentToIteraAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        // Converte Base64 para IFormFile
        var fileBytes = Convert.FromBase64String(document.ContentBase64);
        using var stream = new MemoryStream(fileBytes);
        
        var formFile = new FormFile(stream, 0, fileBytes.Length, "file", document.FileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = document.ContentType
        };

        return await _iteraApiClient.UploadDocumentAsync(
            formFile,
            document.Source,
            document.Description,
            document.Cnpj,
            cancellationToken);
    }

    /// <summary>
    /// Obtém e salva os resultados exportados da Itera.
    /// </summary>
    private async Task<ExportResult> GetAndSaveExportResultsAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        var result = new ExportResult
        {
            DocumentId = document.Id,
            Cnpj = document.Cnpj
        };

        try
        {
            // Converte CNPJ para long (remove formatação)
            var cnpjClean = new string(document.Cnpj.Where(char.IsDigit).ToArray());
            if (!long.TryParse(cnpjClean, out var cnpjLong))
            {
                result.ErrorMessage = "CNPJ inválido para exportação.";
                return result;
            }

            // Obtém dados da Itera
            var exportData = await _iteraApiClient.GetExportJsonAsync(cnpjLong, cancellationToken);

            if (exportData.Any())
            {
                // Remove resultados anteriores deste documento
                await _exportResultRepository.DeleteByDocumentIdAsync(document.Id, cancellationToken);

                // Converte e salva novos resultados
                var entities = exportData.Select(e => MapToExportResultEntity(e, document.Id)).ToList();
                await _exportResultRepository.AddRangeAsync(entities, cancellationToken);

                result.IsSuccess = true;
                result.RecordsCount = entities.Count;
                result.Data = exportData;

                _logger.LogInformation("Salvos {Count} registros de exportação para documento {DocumentId}", 
                    entities.Count, document.Id);
            }
            else
            {
                result.ErrorMessage = "Nenhum dado retornado pela exportação.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter/salvar resultados de exportação para documento {DocumentId}", document.Id);
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Mapeia ControladoraResponse para entidade DocumentExportResult.
    /// </summary>
    private static DocumentExportResult MapToExportResultEntity(ControladoraResponse source, Guid documentId)
    {
        return new DocumentExportResult
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Codigo = source.codigo ?? string.Empty,
            Tempo = source.tempo ?? string.Empty,
            Conf = source.conf ?? string.Empty,
            Data = source.data ?? string.Empty,
            DataAno = source.data_ano ?? string.Empty,
            TermoTotal = source.termo_total ?? string.Empty,
            Valor = source.valor ?? string.Empty,
            Page = source.page ?? string.Empty,
            ItemId = source.id ?? string.Empty,
            Subsection = source.subsection ?? string.Empty,
            Tipo = source.tipo ?? string.Empty,
            Section = source.section ?? string.Empty,
            Moeda = source.moeda ?? string.Empty,
            Escala = source.escala ?? string.Empty,
            Empresa = source.empresa ?? string.Empty,
            Cnpj = source.cnpj ?? string.Empty,
            Consolidado = source.consolidado ?? string.Empty,
            TipoBalanco = source.tipo_balanco ?? string.Empty,
            Y = source.y ?? string.Empty,
            X = source.x ?? string.Empty,
            UniqueId = source.unique_id ?? string.Empty,
            Parent = source.parent ?? string.Empty,
            IsTotal = source.is_total ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mapeia entidade DocumentExportResult para ControladoraResponse.
    /// </summary>
    private static ControladoraResponse MapToControladoraResponse(DocumentExportResult source)
    {
        return new ControladoraResponse
        {
            codigo = source.Codigo,
            tempo = source.Tempo,
            conf = source.Conf,
            data = source.Data,
            data_ano = source.DataAno,
            termo_total = source.TermoTotal,
            valor = source.Valor,
            page = source.Page,
            id = source.ItemId,
            subsection = source.Subsection,
            tipo = source.Tipo,
            section = source.Section,
            moeda = source.Moeda,
            escala = source.Escala,
            empresa = source.Empresa,
            cnpj = source.Cnpj,
            consolidado = source.Consolidado,
            tipo_balanco = source.TipoBalanco,
            y = source.Y,
            x = source.X,
            unique_id = source.UniqueId,
            parent = source.Parent,
            is_total = source.IsTotal
        };
    }
}
