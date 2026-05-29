using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Helpers to execute IQueryable asynchronously without referencing EF Core in Application Layer
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    
    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    
    Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    
    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
}
