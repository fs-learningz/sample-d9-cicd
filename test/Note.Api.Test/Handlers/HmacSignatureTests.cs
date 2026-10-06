using System.Security.Cryptography;
using System.Text;
using Note.Api.Handlers;

namespace Note.Api.Test.Handlers;

public class HmacSignatureTests
{
    private const string Credential = "secret";
    private const string Application = "app";
    private const long Timestamp = 1_700_000_000;
    private const string Nonce = "nonce-1";
    private const string Data = "{\"title\":\"t\"}";

    [Fact]
    public void Create_WhenInputsAreKnown_ReturnsExpectedHmacSha256Base64()
    {
        // Arrange
        var sut = new HmacSignature(Credential);
        var payload = Encoding.UTF8.GetBytes($"{Application}:{Timestamp}:{Nonce}:{Data}");
        var expected = Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(Credential), payload));

        // Act
        var result = sut.Create(Application, Timestamp, Nonce, Data);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Create_WhenCalledTwiceWithSameInputs_ReturnsSameSignature()
    {
        // Arrange
        var sut = new HmacSignature(Credential);

        // Act
        var first = sut.Create(Application, Timestamp, Nonce, Data);
        var second = sut.Create(Application, Timestamp, Nonce, Data);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Create_WhenDataIsEmpty_ReturnsNonEmptySignature()
    {
        // Arrange
        var sut = new HmacSignature(Credential);

        // Act
        var result = sut.Create(Application, Timestamp, Nonce, "");

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Theory]
    [InlineData("other-app", Timestamp, Nonce, Data)]
    [InlineData(Application, Timestamp + 1, Nonce, Data)]
    [InlineData(Application, Timestamp, "other-nonce", Data)]
    [InlineData(Application, Timestamp, Nonce, "other-data")]
    public void Create_WhenAnyInputChanges_ReturnsDifferentSignature(string application, long timestamp,
        string nonce, string data)
    {
        // Arrange
        var sut = new HmacSignature(Credential);
        var baseline = sut.Create(Application, Timestamp, Nonce, Data);

        // Act
        var result = sut.Create(application, timestamp, nonce, data);

        // Assert
        Assert.NotEqual(baseline, result);
    }

    [Fact]
    public void Create_WhenCredentialChanges_ReturnsDifferentSignature()
    {
        // Arrange
        var baseline = new HmacSignature(Credential).Create(Application, Timestamp, Nonce, Data);

        // Act
        var result = new HmacSignature("other-secret").Create(Application, Timestamp, Nonce, Data);

        // Assert
        Assert.NotEqual(baseline, result);
    }
}
