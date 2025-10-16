var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", port: 5632)
    .WithDataVolume()
    .WithPgAdmin();

var defaultDb = postgres.AddDatabase("defaultdb");

builder.AddProject<Projects.WcsProject_Web_Entry>("api")
    .WithReference(defaultDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();