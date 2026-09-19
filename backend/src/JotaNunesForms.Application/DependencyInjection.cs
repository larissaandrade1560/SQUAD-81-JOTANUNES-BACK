using JotaNunesForms.Application.UseCases.Formularios;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListFormulariosUseCase>();
        return services;
    }
}
