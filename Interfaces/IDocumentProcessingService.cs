using IteraClient.Models;

namespace IteraClient.Interfaces;

/// <summary>
/// Interface para o serviço de processamento de documentos em lote.
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Processa múltiplos documentos: upload para Itera, monitoramento de status e gravação de resultados.
    /// </summary>
    /// <param name="documentIds">Lista de IDs dos documentos a processar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado do processamento em lote</returns>
    Task<BatchProcessingResult> ProcessDocumentsAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica o status de um documento na Itera e atualiza o banco.
    /// </summary>
    /// <param name="documentId">ID do documento local</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status atual do documento</returns>
    Task<DocumentProcessingStatus> CheckAndUpdateStatusAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os resultados exportados de um documento processado.
    /// </summary>
    /// <param name="documentId">ID do documento local</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da exportação</returns>
    Task<ExportResult> GetExportResultsAsync(Guid documentId, CancellationToken cancellationToken = default);
}
