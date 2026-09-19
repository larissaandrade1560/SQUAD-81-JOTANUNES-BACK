using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Formularios;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListFormulariosUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<GetAuthenticatedUserUseCase>();
        return services;
    }
}
