using Furion.DependencyInjection;
using Furion.FriendlyException;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WcsProject.Application.Modules.StorageUnit.Dtos;
using WcsProject.Application.Repositories.StorageUnit;

namespace WcsProject.Application.Modules.StorageUnit.Services;

[DynamicApiController]
public class StorageUnitService : IStorageUnitService, ITransient
{
    private readonly IStorageUnitRepository _storageUnitRepo;
    private readonly ILogger<StorageUnitService> _logger;
    private readonly IDistributedCache _cache;
    
    public StorageUnitService(
        IStorageUnitRepository storageUnitRepo,
        ILogger<StorageUnitService> logger,
        IDistributedCache cache)
    {
        _storageUnitRepo = storageUnitRepo;
        _logger = logger;
        _cache = cache;
    }

    public async Task<StorageUnitDto> GetAsync(Guid id)
    {
        var entity = await _storageUnitRepo.GetByIdAsync(id);
        return entity.Adapt<StorageUnitDto>();
    }

    public async Task<StorageUnitDto> GetByCodeAsync(string code)
    {
        var entity = await _storageUnitRepo.GetByCodeAsync(code);
        return entity.Adapt<StorageUnitDto>();
    }

    public async Task<List<StorageUnitDto>> GetListAsync()
    {
        var entities = await _storageUnitRepo.GetListAsync();
        return entities.Adapt<List<StorageUnitDto>>();
    }

    public async Task<StorageUnitDto> CreateAsync(CreateStorageUnitInput input)
    {
        if (await _storageUnitRepo.IsCodeExistAsync(input.Code))
            throw Oops.Oh($"Storage Unit Code {input.Code} already exists.");
        
        var entity = input.Adapt<Core.Entities.Matrix.StorageUnit>();
        
        var success = await _storageUnitRepo.InsertAsync(entity);
        
        if (!success)
            throw Oops.Oh($"Failed to create Storage Unit with Code {input.Code}.");
        
        _logger.LogInformation("Created storage unit {Code}", entity.Code);
        
        // Invalidate cache
        await _cache.RemoveAsync($"storage-units:{entity.Code}");
        
        return entity.Adapt<StorageUnitDto>();
    }

    public async Task<StorageUnitDto> UpdateAsync(Guid id, UpdateStorageUnitInput input)
    {
        var entity = await _storageUnitRepo.GetByIdAsync(id);
        
        if (entity == null)
            throw Oops.Oh("Storage unit not found");
        
        // Validate unique code
        if (input.Code != entity.Code)
        {
            if (await _storageUnitRepo.IsCodeExistAsync(input.Code, id))
                throw Oops.Oh($"Storage unit code '{input.Code}' already exists");
        }
        
        input.Adapt(entity);
        
        // Use SimpleClient update method
        var success = await _storageUnitRepo.UpdateAsync(entity);
        
        if (!success)
            throw Oops.Oh("Update failed - version mismatch or no changes");
        
        _logger.LogInformation("Updated storage unit {Id}", id);
        
        // Invalidate cache
        await _cache.RemoveAsync($"storage-units:{entity.Code}");
        
        return entity.Adapt<StorageUnitDto>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var success = await _storageUnitRepo.DeleteByIdAsync(id);
        
        if (success)
            _logger.LogInformation("Deleted storage unit {id", id);
        
        return success;
    }
}