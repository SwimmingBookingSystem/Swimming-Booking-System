using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Data.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public IQueryable<T> Query()
    {
        return DbSet;
    }

    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new[] { id }, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(System.Collections.Generic.IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        DbSet.Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(T entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }
        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(System.Collections.Generic.IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
}
