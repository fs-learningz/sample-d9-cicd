using Microsoft.AspNetCore.Http;
using NSubstitute;
using Note.Application.DTO;
using Note.Application.ExternalServices.Persistence;
using Note.Application.Services.Default;
using Note.Application.Wrapper;
using ApplicationEntity = Note.Domain.Entities.Application;
using ApplicationCredentialEntity = Note.Domain.Entities.ApplicationCredential;

namespace Note.Application.Test.Services;

public class ApplicationServiceTests
{
    private static readonly DateTime Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly IQueryApplication _queryApplication = Substitute.For<IQueryApplication>();
    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _sut = new ApplicationService(_queryApplication);
    }

    [Fact]
    public async Task GetApplication_WhenApplicationExists_ReturnsOkWithMappedApplication()
    {
        // Arrange
        _queryApplication.GetApplication("app").Returns(
            new ApplicationEntity { ExternalId = "app", Name = "My App", CreatedAt = Created });

        // Act
        var result = await _sut.GetApplication("app");

        // Assert
        var ok = Assert.IsType<OutputOk<ApplicationOutput>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Equal(new ApplicationOutput("app", "My App", Created), ok.Value);
    }

    [Fact]
    public async Task GetApplication_WhenApplicationDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _queryApplication.GetApplication("missing").Returns((ApplicationEntity?)null);

        // Act
        var result = await _sut.GetApplication("missing");

        // Assert
        var error = Assert.IsType<OutputError>(result);
        Assert.Equal(StatusCodes.Status404NotFound, error.Code);
        Assert.Equal("Not Found", error.Error);
        Assert.Equal("The application with external id = missing was not found", error.ErrorDescription);
    }

    [Fact]
    public async Task GetApplicationPlusCredential_WhenApplicationDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _queryApplication.GetApplicationPlusCredential("missing").Returns((ApplicationEntity?)null);

        // Act
        var result = await _sut.GetApplicationPlusCredential("missing");

        // Assert
        var error = Assert.IsType<OutputError>(result);
        Assert.Equal(StatusCodes.Status404NotFound, error.Code);
        Assert.Equal("The application with external id = missing was not found", error.ErrorDescription);
    }

    [Fact]
    public async Task GetApplicationPlusCredential_WhenCredentialExists_ReturnsOkWithMappedCredential()
    {
        // Arrange
        var credentialCreated = Created.AddDays(1);
        _queryApplication.GetApplicationPlusCredential("app").Returns(new ApplicationEntity
        {
            ExternalId = "app",
            Name = "My App",
            CreatedAt = Created,
            Credential = new ApplicationCredentialEntity { Credential = "secret", CreatedAt = credentialCreated },
        });

        // Act
        var result = await _sut.GetApplicationPlusCredential("app");

        // Assert
        var ok = Assert.IsType<OutputOk<ApplicationOutputPlusCredential>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Equal(
            new ApplicationOutputPlusCredential("app", "My App", Created,
                new ApplicationCredentialOutput("secret", credentialCreated)),
            ok.Value);
    }

    [Fact]
    public async Task GetApplicationPlusCredential_WhenCredentialIsMissing_ReturnsOkWithNullCredential()
    {
        // Arrange
        _queryApplication.GetApplicationPlusCredential("app").Returns(
            new ApplicationEntity { ExternalId = "app", Name = "My App", CreatedAt = Created, Credential = null });

        // Act
        var result = await _sut.GetApplicationPlusCredential("app");

        // Assert
        var ok = Assert.IsType<OutputOk<ApplicationOutputPlusCredential>>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.Code);
        Assert.Null(ok.Value.Credential);
        Assert.Equal("app", ok.Value.ExternalId);
    }
}
