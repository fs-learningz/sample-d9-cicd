namespace Note.Application.ExternalServices.Persistence;

public interface IQueryNote
{
    Task<List<Domain.Entities.Note>> GetNotes();
    Task<Domain.Entities.Note?> GetNote(string externalId);
    Task<Domain.Entities.Note> CreateNote(Domain.Entities.Note note);
    Task<Domain.Entities.Note> UpdateNote(Domain.Entities.Note note);
    Task<int> DeleteNote(string externalId);
}