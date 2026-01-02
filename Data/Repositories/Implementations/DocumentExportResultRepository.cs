using IteraClient.Data.Entities;
using IteraClient.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IteraClient.Data.Repositories.Implementations;

/// <summary>
/// Implementação do repositório de resultados de exportação.
/// </summary>
public class DocumentExportResultRepository : Repository<DocumentExportResult>, IDocumentExportResultRepository
{
    public DocumentExportResultRepository(IteraDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentExportResult>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.DocumentId == documentId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentExportResult>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Cnpj == cnpj)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Where(r => r.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        if (results.Any())
        {
            _dbSet.RemoveRange(results);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<DocumentExportResult>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbSet
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }
}
