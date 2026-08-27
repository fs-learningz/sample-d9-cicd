using Note.Application.DTO;
using Note.Application.Validator.Abstraction;

namespace Note.Application.Validator;

public class NoteUpdateValidate : IValidator<NoteInputUpdate>
{
    private const int MaxLengthTile = 200;
    private const int MaxLengthText = 10_000;

    public Dictionary<string, string[]> Validate(NoteInputUpdate input)
    {
        var errors = new Dictionary<string, string[]>();

        if (input.ExternalId is null or "")
        {
            errors[nameof(NoteInputUpdate.ExternalId)] = ["The external id is required."];
        }

        if (input.Title is null or "" && input.Text is null or "")
        {
            errors[nameof(NoteInputCreate.Title)] = ["Either a title or a text must be provided."];
        }

        if (input.Title is { Length: > MaxLengthTile })
        {
            errors[nameof(NoteInputCreate.Title)] = [$"The title must be {MaxLengthTile} characters or fewer."];
        }

        if (input.Text is { Length: > MaxLengthText })
        {
            errors[nameof(NoteInputCreate.Text)] = [$"The text must be {MaxLengthText} characters or fewer."];
        }

        return errors;
    }
}