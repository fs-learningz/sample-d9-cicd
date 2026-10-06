using Microsoft.AspNetCore.Http;
using NSubstitute;
using Note.Application.DTO;
using Note.Application.ExternalServices.Persistence;
using Note.Application.Services.Default;
using Note.Application.Wrapper;
using NoteEntity = Note.Domain.Entities.Note;

namespace Note.Application.Test.Services;

public class NoteServiceReadDeleteTests
{
    private readonly IQueryNote _queryNote = Substitute.For<IQueryNote>();
    private readonly NoteService _sut;

    public NoteServiceReadDeleteTests()
    {
        _sut = new NoteService(_queryNote);
    }

    [Fact]
    public async Task GetNotes_WhenNotesExist_ReturnsMappedOutputs()
    {
        // Arrange
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = created.AddDays(1);
        _queryNote.GetNotes().Returns([
            new NoteEntity { ExternalId = "a", Title = "t1", Text = "x1", CreatedAt = created, UpdatedAt = updated },
            new NoteEntity { ExternalId = "b", Title = null, Text = "x2", CreatedAt = created, UpdatedAt = null },
        ]);

        // Act
        var result = await _sut.GetNotes();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(new NoteOutput("a", "t1", "x1", created, updated), result[0]);
        Assert.Equal(new NoteOutput("b", null, "x2", created, null), result[1]);
    }

    [Fact]
    public async Task GetNotes_WhenNoNotesExist_ReturnsEmptyList()
    {
        // Arrange
        _queryNote.GetNotes().Returns([]);

        // Act
        var result = await _sut.GetNotes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNote_WhenNoteExists_ReturnsOkWithMappedNote()
    {
        // Arrange
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _queryNote.GetNote("a").Returns(new NoteEntity
        {
            ExternalId = "a",
            Title = "t",
            Text = "x",
            CreatedAt = created,
            UpdatedAt = null
        });

        // Act
        var result = await _sut.GetNote("a");

        // Assert
        var ok = Assert.IsType<OutputOk<NoteOutput>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Equal(new NoteOutput("a", "t", "x", created, null), ok.Value);
    }

    [Fact]
    public async Task GetNote_WhenNoteDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _queryNote.GetNote("missing").Returns((NoteEntity?)null);

        // Act
        var result = await _sut.GetNote("missing");

        // Assert
        var error = Assert.IsType<OutputError>(result);
        Assert.Equal(StatusCodes.Status404NotFound, error.Code);
        Assert.Equal("Not Found", error.Error);
        Assert.Equal("The note with external id = missing was not found", error.ErrorDescription);
    }

    [Fact]
    public async Task DeleteNote_WhenRowsDeleted_ReturnsOkWithCount()
    {
        // Arrange
        _queryNote.DeleteNote("a").Returns(1);

        // Act
        var result = await _sut.DeleteNote("a");

        // Assert
        var ok = Assert.IsType<OutputOk<int>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Equal(1, ok.Value);
    }

    [Fact]
    public async Task DeleteNote_WhenNoRowsDeleted_ReturnsNotFoundError()
    {
        // Arrange
        _queryNote.DeleteNote("missing").Returns(0);

        // Act
        var result = await _sut.DeleteNote("missing");

        // Assert
        var error = Assert.IsType<OutputError>(result);
        Assert.Equal(StatusCodes.Status404NotFound, error.Code);
        Assert.Equal("The note with external id = missing was not found", error.ErrorDescription);
    }
}
