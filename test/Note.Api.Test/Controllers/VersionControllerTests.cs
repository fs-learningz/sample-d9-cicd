using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Note.Api.Controllers;

namespace Note.Api.Test.Controllers;

public class VersionControllerTests
{
    [Fact]
    public void GetVersion_WhenCalled_ReturnsOkWithAssemblyInformationalVersion()
    {
        // Arrange
        var sut = new VersionController();
        var expected = typeof(VersionController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        // Act
        var result = sut.GetVersion();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var output = Assert.IsType<VersionController.VersionOutput>(ok.Value);
        Assert.Equal(expected, output.Version);
    }

    [Fact]
    public void VersionController_WhenInspected_IsVersionNeutralAndUsesVersionRoute()
    {
        // Arrange
        var type = typeof(VersionController);

        // Act
        var neutral = type.GetCustomAttribute<ApiVersionNeutralAttribute>(inherit: false);
        var route = type.GetCustomAttribute<RouteAttribute>(inherit: false);

        // Assert
        Assert.NotNull(neutral);
        Assert.NotNull(route);
        Assert.Equal("version", route.Template);
    }
}
