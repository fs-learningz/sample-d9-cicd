using Note.Application.Wrapper;

namespace Note.Application.Services;

public interface IApplicationService
{
    Task<Output> GetApplication(string externalId);
    Task<Output> GetApplicationPlusCredential(string externalId);
}
