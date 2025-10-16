namespace WcsProject.Core.Options;

/// <summary>
///     SqlSugar configuration options
/// </summary>
public class SqlSugarOptions
{
    public const string SectionName = "SqlSugar";

    public bool IsAutoRemoveDataCache { get; set; } = true;

    public bool PgSqlIsAutoToLower { get; set; } = false;

    public bool EnableJsonb { get; set; } = true;

    public bool LogSqlExecuting { get; set; } = true;

    public bool LogSqlExecuted { get; set; } = true;
}