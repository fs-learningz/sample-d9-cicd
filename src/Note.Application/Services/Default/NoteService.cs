using Microsoft.AspNetCore.Http;
using Note.Application.DTO;
using Note.Application.ExternalServices.Persistence;
using Note.Application.Validator;
using Note.Application.Wrapper;

namespace Note.Application.Services.Default;

public class NoteService : INoteService
{
    private readonly IQueryNote _queryNote;

    public NoteService(IQueryNote queryNote)
    {
        _queryNote = queryNote;
    }

    public async Task<List<NoteOutput>> GetNotes()
    {
        var notes = await _queryNote.GetNotes();

        return notes
            .Select(x => new NoteOutput(x.ExternalId, x.Title, x.Text, x.CreatedAt, x.UpdatedAt))
            .ToList();
    }

    public async Task<Output> GetNote(string externalId)
    {
        var note = await _queryNote.GetNote(externalId);

        if (note is null)
        {
            return new OutputError(StatusCodes.Status404NotFound, "Not Found",
                $"The note with external id = {externalId} was not found");
        }

        return new OutputOk<NoteOutput>(StatusCodes.Status200OK,
            new NoteOutput(note.ExternalId, note.Title, note.Text, note.CreatedAt, note.UpdatedAt));
    }

    public async Task<Output> CreateNote(NoteInputCreate input)
    {
        var validator = new NoteCreateValidate();
        var validatorErrors = validator.Validate(input);

        if (validatorErrors.Count > 0)
        {
            return new OutputErrorValidation(StatusCodes.Status400BadRequest, validatorErrors);
        }

        var note = new Domain.Entities.Note
        {
            ExternalId = Guid.NewGuid().ToString(),
            Title = input.Title,
            Text = input.Text,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
        };
        var item = await _queryNote.CreateNote(note);

        return new OutputOk<NoteOutput>(StatusCodes.Status201Created,
            new NoteOutput(item.ExternalId, item.Title, item.Text, item.CreatedAt, item.UpdatedAt));
    }

    public async Task<Output> UpdateNote(NoteInputUpdate input)
    {
        var validator = new NoteUpdateValidate();
        var validatorErrors = validator.Validate(input);

        if (validatorErrors.Count > 0)
        {
            return new OutputErrorValidation(StatusCodes.Status400BadRequest, validatorErrors);
        }

        var note = new Domain.Entities.Note
        {
            ExternalId = input.ExternalId,
            Title = input.Title,
            Text = input.Text,
            UpdatedAt = DateTime.UtcNow,
        };
        var item = await _queryNote.UpdateNote(note);

        return new OutputOk<NoteOutput>(StatusCodes.Status200OK,
            new NoteOutput(item.ExternalId, item.Title, item.Text, item.CreatedAt, item.UpdatedAt));
    }

    public async Task<Output> DeleteNote(string externalId)
    {
        var item = await _queryNote.DeleteNote(externalId);

        if (item is 0)
        {
            return new OutputError(StatusCodes.Status404NotFound, "Not Found",
                $"The note with external id = {externalId} was not found");
        }

        return new OutputOk<int>(StatusCodes.Status200OK, item);
    }
}