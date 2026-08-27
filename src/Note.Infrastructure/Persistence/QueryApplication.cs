using Dapper;
using Microsoft.Data.Sqlite;
using Note.Application.ExternalServices.Persistence;
using Note.Domain.Entities;

namespace Note.Infrastructure.Persistence;

public class QueryApplication : IQueryApplication
{
    private readonly SqliteConnection _connection;

    public QueryApplication(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<Domain.Entities.Application?> GetApplication(string externalId)
    {
        var command = new CommandDefinition("""
                                            select * from application
                                            where external_id = @ExternalId
                                            limit 1;
                                            """,
            new
            {
                ExternalId = externalId
            });

        return await _connection.QuerySingleOrDefaultAsync<Domain.Entities.Application>(command);
    }

    public async Task<Domain.Entities.Application?> GetApplicationPlusCredential(string externalId)
    {
        var command = new CommandDefinition("""
                                            select a.*, c.*
                                            from application a
                                            left join application_credentials c 
                                                on c.application_id = a.Id
                                            where a.external_id = @ExternalId
                                            limit 1;
                                            """,
            new
            {
                ExternalId = externalId
            });

        var applications = await _connection
            .QueryAsync<Domain.Entities.Application, ApplicationCredential?, Domain.Entities.Application>(
                command,
                (application, credential) =>
                {
                    if (credential is { Id: > 0 })
                    {
                        application.Credential = credential;
                    }

                    return application;
                },
                splitOn: "Id");

        return applications.FirstOrDefault();
    }
}