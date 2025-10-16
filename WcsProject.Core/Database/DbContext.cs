using Furion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using WcsProject.Core.Entities;
using WcsProject.Core.Options;

namespace WcsProject.Core.Database;

public static class DbContext
{
    public static readonly SqlSugarScope Instance = InitializeSqlSugar();

    private static SqlSugarScope InitializeSqlSugar()
    {
        // Get ConnectionConfigs from appsettings.json
        var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
        
        // Get SqlSugarOptions from appsettings.json
        var sqlSugarOptions = App.GetService<IOptions<SqlSugarOptions>>()?.Value ?? new SqlSugarOptions();
        
        // Include ConfigureExternalServices and MoreSettings for each ConnectionConfig
        foreach (var connectionConfig in connectionConfigs)
        {
            connectionConfig.ConfigureExternalServices = new ConfigureExternalServices()
            {
                EntityService = (x, p) => //处理列名
                {
                    if (p.EntityName.ToLower().EndsWith("dto"))
                        return;

                    p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);
                },
                EntityNameService = (x, p) => //处理表名
                {
                    if (p.EntityName.ToLower().EndsWith("dto"))
                        return;
                    
                    p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);
                }
            };
            
            connectionConfig.MoreSettings = new ConnMoreSettings()
            {
                IsAutoRemoveDataCache = sqlSugarOptions.IsAutoRemoveDataCache,
                PgSqlIsAutoToLower = sqlSugarOptions.PgSqlIsAutoToLower,
                EnableJsonb = sqlSugarOptions.EnableJsonb
            };
        }
        
        var sqlSugarScope = new SqlSugarScope(connectionConfigs, db =>
        {
            if (sqlSugarOptions.LogSqlExecuting)
            {
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    // 集成 Aspire Dashboard 的日志
                    var logger = App.GetService<ILogger<object>>();
                    logger?.LogInformation("SQL Executing: {sql}, Parameters: {parameter}", sql, pars is { Length: > 0} ? string.Join(", ", pars.Select(p => $"{p.ParameterName}={p.Value}")) : "");
                };
            }
            
            // Autofill audit fields
            db.Aop.DataExecuting = (oldValue, entityInfo) =>
            {
                if (entityInfo.EntityValue is AuditEntity auditEntity)
                {
                    var currentUser = "admin"; // TODO: Need to get current user
                    var now = DateTime.UtcNow;
                    
                    if (entityInfo.OperationType == DataFilterType.InsertByObject)
                    {
                        switch (entityInfo.PropertyName)
                        {
                            case "CreatedAt":
                                if ((DateTime)oldValue == default(DateTime))
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
                    }
                    
                    if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                    {
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
                }

            };
        });

        return sqlSugarScope;
    }
}