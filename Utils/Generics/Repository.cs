using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Utils.Responses;

namespace Utils.Generics
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _db;
        protected readonly DbSet<T> _table;

        protected Repository(DbContext db)
        {
            _db = db;
            _table = _db.Set<T>();
        }

        
        //Find
        public async Task<IResult<T>> FindByIdAsync(object id)
        {
            var entity = await _table.FindAsync(id);
            if (entity == null)
            {
                return Result<T>.Failure(["Record not found"], 404);
            }
            return Result<T>.Success(entity);
        }

        public async Task<IResult<IEnumerable<T>>> FindManyAsync(Expression<Func<T, bool>>? expression = null, params string[] includes)
        {
            var entities = expression == null ? _table : _table.Where(expression);

            if (entities == null || !await entities.AnyAsync())
            {
                return Result<IEnumerable<T>>.Failure(["Entities Not found!"], 404);
            }

            foreach (var include in includes)
            {
                entities = entities.Include(include);
            }

            var data = await entities.ToListAsync();
            return Result<IEnumerable<T>>.Success(data);
        }

        public async Task<IResult<T>> FindFirstAsync(Expression<Func<T, bool>>? expression = null)
        {
            var entity = expression == null 
                ? await _table.FirstOrDefaultAsync() 
                : await _table.FirstOrDefaultAsync(expression);

            if (entity == null)
            {
                return Result<T>.Failure(["Record not found"], 404);
            }

            return Result<T>.Success(entity);
        }
        //Create
        public async Task<IResult> CreateAsync(T entity)
        {
            try
            {
                await _table.AddAsync(entity);
                return Result.Success(201); 
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message, 500);
            }
        }

        public async Task<IResult> CreateManyAsync(IEnumerable<T> entities)
        {
            try
            {
                await _table.AddRangeAsync(entities);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message, 500);
            }
        }
        
        //Update
        public async Task<IResult> UpdateAsync(T entity)
        {
            try
            {
                await Task.Run(() => _table.Update(entity));
                return Result.Success(200);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message, 500);
            }
        }

        //Delete
        public async Task<IResult> DeleteAsync(T entity)
        {
            try
            {
                await Task.Run(() => _table.Remove(entity));
                return Result.Success(204); 
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message, 500);
            }
        }

        public async Task<IResult> DeleteAsync(object id)
        {
            var result = await FindByIdAsync(id);
            if (result.IsSuccess)
            {
                return await DeleteAsync(result.Data);
            }
            return result;
        }

        public async Task<IResult<int>> CountAsync(Expression<Func<T, bool>>? expression = null)
        {
            try
            {
                int count = expression != null 
                    ? await _table.CountAsync(expression) 
                    : await _table.CountAsync();
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure([ex.Message], 500);
            }
        }

        public async Task<IResult<bool>> AnyAsync(Expression<Func<T, bool>>? expression = null)
        {
            try
            {
                bool exists = expression != null 
                    ? await _table.AnyAsync(expression) 
                    : await _table.AnyAsync();
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure([ex.Message], 500);
            }
        }
    }
}