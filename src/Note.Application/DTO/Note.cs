
namespace Note.Application.DTO;

public sealed record NoteOutput(string ExternalId, string? Title, string? Text, DateTime? CreatedAt, DateTime? UpdatedAt);
public sealed record NoteInputCreate(string Title, string Text);
public sealed record NoteInputUpdate(string ExternalId, string Title, string Text);
