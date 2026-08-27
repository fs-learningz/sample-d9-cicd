namespace Note.Application.ExternalServices.Persistence;

public interface IQueryApplication
{
    Task<Domain.Entities.Application?> GetApplication(string externalId);
    Task<Domain.Entities.Application?> GetApplicationPlusCredential(string externalId);
}