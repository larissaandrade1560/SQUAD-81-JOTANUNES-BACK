using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Dashboard;
using JotaNunesForms.Application.UseCases.Formularios;
using JotaNunesForms.Application.UseCases.Empresas;
using JotaNunesForms.Application.UseCases.Funcionarios;
using JotaNunesForms.Application.UseCases.Obras;
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
        services.AddScoped<ListObrasUseCase>();
        services.AddScoped<GetObraUseCase>();
        services.AddScoped<CreateObraUseCase>();
        services.AddScoped<UpdateObraUseCase>();
        services.AddScoped<ListFuncionariosUseCase>();
        services.AddScoped<CreateFuncionarioUseCase>();
        services.AddScoped<UpdateFuncionarioUseCase>();
        services.AddScoped<ListObraFuncionariosUseCase>();
        services.AddScoped<GetDashboardResumoUseCase>();
        return services;
    }
}
