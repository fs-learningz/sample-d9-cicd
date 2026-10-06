using Microsoft.Extensions.DependencyInjection;
using Note.Application.ExternalServices.Persistence;
using Note.Infrastructure.Persistence;

namespace Note.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddNoteInfrastructure()
        {
            serviceCollection.AddScoped<IQueryNote, QueryNote>();
            serviceCollection.AddScoped<IQueryApplication, QueryApplication>();
            serviceCollection.AddScoped<IQueryHmacNonce, QueryHmacNonce>();

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            return serviceCollection;
        }
    }
}