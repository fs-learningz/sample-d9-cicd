using Microsoft.AspNetCore.Mvc;
using Note.Api.Input;
using Note.Application.DTO;
using Note.Application.Services;
using Note.Application.Wrapper;

namespace Note.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notes")]
[Produces("application/json")]
public sealed class NotesController : ControllerBase
{
    private readonly INoteService _service;

    public NotesController(INoteService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NoteOutput>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetNotes()
    {
        return Ok(await _service.GetNotes());
    }

    [HttpGet("{externalId}")]
    [ProducesResponseType(typeof(NoteOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetNote(string externalId)
    {
        var item = await _service.GetNote(externalId);
        
        if (item is OutputError error)
        {
            return Problem(statusCode: error.Code, title: error.Error, detail: error.ErrorDescription);
        }

        if (item is not OutputOk<NoteOutput> fine)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.");
        }

        return Ok(fine.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NoteOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateNoteInput request)
    {
        var item = await _service.CreateNote(new NoteInputCreate(request.Title, request.Text));

        if (item is OutputErrorValidation errorValidation)
        {
            return ValidationProblem(new ValidationProblemDetails(errorValidation.Errors)
            {
                Status = errorValidation.Code
            });
        }

        if (item is OutputError error)
        {
            return Problem(statusCode: error.Code, title: error.Error, detail: error.ErrorDescription);
        }

        if (item is not OutputOk<NoteOutput> fine)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.");
        }

        return Created("/api/v1/notes", fine.Value);
    }

    [HttpPut("{externalId}")]
    [ProducesResponseType(typeof(NoteOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(string externalId, UpdateNoteInput request)
    {
        var item = await _service.UpdateNote(new NoteInputUpdate(externalId, request.Title, request.Text));
        
        if (item is OutputErrorValidation errorValidation)
        {
            return ValidationProblem(new ValidationProblemDetails(errorValidation.Errors)
            {
                Status = errorValidation.Code
            });
        }

        if (item is OutputError error)
        {
            return Problem(statusCode: error.Code, title: error.Error, detail: error.ErrorDescription);
        }

        if (item is not OutputOk<NoteOutput> fine)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.");
        }

        return Ok(fine.Value);
    }

    [HttpDelete("{externalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(string externalId)
    {
        var item = await _service.DeleteNote(externalId);
        
        if (item is OutputError error)
        {
            return Problem(statusCode: error.Code, title: error.Error, detail: error.ErrorDescription);
        }

        if (item is not OutputOk<int>)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.");
        }

        return NoContent();
    }
}