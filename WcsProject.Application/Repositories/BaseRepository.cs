using Furion;
using SqlSugar;
using WcsProject.Core.Entities;

namespace WcsProject.Application.Repositories;

public class BaseRepository<T> : SimpleClient<T>, IBaseRepository<T> where T : class, new()
{
    public BaseRepository(ISqlSugarClient context = null) : base(context)
    {
        Context = context ?? App.GetService<ISqlSugarClient>();
        
        // Configure query filters (soft delete, multi-tenancy)
        // Context.QueryFilter.AddTableFilter<T>(x => (x is AuditEntity) == false).Add(new TableFilterItem<T>(it => (it as AuditEntity).IsDeleted == false));
    }
    
    // Expose Context through interface
    public ISqlSugarClient GetClient() => Context;
    
    // Override SimpleClient methods to add custom behavior
    public new async Task<bool> DeleteAsync(T deleteObj)
    {
        // Soft delete instead of hard delete
        if (deleteObj is AuditEntity auditEntity)
        {
            auditEntity.IsDeleted = true;
            auditEntity.DeletedAt = DateTime.UtcNow;
            auditEntity.DeletedBy = "admin"; // TODO: need to get current user
            
            return await base.UpdateAsync(deleteObj);
        }
        
        return await base.DeleteAsync(deleteObj);
    }
    
    public new async Task<bool> DeleteByIdAsync(dynamic id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        
        return await DeleteAsync(entity);
    }
    
    public new async Task<bool> DeleteByIdsAsync(dynamic[] ids)
    {
        var entities = await Context.Queryable<T>()
            .In(ids)
            .ToListAsync();
        
        var now = DateTime.UtcNow;
        var user = "admin"; // TODO: need to get current user
        
        foreach (var entity in entities)
        {
            if (entity is AuditEntity auditEntity)
            {
                auditEntity.IsDeleted = true;
                auditEntity.DeletedAt = now;
                auditEntity.DeletedBy = user;
            }
        }
        
        return await base.UpdateRangeAsync(entities);
    }
    
    // Add custom methods here
    public async Task<T> GetByIdWithoutDeletedAsync(int id)
    {
        return await Context.Queryable<T>()
            .In(id)
            .FirstAsync();
    }
}