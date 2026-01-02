using System.Linq.Expressions;
using IteraClient.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IteraClient.Data.Repositories.Implementations;

/// <summary>
/// Implementação genérica do padrão Repository.
/// Fornece operações CRUD básicas para qualquer entidade.
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IteraDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(IteraDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        // Esta implementação assume que a entidade tem uma propriedade Id do tipo Guid
        // Para entidades com chave diferente, sobrescrever este método
        var idList = ids.ToList();
        return await _dbSet
            .Where(e => idList.Contains(EF.Property<Guid>(e, "Id")))
            .ToListAsync(cancellationToken);
    }
}
