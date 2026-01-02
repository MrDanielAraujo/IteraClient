using IteraClient.Data.Entities;
using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace IteraClient.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento e processamento em lote de documentos.
/// Permite armazenar documentos no banco de dados local e processá-los em lote na API Itera.
/// </summary>
/// <remarks>
/// Este controller implementa o fluxo completo de processamento:
/// 1. Armazenamento de documentos (Base64) no banco de dados
/// 2. Envio em lote para a API Itera
/// 3. Monitoramento automático de status
/// 4. Obtenção e armazenamento dos resultados exportados
/// 
/// **Banco de Dados:**
/// Atualmente utiliza banco em memória (InMemory), preparado para migração para PostgreSQL.
/// </remarks>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("Processamento em Lote")]
public class DocumentProcessingController(
    IDocumentProcessingService processingService,
    IDocumentRepository documentRepository,
    IDocumentExportResultRepository exportResultRepository)
    : ControllerBase
{
    private readonly IDocumentProcessingService _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    private readonly IDocumentExportResultRepository _exportResultRepository = exportResultRepository ?? throw new ArgumentNullException(nameof(exportResultRepository));

    /// <summary>
    /// Processa múltiplos documentos em lote enviando para a API Itera.
    /// </summary>
    /// <remarks>
    /// Este é o endpoint principal para processamento em lote. Ele executa o seguinte fluxo:
    /// 
    /// **Fluxo de Processamento:**
    /// 1. Busca os documentos pelos IDs no banco de dados local
    /// 2. Converte o conteúdo Base64 para arquivo
    /// 3. Envia cada documento para a API Itera via upload
    /// 4. Se `WaitForCompletion` for true, monitora o status até conclusão
    /// 5. Quando concluído, obtém os dados exportados e salva no banco
    /// 
    /// **Configurações disponíveis:**
    /// | Campo | Tipo | Padrão | Descrição |
    /// |-------|------|--------|-----------|
    /// | DocumentIds | array | - | Lista de GUIDs dos documentos a processar |
    /// | WaitForCompletion | bool | true | Se deve aguardar a conclusão do processamento |
    /// | TimeoutSeconds | int | 300 | Tempo máximo de espera (5 minutos) |
    /// | PollingIntervalSeconds | int | 10 | Intervalo entre verificações de status |
    /// 
    /// **Exemplo de requisição:**
    /// ```json
    /// {
    ///   "documentIds": [
    ///     "9d2fa731-837f-460c-9357-9417b9ae280c",
    ///     "11be1db9-1c28-4253-aece-b9727a5d3288"
    ///   ],
    ///   "waitForCompletion": true,
    ///   "timeoutSeconds": 300,
    ///   "pollingIntervalSeconds": 10
    /// }
    /// ```
    /// 
    /// **Resposta de sucesso:**
    /// ```json
    /// {
    ///   "totalDocuments": 2,
    ///   "successCount": 2,
    ///   "errorCount": 0,
    ///   "processingCount": 0,
    ///   "documentStatuses": [...],
    ///   "isSuccess": true,
    ///   "message": "Processamento concluído: 2 sucesso, 0 erro"
    /// }
    /// ```
    /// 
    /// **Importante:**
    /// - Os documentos devem ser previamente cadastrados via `POST /DocumentProcessing/Documents`
    /// - O processamento pode demorar dependendo do tamanho e quantidade de documentos
    /// - Use `WaitForCompletion: false` para processamento assíncrono
    /// </remarks>
    /// <param name="request">Configurações do processamento em lote</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Resultado detalhado do processamento de cada documento</returns>
    /// <response code="200">Processamento iniciado/concluído com sucesso</response>
    /// <response code="400">Lista de IDs vazia ou inválida</response>
    /// <response code="500">Erro interno durante o processamento</response>
    [HttpPost("ProcessBatch")]
    [ProducesResponseType(typeof(BatchProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessBatch(
        [FromBody] BatchProcessingRequest? request,
        CancellationToken cancellationToken)
    {
        if (request?.DocumentIds == null || !request.DocumentIds.Any())
        {
            return BadRequest("A lista de IDs de documentos é obrigatória.");
        }

        var result = await _processingService.ProcessDocumentsAsync(request.DocumentIds, cancellationToken);

        if (request.WaitForCompletion && result.ProcessingCount > 0)
        {
            var timeout = TimeSpan.FromSeconds(request.TimeoutSeconds);
            var pollingInterval = TimeSpan.FromSeconds(request.PollingIntervalSeconds);
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                await Task.Delay(pollingInterval, cancellationToken);

                var updatedStatuses = new List<DocumentProcessingStatus>();
                var stillProcessing = 0;

                foreach (var docStatus in result.DocumentStatuses.Where(s => s.IsProcessing))
                {
                    var currentStatus = await _processingService.CheckAndUpdateStatusAsync(
                        docStatus.DocumentId, 
                        cancellationToken);

                    updatedStatuses.Add(currentStatus);

                    if (currentStatus.IsProcessing)
                        stillProcessing++;
                    else if (currentStatus.IsSuccess)
                    {
                        result.SuccessCount++;
                        result.ProcessingCount--;
                    }
                    else
                    {
                        result.ErrorCount++;
                        result.ProcessingCount--;
                    }
                }

                foreach (var updated in updatedStatuses)
                {
                    var existing = result.DocumentStatuses.FirstOrDefault(s => s.DocumentId == updated.DocumentId);
                    if (existing != null)
                    {
                        var index = result.DocumentStatuses.IndexOf(existing);
                        result.DocumentStatuses[index] = updated;
                    }
                }

                if (stillProcessing == 0) break;
            }

            result.Message = $"Processamento concluído: {result.SuccessCount} sucesso, {result.ErrorCount} erro, {result.ProcessingCount} ainda em processamento.";
        }

        return Ok(result);
    }

    /// <summary>
    /// Verifica o status de processamento de um documento específico na Itera.
    /// </summary>
    /// <remarks>
    /// Use este endpoint para verificar o progresso de um documento individual.
    /// 
    /// **Status possíveis:**
    /// | Status | Descrição |
    /// |--------|-----------|
    /// | NotFound | Documento não existe no banco local |
    /// | NotUploaded | Documento ainda não foi enviado para Itera |
    /// | Uploaded | Documento enviado, aguardando processamento |
    /// | Processando | Documento em processamento na Itera |
    /// | Concluido | Processamento finalizado com sucesso |
    /// | Erro | Ocorreu erro no processamento |
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /DocumentProcessing/Status/9d2fa731-837f-460c-9357-9417b9ae280c
    /// ```
    /// 
    /// **Resposta de sucesso:**
    /// ```json
    /// {
    ///   "documentId": "9d2fa731-837f-460c-9357-9417b9ae280c",
    ///   "iteraDocumentId": "abc123...",
    ///   "fileName": "documento.pdf",
    ///   "status": "Concluido",
    ///   "isSuccess": true,
    ///   "isProcessing": false,
    ///   "exportedResultsCount": 150
    /// }
    /// ```
    /// 
    /// **Importante:**
    /// - Este endpoint também atualiza o status no banco de dados local
    /// - Se o status for "Concluido", os dados exportados são automaticamente salvos
    /// </remarks>
    /// <param name="documentId">ID único do documento no banco local (GUID)</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Status atual detalhado do documento</returns>
    /// <response code="200">Status obtido com sucesso</response>
    /// <response code="404">Documento não encontrado</response>
    [HttpGet("Status/{documentId}")]
    [ProducesResponseType(typeof(DocumentProcessingStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid documentId, CancellationToken cancellationToken)
    {
        var status = await _processingService.CheckAndUpdateStatusAsync(documentId, cancellationToken);
        
        if (status.Status == "NotFound")
            return NotFound(status.ErrorMessage);

        return Ok(status);
    }

    /// <summary>
    /// Obtém os resultados exportados de um documento processado com sucesso.
    /// </summary>
    /// <remarks>
    /// Retorna os dados extraídos pela Itera que foram salvos no banco de dados local.
    /// 
    /// **Pré-requisitos:**
    /// - O documento deve ter sido processado com sucesso (status "Concluido")
    /// - Os dados são salvos automaticamente quando o processamento é concluído
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /DocumentProcessing/Export/9d2fa731-837f-460c-9357-9417b9ae280c
    /// ```
    /// 
    /// **Resposta de sucesso:**
    /// ```json
    /// {
    ///   "documentId": "9d2fa731-837f-460c-9357-9417b9ae280c",
    ///   "cnpj": "34413970000130",
    ///   "isSuccess": true,
    ///   "recordsCount": 150,
    ///   "data": [
    ///     {
    ///       "codigo": "1.01",
    ///       "valor": "1000000.00",
    ///       "empresa": "Empresa XYZ",
    ///       ...
    ///     }
    ///   ]
    /// }
    /// ```
    /// 
    /// **Quando usar:**
    /// - Após verificar que o status do documento é "Concluido"
    /// - Para obter os dados extraídos sem precisar chamar a API Itera novamente
    /// </remarks>
    /// <param name="documentId">ID único do documento no banco local (GUID)</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Dados exportados do documento</returns>
    /// <response code="200">Dados obtidos com sucesso</response>
    /// <response code="404">Documento não encontrado ou sem dados exportados</response>
    [HttpGet("Export/{documentId}")]
    [ProducesResponseType(typeof(ExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExportResults(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _processingService.GetExportResultsAsync(documentId, cancellationToken);
        
        if (!result.IsSuccess && result.ErrorMessage?.Contains("não encontrado") == true)
            return NotFound(result.ErrorMessage);

        return Ok(result);
    }

    /// <summary>
    /// Lista todos os documentos armazenados no banco de dados.
    /// </summary>
    /// <remarks>
    /// Retorna uma lista resumida de todos os documentos cadastrados, incluindo seu status de processamento.
    /// 
    /// **Campos retornados:**
    /// | Campo | Descrição |
    /// |-------|-----------|
    /// | Id | ID único do documento |
    /// | FileName | Nome do arquivo |
    /// | Cnpj | CNPJ associado |
    /// | Source | Origem do documento |
    /// | Description | Descrição |
    /// | IteraDocumentId | ID na Itera (após upload) |
    /// | IteraStatus | Status na Itera |
    /// | IsProcessed | Se foi processado com sucesso |
    /// | CreatedAt | Data de criação |
    /// | UpdatedAt | Data da última atualização |
    /// | ErrorMessage | Mensagem de erro (se houver) |
    /// | ContentBase64Length | Tamanho do conteúdo em caracteres |
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /DocumentProcessing/Documents
    /// ```
    /// 
    /// **Nota:** O conteúdo Base64 não é retornado para economizar banda.
    /// Use `GET /DocumentProcessing/Documents/{id}` para detalhes completos.
    /// </remarks>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Lista de documentos com informações resumidas</returns>
    /// <response code="200">Lista obtida com sucesso</response>
    [HttpGet("Documents")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDocuments(CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetAllAsync(cancellationToken);
        return Ok(documents.Select(d => new
        {
            d.Id,
            d.FileName,
            d.Cnpj,
            d.Source,
            d.Description,
            d.IteraDocumentId,
            d.IteraStatus,
            d.IsProcessed,
            d.CreatedAt,
            d.UpdatedAt,
            d.ErrorMessage,
            ContentBase64Length = d.ContentBase64?.Length ?? 0
        }));
    }

    /// <summary>
    /// Adiciona um novo documento ao banco de dados para processamento posterior.
    /// </summary>
    /// <remarks>
    /// Armazena um documento no banco de dados local. O documento pode ser processado
    /// posteriormente usando o endpoint `POST /DocumentProcessing/ProcessBatch`.
    /// 
    /// **Campos obrigatórios:**
    /// | Campo | Tipo | Descrição |
    /// |-------|------|-----------|
    /// | FileName | string | Nome do arquivo (ex: "balanco2024.pdf") |
    /// | ContentBase64 | string | Conteúdo do arquivo codificado em Base64 |
    /// | Cnpj | string | CNPJ da empresa (14 dígitos, apenas números) |
    /// 
    /// **Campos opcionais:**
    /// | Campo | Tipo | Padrão | Descrição |
    /// |-------|------|--------|-----------|
    /// | ContentType | string | application/pdf | Tipo MIME do arquivo |
    /// | Source | string | "" | Sistema/origem do documento |
    /// | Description | string | "" | Descrição do documento |
    /// 
    /// **Exemplo de requisição:**
    /// ```json
    /// {
    ///   "fileName": "balanco-2024.pdf",
    ///   "contentBase64": "JVBERi0xLjQK...",
    ///   "contentType": "application/pdf",
    ///   "cnpj": "34413970000130",
    ///   "source": "Sistema Contábil",
    ///   "description": "Balanço Patrimonial 2024"
    /// }
    /// ```
    /// 
    /// **Como converter arquivo para Base64:**
    /// ```csharp
    /// var bytes = File.ReadAllBytes("documento.pdf");
    /// var base64 = Convert.ToBase64String(bytes);
    /// ```
    /// 
    /// **Resposta de sucesso (201 Created):**
    /// ```json
    /// {
    ///   "id": "9d2fa731-837f-460c-9357-9417b9ae280c",
    ///   "fileName": "balanco-2024.pdf",
    ///   "cnpj": "34413970000130",
    ///   "source": "Sistema Contábil",
    ///   "description": "Balanço Patrimonial 2024",
    ///   "createdAt": "2024-01-15T10:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do documento a ser armazenado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Documento criado com seu ID gerado</returns>
    /// <response code="201">Documento criado com sucesso</response>
    /// <response code="400">Dados inválidos ou campos obrigatórios faltando</response>
    [HttpPost("Documents")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDocument(
        [FromBody] AddDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ContentBase64))
            return BadRequest("O conteúdo em Base64 é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Cnpj))
            return BadRequest("O CNPJ é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest("O nome do arquivo é obrigatório.");

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            ContentBase64 = request.ContentBase64,
            ContentType = request.ContentType ?? "application/pdf",
            Cnpj = request.Cnpj,
            Source = request.Source ?? string.Empty,
            Description = request.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, new
        {
            document.Id,
            document.FileName,
            document.Cnpj,
            document.Source,
            document.Description,
            document.CreatedAt
        });
    }

    /// <summary>
    /// Obtém os detalhes de um documento específico pelo ID.
    /// </summary>
    /// <remarks>
    /// Retorna informações detalhadas de um documento, exceto o conteúdo Base64
    /// (para economizar banda).
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /DocumentProcessing/Documents/9d2fa731-837f-460c-9357-9417b9ae280c
    /// ```
    /// 
    /// **Resposta de sucesso:**
    /// ```json
    /// {
    ///   "id": "9d2fa731-837f-460c-9357-9417b9ae280c",
    ///   "fileName": "balanco-2024.pdf",
    ///   "cnpj": "34413970000130",
    ///   "source": "Sistema Contábil",
    ///   "description": "Balanço Patrimonial 2024",
    ///   "iteraDocumentId": "abc123...",
    ///   "iteraStatus": "Concluido",
    ///   "isProcessed": true,
    ///   "createdAt": "2024-01-15T10:30:00Z",
    ///   "updatedAt": "2024-01-15T10:35:00Z",
    ///   "errorMessage": null,
    ///   "contentBase64Length": 125000
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID único do documento (GUID)</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Detalhes do documento</returns>
    /// <response code="200">Documento encontrado</response>
    /// <response code="404">Documento não encontrado</response>
    [HttpGet("Documents/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        
        if (document == null)
            return NotFound("Documento não encontrado.");

        return Ok(new
        {
            document.Id,
            document.FileName,
            document.Cnpj,
            document.Source,
            document.Description,
            document.IteraDocumentId,
            document.IteraStatus,
            document.IsProcessed,
            document.CreatedAt,
            document.UpdatedAt,
            document.ErrorMessage,
            ContentBase64Length = document.ContentBase64?.Length ?? 0
        });
    }

    /// <summary>
    /// Adiciona múltiplos documentos ao banco de dados em uma única requisição.
    /// </summary>
    /// <remarks>
    /// Permite cadastrar vários documentos de uma vez, ideal para importação em massa.
    /// 
    /// **Exemplo de requisição:**
    /// ```json
    /// [
    ///   {
    ///     "fileName": "balanco-2024-q1.pdf",
    ///     "contentBase64": "JVBERi0xLjQK...",
    ///     "cnpj": "34413970000130",
    ///     "source": "Sistema Contábil",
    ///     "description": "Balanço Q1 2024"
    ///   },
    ///   {
    ///     "fileName": "balanco-2024-q2.pdf",
    ///     "contentBase64": "JVBERi0xLjQK...",
    ///     "cnpj": "34413970000130",
    ///     "source": "Sistema Contábil",
    ///     "description": "Balanço Q2 2024"
    ///   }
    /// ]
    /// ```
    /// 
    /// **Resposta de sucesso (201 Created):**
    /// ```json
    /// {
    ///   "created": [
    ///     { "id": "9d2fa731-...", "fileName": "balanco-2024-q1.pdf", "cnpj": "34413970000130" },
    ///     { "id": "11be1db9-...", "fileName": "balanco-2024-q2.pdf", "cnpj": "34413970000130" }
    ///   ],
    ///   "errors": []
    /// }
    /// ```
    /// 
    /// **Tratamento de erros:**
    /// - Documentos válidos são criados mesmo se alguns falharem
    /// - Erros de validação são retornados no array `errors`
    /// - Se todos falharem, retorna 400 Bad Request
    /// </remarks>
    /// <param name="requests">Lista de documentos a serem criados</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Lista de documentos criados e eventuais erros</returns>
    /// <response code="201">Documentos criados (pode conter erros parciais)</response>
    /// <response code="400">Todos os documentos falharam na validação</response>
    [HttpPost("Documents/Batch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDocumentsBatch(
        [FromBody] List<AddDocumentRequest>? requests,
        CancellationToken cancellationToken)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("A lista de documentos é obrigatória.");

        var documents = new List<Document>();
        var errors = new List<string>();

        for (int i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            
            if (string.IsNullOrWhiteSpace(request.ContentBase64))
            {
                errors.Add($"Documento {i + 1}: O conteúdo em Base64 é obrigatório.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(request.Cnpj))
            {
                errors.Add($"Documento {i + 1}: O CNPJ é obrigatório.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                errors.Add($"Documento {i + 1}: O nome do arquivo é obrigatório.");
                continue;
            }

            documents.Add(new Document
            {
                Id = Guid.NewGuid(),
                FileName = request.FileName,
                ContentBase64 = request.ContentBase64,
                ContentType = request.ContentType ?? "application/pdf",
                Cnpj = request.Cnpj,
                Source = request.Source ?? string.Empty,
                Description = request.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (errors.Any() && !documents.Any())
            return BadRequest(new { Errors = errors });

        await _documentRepository.AddRangeAsync(documents, cancellationToken);

        return CreatedAtAction(nameof(GetAllDocuments), null, new
        {
            Created = documents.Select(d => new { d.Id, d.FileName, d.Cnpj }),
            Errors = errors
        });
    }
}
