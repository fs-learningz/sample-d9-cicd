using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Note.Api.Handlers;
using NoteV1Controller = Note.Api.Controllers.v1.NotesController;
using NoteV2Controller = Note.Api.Controllers.v2.NotesController;

namespace Note.Api.Test.Controllers.v2;

public class NotesControllerTests
{
    [Fact]
    public void NotesController_WhenInspected_InheritsFromV1Controller()
    {
        // Arrange
        var type = typeof(NoteV2Controller);

        // Act
        var baseType = type.BaseType;

        // Assert
        Assert.Equal(typeof(NoteV1Controller), baseType);
    }

    [Fact]
    public void NotesController_WhenInspected_RequiresHmacAuthenticationScheme()
    {
        // Arrange
        var type = typeof(NoteV2Controller);

        // Act
        var authorize = type.GetCustomAttribute<AuthorizeAttribute>(inherit: false);

        // Assert
        Assert.NotNull(authorize);
        Assert.Equal(HmacAuthenticationHandler.Scheme, authorize.AuthenticationSchemes);
    }

    [Fact]
    public void NotesController_WhenInspected_DeclaresApiVersion2()
    {
        // Arrange
        var type = typeof(NoteV2Controller);

        // Act
        var versions = type.GetCustomAttributes<ApiVersionAttribute>(inherit: false)
            .SelectMany(x => x.Versions)
            .ToList();

        // Assert
        Assert.Equal(new ApiVersion(2, 0), Assert.Single(versions));
    }

    [Fact]
    public void NotesController_WhenInspected_UsesVersionedNotesRoute()
    {
        // Arrange
        var type = typeof(NoteV2Controller);

        // Act
        var route = type.GetCustomAttribute<RouteAttribute>(inherit: false);

        // Assert
        Assert.NotNull(route);
        Assert.Equal("api/v{version:apiVersion}/notes", route.Template);
    }
}
