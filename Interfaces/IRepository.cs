using System.Linq.Expressions;

namespace IteraClient.Interfaces;

/// <summary>
/// Interface genérica de repositório para operações CRUD.
/// Segue o princípio de Interface Segregation (ISP) e permite fácil substituição de implementações.
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtém uma entidade pelo ID.
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as entidades.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém entidades que correspondem a um predicado.
    /// </summary>
    /// <param name="predicate">Expressão de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades filtradas</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova entidade.
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade adicionada</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona múltiplas entidades.
    /// </summary>
    /// <param name="entities">Entidades a serem adicionadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente.
    /// </summary>
    /// <param name="entity">Entidade a ser atualizada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma entidade.
    /// </summary>
    /// <param name="entity">Entidade a ser removida</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém múltiplas entidades pelos IDs.
    /// </summary>
    /// <param name="ids">Lista de IDs</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades encontradas</returns>
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
