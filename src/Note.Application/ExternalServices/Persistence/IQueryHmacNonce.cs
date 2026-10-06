namespace Note.Application.ExternalServices.Persistence;

public interface IQueryHmacNonce
{
    Task<bool> TryClaim(string nonce);
}