using IteraClient.Data.Entities;

namespace IteraClient.Interfaces;

/// <summary>
/// Interface específica para operações com documentos.
/// Estende IRepository com métodos específicos para Document.
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Obtém documentos não processados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de documentos não processados</returns>
    Task<IEnumerable<Document>> GetUnprocessedDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém documentos por CNPJ.
    /// </summary>
    /// <param name="cnpj">CNPJ da empresa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de documentos do CNPJ</returns>
    Task<IEnumerable<Document>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém documento pelo ID da Itera.
    /// </summary>
    /// <param name="iteraDocumentId">ID do documento na Itera</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Documento encontrado ou null</returns>
    Task<Document?> GetByIteraIdAsync(Guid iteraDocumentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status do documento na Itera.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="iteraDocumentId">ID retornado pela Itera</param>
    /// <param name="status">Status atual</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task UpdateIteraStatusAsync(Guid documentId, Guid? iteraDocumentId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca documento como processado.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task MarkAsProcessedAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca documento com erro.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="errorMessage">Mensagem de erro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task MarkAsErrorAsync(Guid documentId, string errorMessage, CancellationToken cancellationToken = default);
}
