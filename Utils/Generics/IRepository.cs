using System.Linq.Expressions;
using Utils.Responses;

namespace Utils.Generics
{
    /// <summary>
    /// Refined generic interface for GetMaidV2 database operations.
    /// Handles standard CRUD while supporting complex service-based filtering.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        // CREATE
        Task<IResult> CreateAsync(T entity);
        Task<IResult> CreateManyAsync(IEnumerable<T> entities);

        // READ
        Task<IResult<T>> FindByIdAsync(object id);
        Task<IResult<T>> FindFirstAsync(Expression<Func<T, bool>>? expression = null);
        
        /// <summary>
        /// Retrieves multiple entities with support for Eager Loading (Includes).
        /// Essential for GetMaid to pull a Worker along with their Reviews.
        /// </summary>
        Task<IResult<IEnumerable<T>>> FindManyAsync(Expression<Func<T, bool>>? expression = null, params string[] includes);

        // UPDATE
        Task<IResult> UpdateAsync(T entity);

        // DELETE
        Task<IResult> DeleteAsync(T entity);
        Task<IResult> DeleteAsync(object id);

        // UTILITY
        Task<IResult<int>> CountAsync(Expression<Func<T, bool>>? expression = null);
        Task<IResult<bool>> AnyAsync(Expression<Func<T, bool>>? expression = null);
    }
}