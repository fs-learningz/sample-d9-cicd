using Note.Application.DTO;
using Note.Application.Wrapper;

namespace Note.Application.Services;

public interface INoteService
{
    Task<List<NoteOutput>> GetNotes();
    Task<Output> GetNote(string externalId);
    Task<Output> CreateNote(NoteInputCreate input);
    Task<Output> UpdateNote(NoteInputUpdate input);
    Task<Output> DeleteNote(string externalId);
}