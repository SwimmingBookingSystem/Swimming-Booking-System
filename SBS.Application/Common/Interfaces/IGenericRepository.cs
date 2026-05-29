using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query();
    
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    
    Task AddRangeAsync(System.Collections.Generic.IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    void Update(T entity);
    
    void Delete(T entity);
    
    void DeleteRange(System.Collections.Generic.IEnumerable<T> entities);
}
