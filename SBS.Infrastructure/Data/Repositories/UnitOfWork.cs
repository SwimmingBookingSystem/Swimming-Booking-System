using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<string, object>();
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T).Name;
        return (IGenericRepository<T>)_repositories.GetOrAdd(type, _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // Async Execution Helpers using EF Core extension methods
    public async Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        return await query.CountAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
