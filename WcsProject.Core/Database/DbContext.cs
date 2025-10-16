using WcsProject.Core.Database.DbScopes;
using WcsProject.Core.Entities;
using WcsProject.Core.Options;

namespace WcsProject.Core.Database;

public static class DbContext
{
    public static readonly SqlSugarScope Instance = InitializeSqlSugar();

    private static SqlSugarScope InitializeSqlSugar()
    {
        try
        {
            // Get SqlSugarOptions from appsettings.json
            var configuration = App.GetService<IConfiguration>();
            var logger = App.GetService<ILogger<object>>();
            var sqlSugarOptions = App.GetService<IOptions<SqlSugarOptions>>()?.Value ?? new SqlSugarOptions();

            // Build ConnectionConfigs from configuration
            var connectionConfigs = new List<ConnectionConfig>();
            var defaultConnectionConfig = new ConnectionConfig
            {
                ConfigId = DefaultDbScope.ConfigId,
                DbType = DbType.PostgreSQL,
                ConnectionString = configuration.GetConnectionString(DefaultDbScope.ConfigName),
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                
                // Configure external services here
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    EntityService = (x, p) =>
                    {
                        // Skip DTO classes
                        if (String.IsNullOrEmpty(p.EntityName) || p.EntityName.ToLower().EndsWith("dto"))
                            return;

                        p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);
                    },
                    EntityNameService = (x, p) =>
                    {
                        // Skip DTO classes
                        if (String.IsNullOrEmpty(p.EntityName) || p.EntityName.ToLower().EndsWith("dto"))
                            return;

                        p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);
                    }
                },
                
                // Configure MoreSettings here
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = sqlSugarOptions.IsAutoRemoveDataCache,
                    PgSqlIsAutoToLower = sqlSugarOptions.PgSqlIsAutoToLower,
                    EnableJsonb = sqlSugarOptions.EnableJsonb
                }
            };
            connectionConfigs.Add(defaultConnectionConfig);

            var sqlSugarScope = new SqlSugarScope(connectionConfigs, db =>
            {
                if (sqlSugarOptions.LogSqlExecuting)
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        var logMessage = sql;
                        if (pars is { Length: > 0 })
                        {
                            var parameters = string.Join(", ", 
                                pars.Select(p => $"{p.ParameterName}={p.Value}"));
                            logMessage += $", Parameters: {parameters}";
                        }
                        logger?.LogInformation("SQL Executing: {sql}", logMessage);
                    };

                // Autofill audit fields
                db.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    if (entityInfo.EntityValue is AuditEntity auditEntity)
                    {
                        var currentUser = "admin"; // TODO: Need to get current user
                        var now = DateTime.UtcNow;

                        if (entityInfo.OperationType == DataFilterType.InsertByObject)
                            switch (entityInfo.PropertyName)
                            {
                                case "CreatedAt":
                                    if ((DateTime)oldValue == default)
                                        entityInfo.SetValue(now);
                                    break;
                                case "CreatedBy":
                                    if (oldValue == null)
                                        entityInfo.SetValue(currentUser);
                                    break;
                                case "Version":
                                    if ((int)oldValue == 0)
                                        entityInfo.SetValue(1);
                                    break;
                                case "IsDeleted":
                                    if (oldValue == null)
                                        entityInfo.SetValue(false);
                                    break;
                            }

                        if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                            switch (entityInfo.PropertyName)
                            {
                                case "UpdatedAt":
                                    entityInfo.SetValue(now);
                                    break;
                                case "UpdatedBy":
                                    entityInfo.SetValue(currentUser);
                                    break;
                            }
                    }
                };
            });

            return sqlSugarScope;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize SqlSugar: {ex.Message}");
            throw;
        }
    }
}