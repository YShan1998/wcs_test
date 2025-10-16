namespace WcsProject.Application.Repositories.StorageUnit;

public interface IStorageUnitRepository : IBaseRepository<Core.Entities.Matrix.StorageUnit>
{
    // Domain-specific queries
    Task<Core.Entities.Matrix.StorageUnit> GetByCodeAsync(string code);
    Task<bool> IsCodeExistAsync(string code, Guid? excludeId = null);
}