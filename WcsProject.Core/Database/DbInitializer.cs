using Furion.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using WcsProject.Core.Database.DbScopes;
using WcsProject.Core.Database.Seeds;
using WcsProject.Core.Entities;
using WcsProject.Core.Entities.Matrix;

namespace WcsProject.Core.Database;

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer : IDbInitializer, IScoped
{
    private readonly ISqlSugarClient _db;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DbInitializer(
        ISqlSugarClient db,
        ILogger<DbInitializer> logger,
        IEnumerable<IDataSeeder> seeders)
    {
        _db = db;
        _logger = logger;
        _seeders = seeders;
    }

    public async Task InitializeAsync()
    {
        await InitializeDefaultDbAsync();
    }

    private async Task InitializeDefaultDbAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization for {ConfigId}...",
                DefaultDbScope.ConfigId);

            // Create database if not exists
            if (!_db.DbMaintenance.IsAnyTable("storage_units", false))
            {
                _db.DbMaintenance.CreateDatabase();
                _logger.LogInformation("Created database: {ConfigId}", DefaultDbScope.ConfigId);
            }

            // Get all entity types from assembly
            var entityTypes = GetEntityTypes();

            _logger.LogInformation("Found {Count} entity types to initialize", entityTypes.Length);

            // Create tables
            _db.CodeFirst.InitTables(entityTypes);

            _logger.LogInformation("Successfully initialized {Count} tables:", entityTypes.Length);
            foreach (var type in entityTypes) _logger.LogInformation("  ✓ {TableName}", type.Name);

            // Seed initial data
            await SeedDataAsync();

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    public async Task SeedDataAsync()
    {
        _logger.LogInformation("Starting data seeding...");

        // Order seeders by their Order property
        var orderedSeeders = _seeders.OrderBy(s => s.Order).ToList();

        _logger.LogInformation("Found {Count} seeders to execute", orderedSeeders.Count);

        foreach (var seeder in orderedSeeders)
            try
            {
                _logger.LogInformation("Checking seeder: {Name} (Order: {Order})",
                    seeder.Name, seeder.Order);

                // Check if seeding is needed
                if (await seeder.ShouldSeedAsync(_db))
                {
                    _logger.LogInformation("Executing seeder: {Name}", seeder.Name);

                    await seeder.SeedAsync(_db);

                    _logger.LogInformation("✓ Completed seeder: {Name}", seeder.Name);
                }
                else
                {
                    _logger.LogInformation("⊘ Skipped seeder: {Name}", seeder.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute seeder: {Name}", seeder.Name);
                // Continue with other seeders even if one fails
            }

        _logger.LogInformation("Data seeding completed");
    }

    private Type[] GetEntityTypes(string namespaceFilter = null)
    {
        // 1. Get the assembly
        var assembly = typeof(StorageUnit).Assembly;

        // 2. Get all types and filter
        var query = assembly.GetTypes()
            .Where(t =>
                t.IsClass && // Only classes
                !t.IsAbstract && // Only concrete (not abstract)
                typeof(AuditEntity).IsAssignableFrom(t)); // Only inherits AuditEntity

        if (!string.IsNullOrEmpty(namespaceFilter))
            query = query.Where(t => t.Namespace?.Contains(namespaceFilter) ?? false);

        return query.ToArray();
    }
}