using Dapper;
using Microsoft.Data.Sqlite;
using Note.Application.ExternalServices.Persistence;

namespace Note.Infrastructure.Persistence;

public class QueryNote : IQueryNote
{
    private readonly SqliteConnection _connection;

    public QueryNote(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<Domain.Entities.Note>> GetNotes()
    {
        var command = new CommandDefinition("""
                                            select * from note;
                                            """);

        return (await _connection.QueryAsync<Domain.Entities.Note>(command))
            .ToList();
    }

    public async Task<Domain.Entities.Note?> GetNote(string externalId)
    {
        var command = new CommandDefinition("""
                                            select * from note
                                            where external_id = @ExternalId
                                            limit 1;
                                            """,
            new
            {
                ExternalId = externalId
            });

        return await _connection.QuerySingleOrDefaultAsync<Domain.Entities.Note>(command);
    }

    public async Task<Domain.Entities.Note> CreateNote(Domain.Entities.Note note)
    {
        var command = new CommandDefinition("""
                                            insert into note (external_id, title, text, created_at, updated_at)
                                            values (@ExternalId, @Title, @Text, @CreatedAt, @UpdatedAt)
                                            returning *;
                                            """, note);

        return await _connection.QuerySingleAsync<Domain.Entities.Note>(command);
    }

    public async Task<Domain.Entities.Note> UpdateNote(Domain.Entities.Note note)
    {
        var command = new CommandDefinition("""
                                            update note
                                            set title = @Title,
                                                text = @Text,
                                                updated_at = @UpdatedAt
                                            where external_id = @ExternalId
                                            returning *;
                                            """, note);

        return await _connection.QuerySingleOrDefaultAsync<Domain.Entities.Note>(command) ?? note;
    }

    public async Task<int> DeleteNote(string externalId)
    {
        var command = new CommandDefinition("""
                                            delete from note
                                            where external_id = @ExternalId;
                                            """,
            new
            {
                ExternalId = externalId
            });

        return await _connection.ExecuteAsync(command);
    }
}