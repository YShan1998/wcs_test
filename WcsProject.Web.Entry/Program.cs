using Mapster;
using MapsterMapper;
using SqlSugar;
using WcsProject.Application.Database;
using WcsProject.Application.Database.Seeds;
using WcsProject.Application.Modules.StorageUnit.Services;
using WcsProject.Application.Repositories;
using WcsProject.Application.Repositories.StorageUnit;
using WcsProject.Core.Database;
using WcsProject.Core.Options;

Serve.Run(RunOptions.Default
    .ConfigureBuilder(builder =>
    {
        // Add Aspire service defaults
        builder.AddServiceDefaults();
        
        // Add controllers with Furion
        builder.Services.AddControllers()
            .AddInject()
            .AddFriendlyException();
        
        // Add Mapster
        builder.Services.AddMapster();
        
        // Or with assembly scanning for custom mappings
        var applicationAssembly = typeof(StorageUnitService).Assembly;
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(applicationAssembly);

        builder.Services.AddSingleton(config);
        builder.Services.AddScoped<IMapper, ServiceMapper>();
        
        // Register DbInitializer
        builder.Services.AddScoped<IDbInitializer, DbInitializer>();
        
        // Register Options
        builder.Services.Configure<SqlSugarOptions>(builder.Configuration.GetSection(SqlSugarOptions.SectionName));
        
        // Configure SqlSugar with SqlSugarScope (thread-safe singleton)
        // builder.Services.ConfigureSqlSugar(builder.Configuration, builder.Environment);
        builder.Services.AddSingleton<ISqlSugarClient>(DbContext.Instance);
        
        // Register generic repository
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        
        // Register domain-specific repositories
        builder.Services.AddScoped<IStorageUnitRepository, StorageUnitRepository>();
        
        // Register all seeders
        RegisterSeeders(builder.Services);

    })
    .Configure(app =>
    {
        // Initialize database on startup
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            
            // Only auto-initialize in development or if explicitly configured
            if (app.Configuration.GetValue<bool>("DatabaseSettings:AutoInitialize", 
                    app.Environment.IsDevelopment()))
            {
                try
                {
                    dbInitializer.InitializeAsync().Wait();
                    app.Logger.LogInformation("Database initialized successfully");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Database initialization failed");
                    // Don't throw - allow app to start even if DB init fails
                }
            }
        }
        // Map Aspire health checks and endpoints
        app.MapDefaultEndpoints();
        
        // Furion middleware
        app.UseRouting();
        app.UseInject(string.Empty);
        
        // Map controllers
        app.MapControllers();
    }));

static void RegisterSeeders(IServiceCollection services)
{
    var assembly = typeof(IDataSeeder).Assembly;
    
    var seederTypes = assembly.GetTypes()
        .Where(t => 
            t.IsClass && 
            !t.IsAbstract && 
            typeof(IDataSeeder).IsAssignableFrom(t))
        .ToList();
    
    foreach (var seederType in seederTypes)
    {
        services.AddScoped(typeof(IDataSeeder), seederType);
    }
    
    Console.WriteLine($"Registered {seederTypes.Count} data seeders");
}