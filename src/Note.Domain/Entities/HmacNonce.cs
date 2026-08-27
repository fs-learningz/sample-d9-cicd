namespace Note.Domain.Entities;

public class HmacNonce
{
    public int Id { get; set; }
    public required string Nonce { get; set; }
    public DateTime? CreatedAt { get; set; }
}