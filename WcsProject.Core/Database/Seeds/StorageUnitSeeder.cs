using WcsProject.Core.Entities.Matrix;

namespace WcsProject.Core.Database.Seeds;

public class StorageUnitSeeder : DataSeederBase
{
    public StorageUnitSeeder(ILogger<DataSeederBase> logger) : base(logger)
    {
    }

    public override int Order => 10; // Run early
    public override string Name => "Storage Units";

    public override async Task<bool> ShouldSeedAsync(ISqlSugarClient db)
    {
        // Check if storage units already exist
        var count = await db.Queryable<StorageUnit>().CountAsync();

        if (count > 0)
        {
            LogInfo($"Found {count} existing storage units, skipping seed");
            return false;
        }

        return true;
    }

    public override async Task SeedAsync(ISqlSugarClient db)
    {
        LogInfo("Starting seed...");

        var storageUnits = new List<StorageUnit>
        {
            new()
            {
                Code = "A001",
                Name = "Storage Unit A-001",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder",
                Version = 1,
                IsDeleted = false
            },
            new()
            {
                Code = "A002",
                Name = "Storage Unit A-002",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder",
                Version = 1,
                IsDeleted = false
            },
            new()
            {
                Code = "B001",
                Name = "Storage Unit B-001",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder",
                Version = 1,
                IsDeleted = false
            },
            new()
            {
                Code = "B002",
                Name = "Storage Unit B-002",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder",
                Version = 1,
                IsDeleted = false
            },
            new()
            {
                Code = "C001",
                Name = "Storage Unit C-001",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder",
                Version = 1,
                IsDeleted = false
            }
        };

        var count = await db.Insertable(storageUnits).ExecuteCommandAsync();

        LogInfo($"Seeded {count} storage units");
    }
}