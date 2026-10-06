using Microsoft.Extensions.DependencyInjection;
using Note.Application.Services;
using Note.Application.Services.Default;

namespace Note.Application.Test;

public class DependencyInjectionTests
{
    [Theory]
    [InlineData(typeof(INoteService), typeof(NoteService))]
    [InlineData(typeof(IApplicationService), typeof(ApplicationService))]
    public void AddNoteApplication_WhenCalled_RegistersServiceAsScoped(Type service, Type implementation)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNoteApplication();

        // Assert
        var descriptor = Assert.Single(services, d => d.ServiceType == service);
        Assert.Equal(implementation, descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddNoteApplication_WhenCalled_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNoteApplication();

        // Assert
        Assert.Same(services, result);
    }
}
