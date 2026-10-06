using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Note.Api.Controllers.v1;
using Note.Api.Input;
using Note.Application.DTO;
using Note.Application.Services;
using Note.Application.Wrapper;
using static Note.Api.Test.Controllers.ControllerTestSupport;

namespace Note.Api.Test.Controllers.v1;

public class NotesControllerWriteTests
{
    private readonly INoteService _service = Substitute.For<INoteService>();
    private readonly NotesController _sut;

    public NotesControllerWriteTests()
    {
        _sut = new NotesController(_service);
        UseProblemDetailsFactory(_sut);
    }

    [Fact]
    public async Task Create_WhenServiceSucceeds_ReturnsCreatedWithLocationAndNote()
    {
        // Arrange
        var note = new NoteOutput("new-id", "t", "x", null, null);
        _service.CreateNote(Arg.Any<NoteInputCreate>())
            .Returns(new OutputOk<NoteOutput>(StatusCodes.Status201Created, note));

        // Act
        var result = await _sut.Create(new CreateNoteInput("t", "x"));

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/v1/notes/new-id", created.Location);
        Assert.Equal(note, created.Value);
    }

    [Fact]
    public async Task Create_WhenCalled_PassesRequestFieldsToService()
    {
        // Arrange
        _service.CreateNote(Arg.Any<NoteInputCreate>())
            .Returns(new OutputOk<NoteOutput>(StatusCodes.Status201Created, new NoteOutput("id", "t", "x", null, null)));

        // Act
        await _sut.Create(new CreateNoteInput("title", "text"));

        // Assert
        await _service.Received(1).CreateNote(new NoteInputCreate("title", "text"));
    }

    [Fact]
    public async Task Create_WhenServiceReturnsValidationErrors_ReturnsValidationProblem()
    {
        // Arrange
        var errors = new Dictionary<string, string[]> { ["Title"] = ["required"] };
        _service.CreateNote(Arg.Any<NoteInputCreate>())
            .Returns(new OutputErrorValidation(StatusCodes.Status400BadRequest, errors));

        // Act
        var result = await _sut.Create(new CreateNoteInput("", ""));

        // Assert
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(["required"], problem.Errors["Title"]);
    }

    [Fact]
    public async Task Create_WhenServiceReturnsError_ReturnsProblemWithErrorDetails()
    {
        // Arrange
        _service.CreateNote(Arg.Any<NoteInputCreate>())
            .Returns(new OutputError(StatusCodes.Status409Conflict, "Conflict", "exists"));

        // Act
        var result = await _sut.Create(new CreateNoteInput("t", "x"));

        // Assert
        AssertProblem(result, StatusCodes.Status409Conflict, "Conflict", "exists");
    }

    [Fact]
    public async Task Create_WhenServiceReturnsUnexpectedOutput_ReturnsInternalServerErrorProblem()
    {
        // Arrange
        _service.CreateNote(Arg.Any<NoteInputCreate>()).Returns(new UnknownOutput());

        // Act
        var result = await _sut.Create(new CreateNoteInput("t", "x"));

        // Assert
        AssertUnexpectedOutputProblem(result);
    }

    [Fact]
    public async Task Update_WhenServiceSucceeds_ReturnsOkWithNote()
    {
        // Arrange
        var note = new NoteOutput("a", "t", "x", null, null);
        _service.UpdateNote(Arg.Any<NoteInputUpdate>())
            .Returns(new OutputOk<NoteOutput>(StatusCodes.Status200OK, note));

        // Act
        var result = await _sut.Update("a", new UpdateNoteInput("t", "x"));

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(note, ok.Value);
    }

    [Fact]
    public async Task Update_WhenCalled_PassesRouteIdAndBodyFieldsToService()
    {
        // Arrange
        _service.UpdateNote(Arg.Any<NoteInputUpdate>())
            .Returns(new OutputOk<NoteOutput>(StatusCodes.Status200OK, new NoteOutput("a", "t", "x", null, null)));

        // Act
        await _sut.Update("route-id", new UpdateNoteInput("title", "text"));

        // Assert
        await _service.Received(1).UpdateNote(new NoteInputUpdate("route-id", "title", "text"));
    }

    [Fact]
    public async Task Update_WhenServiceReturnsValidationErrors_ReturnsValidationProblem()
    {
        // Arrange
        var errors = new Dictionary<string, string[]> { ["ExternalId"] = ["required"] };
        _service.UpdateNote(Arg.Any<NoteInputUpdate>())
            .Returns(new OutputErrorValidation(StatusCodes.Status400BadRequest, errors));

        // Act
        var result = await _sut.Update("", new UpdateNoteInput("t", "x"));

        // Assert
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(["required"], problem.Errors["ExternalId"]);
    }

    [Fact]
    public async Task Update_WhenServiceReturnsError_ReturnsProblemWithErrorDetails()
    {
        // Arrange
        _service.UpdateNote(Arg.Any<NoteInputUpdate>())
            .Returns(new OutputError(StatusCodes.Status404NotFound, "Not Found", "missing"));

        // Act
        var result = await _sut.Update("a", new UpdateNoteInput("t", "x"));

        // Assert
        AssertProblem(result, StatusCodes.Status404NotFound, "Not Found", "missing");
    }

    [Fact]
    public async Task Update_WhenServiceReturnsUnexpectedOutput_ReturnsInternalServerErrorProblem()
    {
        // Arrange
        _service.UpdateNote(Arg.Any<NoteInputUpdate>()).Returns(new UnknownOutput());

        // Act
        var result = await _sut.Update("a", new UpdateNoteInput("t", "x"));

        // Assert
        AssertUnexpectedOutputProblem(result);
    }
}
