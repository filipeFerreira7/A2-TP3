using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Repositories;

public interface IRepository<TEntity> where TEntity : EntidadeBase
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void SoftDelete(TEntity entity);
}
