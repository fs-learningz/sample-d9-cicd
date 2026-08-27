using Microsoft.AspNetCore.Http;
using Note.Application.DTO;
using Note.Application.ExternalServices.Persistence;
using Note.Application.Wrapper;

namespace Note.Application.Services.Default;

public class ApplicationService : IApplicationService
{
    private readonly IQueryApplication _queryApplication;

    public ApplicationService(IQueryApplication queryApplication)
    {
        _queryApplication = queryApplication;
    }

    public async Task<Output> GetApplication(string externalId)
    {
        var application = await _queryApplication.GetApplication(externalId);

        if (application is null)
        {
            return new OutputError(StatusCodes.Status404NotFound, "Not Found",
                $"The application with external id = {externalId} was not found");
        }

        return new OutputOk<ApplicationOutput>(StatusCodes.Status200OK,
            new ApplicationOutput(application.ExternalId, application.Name, application.CreatedAt));
    }

    public async Task<Output> GetApplicationPlusCredential(string externalId)
    {
        var application = await _queryApplication.GetApplicationPlusCredential(externalId);

        if (application is null)
        {
            return new OutputError(StatusCodes.Status404NotFound, "Not Found",
                $"The application with external id = {externalId} was not found");
        }

        var applicationOutput =
            new ApplicationOutputPlusCredential(application.ExternalId, application.Name, application.CreatedAt, null);

        if (application.Credential is not null)
        {
            applicationOutput = applicationOutput with
            {
                Credential = new ApplicationCredentialOutput(application.Credential.Credential,
                    application.Credential.CreatedAt)
            };
        }

        return new OutputOk<ApplicationOutputPlusCredential>(StatusCodes.Status200OK, applicationOutput);
    }
}