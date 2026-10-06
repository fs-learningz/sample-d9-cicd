using System.Security.Cryptography;
using System.Text;

namespace Note.Api.Handlers;

public class HmacSignature
{
    private readonly byte[] _credential;

    public HmacSignature(string credential)
    {
        _credential = Encoding.UTF8.GetBytes(credential);
    }

    public string Create(string application, long timestamp, string nonce, string data)
    {
        var payload = $"{application}:{timestamp}:{nonce}:{data}";

        using var hmac = new HMACSHA256(_credential);

        var utf8 = Encoding.UTF8.GetBytes(payload);
        var hash = hmac.ComputeHash(utf8);

        return Convert.ToBase64String(hash);
    }
}
