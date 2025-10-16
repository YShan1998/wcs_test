using Furion.DependencyInjection;
using SqlSugar;

namespace WcsProject.Application.Repositories.StorageUnit;

public class StorageUnitRepository : BaseRepository<Core.Entities.Matrix.StorageUnit>, IStorageUnitRepository, ITransient
{
    public StorageUnitRepository(ISqlSugarClient context) : base(context)
    {
        
    }
    
    public async Task<Core.Entities.Matrix.StorageUnit> GetByCodeAsync(string code)
    {
        return await GetSingleAsync(x => x.Code == code);
    }

    public async Task<bool> IsCodeExistAsync(string code, Guid? excludeId = null)
    {
        var query = Context.Queryable<Core.Entities.Matrix.StorageUnit>()
            .Where(x => x.Code == code);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }
}