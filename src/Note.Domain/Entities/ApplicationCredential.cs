namespace Note.Domain.Entities;

public class ApplicationCredential
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public required string Credential { get; set; }
    public DateTime? CreatedAt { get; set; }
}