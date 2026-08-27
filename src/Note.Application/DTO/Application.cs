
namespace Note.Application.DTO;

public sealed record ApplicationOutput(string ExternalId, string Name, DateTime? CreatedAt);
public sealed record ApplicationOutputPlusCredential(string ExternalId, string Name, DateTime? CreatedAt, ApplicationCredentialOutput? Credential);
public sealed record ApplicationCredentialOutput(string Credential, DateTime? CreatedAt);
