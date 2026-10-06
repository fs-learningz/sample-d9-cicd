using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Note.Api.Handlers;
using Note.Application.ExternalServices.Persistence;
using System.Text.Encodings.Web;
using ApplicationEntity = Note.Domain.Entities.Application;
using ApplicationCredentialEntity = Note.Domain.Entities.ApplicationCredential;

namespace Note.Api.Test.Handlers;

public class HmacAuthenticationHandlerTests
{
    private const string Application = "app";
    private const string Credential = "secret";
    private const string Nonce = "nonce-1";
    private const string Body = "{\"title\":\"t\"}";

    private readonly IQueryApplication _queryApplication = Substitute.For<IQueryApplication>();
    private readonly IQueryHmacNonce _queryHmacNonce = Substitute.For<IQueryHmacNonce>();

    public HmacAuthenticationHandlerTests()
    {
        _queryHmacNonce.TryClaim(Arg.Any<string>()).Returns(true);
        _queryApplication.GetApplicationPlusCredential(Application).Returns(new ApplicationEntity
        {
            ExternalId = Application,
            Name = "My App",
            Credential = new ApplicationCredentialEntity { Credential = Credential },
        });
    }

    private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private static string Sign(long timestamp, string nonce = Nonce, string body = Body)
        => new HmacSignature(Credential).Create(Application, timestamp, nonce, body);

    private static DefaultHttpContext CreateContext(string? timestamp, string? nonce, string? application,
        string? hmac, string body = Body)
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        if (timestamp is not null) context.Request.Headers["X-Timestamp"] = timestamp;
        if (nonce is not null) context.Request.Headers["X-Nonce"] = nonce;
        if (application is not null) context.Request.Headers["X-Application"] = application;
        if (hmac is not null) context.Request.Headers["X-HMAC"] = hmac;
        return context;
    }

    private static DefaultHttpContext CreateValidContext()
    {
        var timestamp = Now();
        return CreateContext(timestamp.ToString(), Nonce, Application, Sign(timestamp));
    }

    private async Task<HmacAuthenticationHandler> CreateHandler(HttpContext context)
    {
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Get(Arg.Any<string?>()).Returns(new AuthenticationSchemeOptions());
        var handler = new HmacAuthenticationHandler(options, NullLoggerFactory.Instance, UrlEncoder.Default,
            _queryApplication, _queryHmacNonce);
        await handler.InitializeAsync(
            new AuthenticationScheme(HmacAuthenticationHandler.Scheme, null, typeof(HmacAuthenticationHandler)),
            context);
        return handler;
    }

    private async Task<AuthenticateResult> Authenticate(HttpContext context)
    {
        var handler = await CreateHandler(context);
        return await handler.AuthenticateAsync();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var context = CreateValidContext();

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Ticket);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenRequestIsValid_ResetsBodyPositionToStart()
    {
        // Arrange
        var context = CreateValidContext();

        // Act
        await Authenticate(context);

        // Assert
        Assert.Equal(0, context.Request.Body.Position);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenTimestampHeaderIsMissing_ReturnsFailure()
    {
        // Arrange
        var context = CreateContext(null, Nonce, Application, Sign(Now()));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to find the X-Timestamp header", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenTimestampIsNotANumber_ReturnsFailure()
    {
        // Arrange
        var context = CreateContext("abc", Nonce, Application, Sign(Now()));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to parse the X-Timestamp value", result.Failure?.Message);
    }

    [Theory]
    [InlineData(-3600)]
    [InlineData(3600)]
    public async Task HandleAuthenticateAsync_WhenTimestampIsOutsideAllowedWindow_ReturnsFailure(long offsetSeconds)
    {
        // Arrange
        var timestamp = Now() + offsetSeconds;
        var context = CreateContext(timestamp.ToString(), Nonce, Application, Sign(timestamp));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: This request is already expired", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenNonceHeaderIsMissing_ReturnsFailure()
    {
        // Arrange
        var timestamp = Now();
        var context = CreateContext(timestamp.ToString(), null, Application, Sign(timestamp));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to find the X-Nonce header", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenNonceIsBlank_ReturnsFailure()
    {
        // Arrange
        var timestamp = Now();
        var context = CreateContext(timestamp.ToString(), "   ", Application, Sign(timestamp));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to find the X-Nonce value", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenNonceWasAlreadyClaimed_ReturnsFailure()
    {
        // Arrange
        _queryHmacNonce.TryClaim(Nonce).Returns(false);
        var context = CreateValidContext();

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: The request is already processed", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenApplicationHeaderIsMissing_ReturnsFailure()
    {
        // Arrange
        var timestamp = Now();
        var context = CreateContext(timestamp.ToString(), Nonce, null, Sign(timestamp));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to find the X-Application header", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenApplicationIsUnknown_ReturnsFailure()
    {
        // Arrange
        var timestamp = Now();
        var context = CreateContext(timestamp.ToString(), Nonce, "unknown", Sign(timestamp));
        _queryApplication.GetApplicationPlusCredential("unknown").Returns((ApplicationEntity?)null);

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Invalid X-Application header value", result.Failure?.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAuthenticateAsync_WhenApplicationHasNoUsableCredential_ReturnsFailure(bool emptyCredential)
    {
        // Arrange
        _queryApplication.GetApplicationPlusCredential(Application).Returns(new ApplicationEntity
        {
            ExternalId = Application,
            Name = "My App",
            Credential = emptyCredential ? new ApplicationCredentialEntity { Credential = "" } : null,
        });
        var context = CreateValidContext();

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Invalid X-Application header value", result.Failure?.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task HandleAuthenticateAsync_WhenHmacHeaderIsMissingOrBlank_ReturnsFailure(string? hmac)
    {
        // Arrange
        var context = CreateContext(Now().ToString(), Nonce, Application, hmac);

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: Unable to find the X-HMAC header", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenSignatureDoesNotMatch_ReturnsFailure()
    {
        // Arrange
        var timestamp = Now();
        var context = CreateContext(timestamp.ToString(), Nonce, Application, Sign(timestamp, body: "tampered"));

        // Act
        var result = await Authenticate(context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error: The request doesn't match the HMAC value", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleChallengeAsync_WhenChallenged_SetsUnauthorizedStatusAndWwwAuthenticateHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var handler = await CreateHandler(context);

        // Act
        await handler.ChallengeAsync(new AuthenticationProperties());

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("X-HMAC", context.Response.Headers.WWWAuthenticate.ToString());
    }
}
