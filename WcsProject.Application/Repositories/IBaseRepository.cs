using System.Linq.Expressions;

namespace WcsProject.Application.Repositories;

public interface IBaseRepository<T> where T : class, new()
{
    ISqlSugarClient GetClient();

    // SimpleClient methods exposed through interface
    Task<T> GetByIdAsync(dynamic id);
    Task<List<T>> GetListAsync();
    Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression);
    Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression);
    Task<int> CountAsync(Expression<Func<T, bool>> whereExpression);

    Task<bool> InsertAsync(T insertObj);
    Task<int> InsertReturnIdentityAsync(T insertObj);
    Task<bool> InsertRangeAsync(List<T> insertObjs);

    Task<bool> UpdateAsync(T updateObj);
    Task<bool> UpdateRangeAsync(List<T> updateObjs);

    Task<bool> DeleteAsync(T deleteObj);
    Task<bool> DeleteByIdAsync(dynamic id);
    Task<bool> DeleteByIdsAsync(dynamic[] ids);

    // Advanced
    // Task<PagedList<T>> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> whereExpression = null);
    // ISqlSugarClient GetClient(); // Escape hatch for complex queries
}