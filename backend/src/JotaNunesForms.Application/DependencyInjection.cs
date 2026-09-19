using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Formularios;
using JotaNunesForms.Application.UseCases.Empresas;
using JotaNunesForms.Application.UseCases.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListFormulariosUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<GetAuthenticatedUserUseCase>();
        services.AddScoped<ListUsuariosUseCase>();
        services.AddScoped<GetUsuarioUseCase>();
        services.AddScoped<CreateUsuarioUseCase>();
        services.AddScoped<UpdateUsuarioUseCase>();
        services.AddScoped<ListEmpresasUseCase>();
        services.AddScoped<GetEmpresaUseCase>();
        services.AddScoped<CreateEmpresaUseCase>();
        services.AddScoped<UpdateEmpresaUseCase>();
        return services;
    }
}
