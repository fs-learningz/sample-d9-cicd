using Microsoft.AspNetCore.Http;
using NSubstitute;
using Note.Application.DTO;
using Note.Application.ExternalServices.Persistence;
using Note.Application.Services.Default;
using Note.Application.Wrapper;
using NoteEntity = Note.Domain.Entities.Note;

namespace Note.Application.Test.Services;

public class NoteServiceWriteTests
{
    private readonly IQueryNote _queryNote = Substitute.For<IQueryNote>();
    private readonly NoteService _sut;

    public NoteServiceWriteTests()
    {
        _sut = new NoteService(_queryNote);
    }

    [Fact]
    public async Task CreateNote_WhenInputIsInvalid_ReturnsValidationErrorAndDoesNotPersist()
    {
        // Arrange
        var input = new NoteInputCreate("", "");

        // Act
        var result = await _sut.CreateNote(input);

        // Assert
        var error = Assert.IsType<OutputErrorValidation>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, error.Code);
        Assert.Contains("Title", error.Errors.Keys);
        await _queryNote.DidNotReceiveWithAnyArgs().CreateNote(default!);
    }

    [Fact]
    public async Task CreateNote_WhenInputIsValid_PersistsNewNoteWithGeneratedIdAndCreatedAt()
    {
        // Arrange
        NoteEntity? captured = null;
        _queryNote.CreateNote(Arg.Do<NoteEntity>(n => captured = n)).Returns(call => call.Arg<NoteEntity>());
        var before = DateTime.UtcNow;

        // Act
        await _sut.CreateNote(new NoteInputCreate("title", "text"));

        // Assert
        var after = DateTime.UtcNow;
        Assert.NotNull(captured);
        Assert.True(Guid.TryParse(captured.ExternalId, out _));
        Assert.Equal("title", captured.Title);
        Assert.Equal("text", captured.Text);
        Assert.NotNull(captured.CreatedAt);
        Assert.InRange(captured.CreatedAt.Value, before, after);
        Assert.Null(captured.UpdatedAt);
    }

    [Fact]
    public async Task CreateNote_WhenInputIsValid_ReturnsCreatedWithMappedPersistedNote()
    {
        // Arrange
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _queryNote.CreateNote(Arg.Any<NoteEntity>()).Returns(new NoteEntity
        {
            ExternalId = "persisted",
            Title = "t",
            Text = "x",
            CreatedAt = created,
            UpdatedAt = null
        });

        // Act
        var result = await _sut.CreateNote(new NoteInputCreate("title", "text"));

        // Assert
        var ok = Assert.IsType<OutputOk<NoteOutput>>(result);
        Assert.Equal(StatusCodes.Status201Created, ok.Code);
        Assert.Equal(new NoteOutput("persisted", "t", "x", created, null), ok.Value);
    }

    [Fact]
    public async Task UpdateNote_WhenInputIsInvalid_ReturnsValidationErrorAndDoesNotPersist()
    {
        // Arrange
        var input = new NoteInputUpdate("", "title", "text");

        // Act
        var result = await _sut.UpdateNote(input);

        // Assert
        var error = Assert.IsType<OutputErrorValidation>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, error.Code);
        Assert.Contains("ExternalId", error.Errors.Keys);
        await _queryNote.DidNotReceiveWithAnyArgs().UpdateNote(null!);
    }

    [Fact]
    public async Task UpdateNote_WhenInputIsValid_PersistsNoteWithIdAndUpdatedAt()
    {
        // Arrange
        NoteEntity? captured = null;
        _queryNote.UpdateNote(Arg.Do<NoteEntity>(n => captured = n)).Returns(call => call.Arg<NoteEntity>());
        var before = DateTime.UtcNow;

        // Act
        await _sut.UpdateNote(new NoteInputUpdate("id-1", "title", "text"));

        // Assert
        var after = DateTime.UtcNow;
        Assert.NotNull(captured);
        Assert.Equal("id-1", captured.ExternalId);
        Assert.Equal("title", captured.Title);
        Assert.Equal("text", captured.Text);
        Assert.NotNull(captured.UpdatedAt);
        Assert.InRange(captured.UpdatedAt.Value, before, after);
    }

    [Fact]
    public async Task UpdateNote_WhenInputIsValid_ReturnsOkWithMappedPersistedNote()
    {
        // Arrange
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = created.AddDays(1);
        _queryNote.UpdateNote(Arg.Any<NoteEntity>()).Returns(new NoteEntity
            { ExternalId = "id-1", Title = "t", Text = "x", CreatedAt = created, UpdatedAt = updated });

        // Act
        var result = await _sut.UpdateNote(new NoteInputUpdate("id-1", "title", "text"));

        // Assert
        var ok = Assert.IsType<OutputOk<NoteOutput>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Equal(new NoteOutput("id-1", "t", "x", created, updated), ok.Value);
    }
}
