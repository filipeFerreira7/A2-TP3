using a2_tp3_job_connect.Data;

namespace a2_tp3_job_connect.Services;

public class UnitOfWork(JobConnectDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
