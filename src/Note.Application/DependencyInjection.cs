using Microsoft.Extensions.DependencyInjection;
using Note.Application.Services;
using Note.Application.Services.Default;

namespace Note.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNoteApplication(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        
        return services;
    }
}
