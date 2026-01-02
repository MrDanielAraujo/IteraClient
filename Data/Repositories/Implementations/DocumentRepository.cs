using IteraClient.Data.Entities;
using IteraClient.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IteraClient.Data.Repositories.Implementations;

/// <summary>
/// Implementação do repositório de documentos.
/// </summary>
public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(IteraDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetUnprocessedDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => !d.IsProcessed && d.ErrorMessage == null)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.Cnpj == cnpj)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Document?> GetByIteraIdAsync(Guid iteraDocumentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.IteraDocumentId == iteraDocumentId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateIteraStatusAsync(Guid documentId, Guid? iteraDocumentId, string status, CancellationToken cancellationToken = default)
    {
        var document = await _dbSet.FindAsync(new object[] { documentId }, cancellationToken);
        if (document != null)
        {
            document.IteraDocumentId = iteraDocumentId;
            document.IteraStatus = status;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _dbSet.FindAsync(new object[] { documentId }, cancellationToken);
        if (document != null)
        {
            document.IsProcessed = true;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task MarkAsErrorAsync(Guid documentId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var document = await _dbSet.FindAsync(new object[] { documentId }, cancellationToken);
        if (document != null)
        {
            document.ErrorMessage = errorMessage;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Document>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbSet
            .Where(d => idList.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }
}
