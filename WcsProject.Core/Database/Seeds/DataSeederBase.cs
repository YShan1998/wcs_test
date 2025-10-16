namespace WcsProject.Core.Database.Seeds;

public abstract class DataSeederBase : IDataSeeder
{
    protected readonly ILogger<DataSeederBase> Logger;

    protected DataSeederBase(ILogger<DataSeederBase> logger)
    {
        Logger = logger;
    }

    public abstract int Order { get; }
    public abstract string Name { get; }

    public abstract Task<bool> ShouldSeedAsync(ISqlSugarClient db);
    public abstract Task SeedAsync(ISqlSugarClient db);

    protected void LogInfo(string message)
    {
        Logger.LogInformation("[{Seeder}] {Message}", Name, message);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning("[{Seeder}] {Message}", Name, message);
    }

    protected void LogError(Exception ex, string message)
    {
        Logger.LogError(ex, "[{Seeder}] {Message}", Name, message);
    }
}