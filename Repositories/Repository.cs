using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Entities;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Repositories;

public class Repository<TEntity>(JobConnectDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _set = context.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _set.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(TEntity entity)
    {
        _set.Update(entity);
    }

    public void SoftDelete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _set.Update(entity);
    }
}
