using Note.Application.Wrapper;

namespace Note.Application.Test.Wrapper;

public class OutputTests
{
    [Fact]
    public void OutputOk_WhenCreated_StoresCodeAndValue()
    {
        // Arrange & Act
        var output = new OutputOk<string>(200, "value");

        // Assert
        Assert.Equal(200, output.Code);
        Assert.Equal("value", output.Value);
    }

    [Fact]
    public void OutputError_WhenCreated_StoresCodeErrorAndDescription()
    {
        // Arrange & Act
        var output = new OutputError(404, "Not Found", "description");

        // Assert
        Assert.Equal(404, output.Code);
        Assert.Equal("Not Found", output.Error);
        Assert.Equal("description", output.ErrorDescription);
    }

    [Fact]
    public void OutputErrorValidation_WhenCreated_StoresCodeAndErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]> { ["Title"] = ["required"] };

        // Act
        var output = new OutputErrorValidation(400, errors);

        // Assert
        Assert.Equal(400, output.Code);
        Assert.Same(errors, output.Errors);
    }
}
