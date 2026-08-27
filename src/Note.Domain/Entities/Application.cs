namespace Note.Domain.Entities;

public class Application
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public required string Name { get; set; }
    public DateTime? CreatedAt { get; set; }

    public ApplicationCredential? Credential { get; set; }
}