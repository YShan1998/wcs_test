using WcsProject.Application.Modules.StorageUnit.Dtos;

namespace WcsProject.Application.Modules.StorageUnit.Services;

public interface IStorageUnitService
{
    public Task<StorageUnitDto> GetAsync(Guid id);
    public Task<StorageUnitDto> GetByCodeAsync(string code);
    public Task<List<StorageUnitDto>> GetListAsync();
    public Task<StorageUnitDto> CreateAsync(CreateStorageUnitInput input);
    public Task<StorageUnitDto> UpdateAsync(Guid id, UpdateStorageUnitInput input);
    public Task<bool> DeleteAsync(Guid id);
}