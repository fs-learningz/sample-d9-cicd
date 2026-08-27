using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite("sqlite", Path.Combine(builder.AppHostDirectory, "data"), "sample.db")
    .WithSqliteWeb(static x =>
    {
        x.WithHttpEndpoint(port: 10999);
    });

var apiNote = builder.AddProject<Note_Api>("api-note")
    .WithExternalHttpEndpoints()
    .WithHttpEndpoint(port: 10101)
    .WithHttpsEndpoint(port: 10102)
    .WithReference(sqlite)
    .WaitFor(sqlite);

builder
    .Build()
    .Run();