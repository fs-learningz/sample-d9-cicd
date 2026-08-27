namespace Note.Domain.Entities;

public class Note
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public DateTime? CreatedAt { get; set; }
}