using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Note.Api.Controllers.v1;
using Note.Application.DTO;
using Note.Application.Services;
using Note.Application.Wrapper;
using static Note.Api.Test.Controllers.ControllerTestSupport;

namespace Note.Api.Test.Controllers.v1;

public class NotesControllerReadDeleteTests
{
    private readonly INoteService _service = Substitute.For<INoteService>();
    private readonly NotesController _sut;

    public NotesControllerReadDeleteTests()
    {
        _sut = new NotesController(_service);
        UseProblemDetailsFactory(_sut);
    }

    [Fact]
    public async Task GetNotes_WhenCalled_ReturnsOkWithNotes()
    {
        // Arrange
        List<NoteOutput> notes = [new("a", "t", "x", null, null)];
        _service.GetNotes().Returns(notes);

        // Act
        var result = await _sut.GetNotes();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(notes, ok.Value);
    }

    [Fact]
    public async Task GetNote_WhenServiceReturnsNote_ReturnsOkWithNote()
    {
        // Arrange
        var note = new NoteOutput("a", "t", "x", null, null);
        _service.GetNote("a").Returns(new OutputOk<NoteOutput>(StatusCodes.Status200OK, note));

        // Act
        var result = await _sut.GetNote("a");

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(note, ok.Value);
    }

    [Fact]
    public async Task GetNote_WhenServiceReturnsError_ReturnsProblemWithErrorDetails()
    {
        // Arrange
        _service.GetNote("a").Returns(new OutputError(StatusCodes.Status404NotFound, "Not Found", "missing"));

        // Act
        var result = await _sut.GetNote("a");

        // Assert
        AssertProblem(result, StatusCodes.Status404NotFound, "Not Found", "missing");
    }

    [Fact]
    public async Task GetNote_WhenServiceReturnsUnexpectedOutput_ReturnsInternalServerErrorProblem()
    {
        // Arrange
        _service.GetNote("a").Returns(new UnknownOutput());

        // Act
        var result = await _sut.GetNote("a");

        // Assert
        AssertUnexpectedOutputProblem(result);
    }

    [Fact]
    public async Task Delete_WhenServiceReturnsOk_ReturnsNoContent()
    {
        // Arrange
        _service.DeleteNote("a").Returns(new OutputOk<int>(StatusCodes.Status200OK, 1));

        // Act
        var result = await _sut.Delete("a");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenServiceReturnsError_ReturnsProblemWithErrorDetails()
    {
        // Arrange
        _service.DeleteNote("a").Returns(new OutputError(StatusCodes.Status404NotFound, "Not Found", "missing"));

        // Act
        var result = await _sut.Delete("a");

        // Assert
        AssertProblem(result, StatusCodes.Status404NotFound, "Not Found", "missing");
    }

    [Fact]
    public async Task Delete_WhenServiceReturnsUnexpectedOutput_ReturnsInternalServerErrorProblem()
    {
        // Arrange
        _service.DeleteNote("a").Returns(new UnknownOutput());

        // Act
        var result = await _sut.Delete("a");

        // Assert
        AssertUnexpectedOutputProblem(result);
    }
}
