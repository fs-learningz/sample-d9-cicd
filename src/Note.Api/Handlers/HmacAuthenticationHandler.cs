using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Note.Application.ExternalServices.Persistence;
using Note.Domain.Entities;

namespace Note.Api.Handlers;

public class HmacAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "Hmac";

    private readonly IQueryApplication _queryApplication;
    private readonly IQueryHmacNonce _queryHmacNonce;

    public HmacAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IQueryApplication queryApplication,
        IQueryHmacNonce queryHmacNonce) :
        base(options, logger, encoder)
    {
        _queryApplication = queryApplication;
        _queryHmacNonce = queryHmacNonce;
    }

    [Obsolete("Obsolete")]
    public HmacAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IQueryApplication queryApplication,
        IQueryHmacNonce queryHmacNonce) : base(options, logger, encoder, clock)
    {
        _queryApplication = queryApplication;
        _queryHmacNonce = queryHmacNonce;
    }

    /// <summary>
    /// Structure
    ///     "APPLICATION":"TIMESTAMP":"NONCE":"DATA"
    /// </summary>
    /// <returns></returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var (okTimestamp, errorTimestamp, timestamp) = CheckTimestamp();

        if (!okTimestamp)
        {
            return AuthenticateResult.Fail(errorTimestamp!);
        }

        var (okNonce, errorNonce, nonce) = await CheckNonce();

        if (!okNonce)
        {
            return AuthenticateResult.Fail(errorNonce!);
        }

        var (okKey, errorKey, application, credential) = await CheckApplication();

        if (!okKey)
        {
            return AuthenticateResult.Fail(errorKey!);
        }

        var (okHmac, errorHmac) = await CheckHmac(timestamp ?? 0, nonce!, application!, credential!);

        if (!okHmac)
        {
            return AuthenticateResult.Fail(errorHmac!);
        }

        return AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity([], "HMAC")),
            new AuthenticationProperties(),
            "HMAC"));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.Append("WWW-Authenticate", "X-HMAC");

        return Task.CompletedTask;
    }

    /// <returns>(isSuccess, error, timestamp)</returns>
    private (bool, string?, long?) CheckTimestamp()
    {
        if (!Context.Request.Headers.TryGetValue("X-Timestamp", out var timestampHeader))
        {
            return (false, "Error: Unable to find the X-Timestamp header", null);
        }

        if (!long.TryParse(timestampHeader, out var timestamp))
        {
            return (false, "Error: Unable to parse the X-Timestamp value", null);
        }

        if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp) > 300) // 300 = 5 minutes
        {
            return (false, "Error: This request is already expired", null);
        }

        return (true, null, timestamp);
    }

    /// <returns>(isSuccess, error, nonce)</returns>
    private async Task<(bool, string?, string?)> CheckNonce()
    {
        if (!Context.Request.Headers.TryGetValue("X-Nonce", out var nonceHeader))
        {
            return (false, "Error: Unable to find the X-Nonce header", null);
        }

        var nonceValue = nonceHeader.ToString()
            .Trim();

        if (nonceValue is "")
        {
            return (false, "Error: Unable to find the X-Nonce value", null);
        }

        if (await _queryHmacNonce.TryClaim(nonceValue) is false)
        {
            return (false, "Error: The request is already processed", null);
        }

        return (true, null, nonceValue);
    }

    /// <returns>(isSuccess, error, key, credential)</returns>
    private async Task<(bool, string?, string?, ApplicationCredential?)> CheckApplication()
    {
        if (!Context.Request.Headers.TryGetValue("X-Application", out var applicationHeader))
        {
            return (false, "Error: Unable to find the X-Application header", null, null);
        }

        var application = applicationHeader.ToString()
            .Trim();

        var credential = await _queryApplication.GetApplicationPlusCredential(application);

        if (credential is null || credential.Credential is null or { Credential: null or "" })
        {
            return (false, "Error: Invalid X-Application header value", null, null);
        }

        return (true, null, application, credential.Credential);
    }

    /// <returns>Task (isSuccess, error)></returns>
    private async Task<(bool, string?)> CheckHmac(long timestamp, string nonce, string application,
        ApplicationCredential credential)
    {
        if (!Context.Request.Headers.TryGetValue("X-HMAC", out var hmacHeader))
        {
            return (false, "Error: Unable to find the X-HMAC header");
        }

        var hmacValue = hmacHeader.ToString()
            .Trim();

        if (hmacValue is "")
        {
            return (false, "Error: Unable to find the X-HMAC header");
        }

        Context.Request.EnableBuffering();
        using var stream = new StreamReader(Context.Request.Body, leaveOpen: true);
        var data = await stream.ReadToEndAsync();
        Context.Request.Body.Position = 0;

        var hmac = new HmacSignature(credential.Credential);
        var comp = hmac.Create(application, timestamp, nonce, data);

        var hmacBytes = Encoding.UTF8.GetBytes(hmacValue);
        var compBytes = Encoding.UTF8.GetBytes(comp);

        // compare request hmac signature and computed signature
        if (!CryptographicOperations.FixedTimeEquals(hmacBytes, compBytes))
        {
            return (false, "Error: The request doesn't match the HMAC value");
        }

        return (true, null);
    }
}