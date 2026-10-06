using Note.Application.DTO;
using Note.Application.Validator;

namespace Note.Application.Test.Validator;

public class NoteUpdateValidateTests
{
    private readonly NoteUpdateValidate _sut = new NoteUpdateValidate();

    [Theory]
    [InlineData("title", "text")]
    [InlineData("title", null)]
    [InlineData("title", "")]
    [InlineData(null, "text")]
    [InlineData("", "text")]
    public void Validate_WhenIdAndTitleOrTextProvided_ReturnsNoErrors(string? title, string? text)
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", title!, text!);

        // Act
        var errors = _sut.Validate(input);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenExternalIdIsEmpty_ReturnsExternalIdError(string? externalId)
    {
        // Arrange
        var input = new NoteInputUpdate(externalId!, "title", "text");

        // Act
        var errors = _sut.Validate(input);

        // Assert
        var error = Assert.Single(errors);
        Assert.Equal("ExternalId", error.Key);
        Assert.Equal(["The external id is required."], error.Value);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("", null)]
    public void Validate_WhenTitleAndTextAreEmpty_ReturnsTitleError(string? title, string? text)
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", title!, text!);

        // Act
        var errors = _sut.Validate(input);

        // Assert
        var error = Assert.Single(errors);
        Assert.Equal("Title", error.Key);
        Assert.Equal(["Either a title or a text must be provided."], error.Value);
    }

    [Fact]
    public void Validate_WhenExternalIdAndTitleAndTextAreEmpty_ReturnsBothErrors()
    {
        // Arrange
        var input = new NoteInputUpdate("", "", "");

        // Act
        var errors = _sut.Validate(input);

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Contains("ExternalId", errors.Keys);
        Assert.Contains("Title", errors.Keys);
    }

    [Fact]
    public void Validate_WhenTitleIsExactlyMaxLength_ReturnsNoErrors()
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", new string('a', 200), "text");

        // Act
        var errors = _sut.Validate(input);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WhenTitleExceedsMaxLength_ReturnsTitleError()
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", new string('a', 201), "text");

        // Act
        var errors = _sut.Validate(input);

        // Assert
        var error = Assert.Single(errors);
        Assert.Equal("Title", error.Key);
        Assert.Equal(["The title must be 200 characters or fewer."], error.Value);
    }

    [Fact]
    public void Validate_WhenTextIsExactlyMaxLength_ReturnsNoErrors()
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", "title", new string('a', 10_000));

        // Act
        var errors = _sut.Validate(input);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WhenTextExceedsMaxLength_ReturnsTextError()
    {
        // Arrange
        var input = new NoteInputUpdate("id-1", "title", new string('a', 10_001));

        // Act
        var errors = _sut.Validate(input);

        // Assert
        var error = Assert.Single(errors);
        Assert.Equal("Text", error.Key);
        Assert.Equal(["The text must be 10000 characters or fewer."], error.Value);
    }
}
