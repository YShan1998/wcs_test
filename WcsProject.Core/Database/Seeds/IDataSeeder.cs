using SqlSugar;

namespace WcsProject.Core.Database.Seeds;

public interface IDataSeeder
{
    /// <summary>
    ///     Order in which this seeder should run (lower numbers run first)
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     Name of the seeder for logging purposes
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Check if data already exists and seeding should be skipped
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    Task<bool> ShouldSeedAsync(ISqlSugarClient db);

    /// <summary>
    ///     Perform the actual data seeding
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    Task SeedAsync(ISqlSugarClient db);
}