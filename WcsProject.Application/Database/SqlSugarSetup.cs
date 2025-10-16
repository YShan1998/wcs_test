using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using WcsProject.Application.Database.DbScopes;
using WcsProject.Core.Entities;

namespace WcsProject.Application.Database;

public class SqlSugarSetup
{
    // Seed data?
}

public static class SqlSugarConfig
{
    public static void ConfigureSqlSugar(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // 如果多个数数据库传 List<ConnectionConfig>
        var configConnection = new ConnectionConfig()
        {
            DbType = DbType.PostgreSQL,
            ConfigId = DefaultDbScope.ConfigId,
            ConnectionString = configuration.GetConnectionString(DefaultDbScope.ConfigName),
            IsAutoCloseConnection = true,
            
            // CodeFirst settings
            InitKeyType = InitKeyType.Attribute,
            
            // Entity configuration
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                EntityService = (x, p) => //处理列名
                {
                    //要排除DTO类，不然MergeTable会有问题
                    p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);
                },
                EntityNameService = (x, p) => //处理表名
                {
                    //最好排除DTO类
                    p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);
                }
            },
            
            // More settings
            MoreSettings = new ConnMoreSettings()
            {
                IsAutoRemoveDataCache = true,
                PgSqlIsAutoToLower = false,
                EnableJsonb = true
            }
        };

        SqlSugarScope sqlSugar = new SqlSugarScope(configConnection,
            db =>
            {
                ConfigurePostgreSQLExtensions(db);
                
                if (environment.IsDevelopment())
                {
                    // Before executing SQL
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine($"SQL: {sql}");
                        if (pars is { Length: > 0 })
                        {
                            Console.WriteLine(
                                $"Parameters: {string.Join(", ", pars.Select(p => $"{p.ParameterName}={p.Value}"))}");
                        }
                    };
                }
                
                // Auto-fill audit fields
                db.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    Console.WriteLine($"AOP DataExecuting: OperationType: {entityInfo.OperationType}, Entity: {entityInfo.EntityValue?.GetType().Name}, Property: {entityInfo.PropertyName}");
                    
                    if (entityInfo.EntityValue is AuditEntity auditEntity)
                    {
                        var currentUser = "admin"; // TODO: Need to get current user
                        var now = DateTime.UtcNow;
                        
                        if (entityInfo.OperationType == DataFilterType.InsertByObject)
                        {
                            Console.WriteLine($"Setting audit fields for INSERT: {entityInfo.EntityValue.GetType().Name}");
                            
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
                            Console.WriteLine($"Setting audit fields for UPDATE: {entityInfo.EntityValue.GetType().Name}");
                            
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

        services.AddSingleton<ISqlSugarClient>(sqlSugar);   // 这边是SqlSugarScope用AddSingleton
    }

    private static void ConfigurePostgreSQLExtensions(ISqlSugarClient db)
    {
        try
        {
            // Enable UUID extension for PostgreSQL
            db.Ado.ExecuteCommand("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
            Console.WriteLine("PostgreSQL pgcrypto extension enabled for UUID support");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not enable pgcrypto extension: {ex.Message}");
            // Continue anyway, as the extension might already be enabled
        }
    }
}