using IteraClient.Data.Entities;

namespace IteraClient.Interfaces;

/// <summary>
/// Interface específica para operações com resultados de exportação.
/// </summary>
public interface IDocumentExportResultRepository : IRepository<DocumentExportResult>
{
    /// <summary>
    /// Obtém resultados por ID do documento.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de resultados do documento</returns>
    Task<IEnumerable<DocumentExportResult>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém resultados por CNPJ.
    /// </summary>
    /// <param name="cnpj">CNPJ da empresa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de resultados do CNPJ</returns>
    Task<IEnumerable<DocumentExportResult>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove todos os resultados de um documento.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
