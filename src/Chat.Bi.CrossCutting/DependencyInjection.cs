﻿using Chat.Bi.Application.Commands.Usuario.CriarContaUsuario;
using Chat.Bi.Core.Repositories;
using Chat.Bi.Core.Services;
using Chat.Bi.Infrastructure.Auth;
using Chat.Bi.Infrastructure.Configuration;
using Chat.Bi.Infrastructure.Logging;
using Chat.Bi.Infrastructure.Persistence;
using Chat.Bi.Infrastructure.Persistence.Repositories;
using Chat.Bi.SharedKernel.Configuration;
using Chat.Bi.SharedKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Bi.CrossCutting;

public static class DependencyInjection
{
    public static IServiceCollection ResolveDependencias(this IServiceCollection services, IConfiguration configuration)
    {
        ResolveConexaoBanco(services, configuration);
        Infraestrutura(services, configuration);
        SharedKernel(configuration);
        return services;
    }

    static void SharedKernel(IConfiguration configuration)
    {
        var criptografia = configuration.GetSection(nameof(CriptografiaSettings));
        var criptografiaSettings = criptografia.Get<CriptografiaSettings>();
        CriptografiaExtensions.Configure(criptografiaSettings);
    }

    static void Infraestrutura(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        services.AddTransient<IAuthService, AuthService>();
        services.AddScoped<IUsuarioAutenticadoService, UsuarioAutenticadoService>();
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CriarContaUsuarioCommand).Assembly));
        ResolverRepository(services);
    }

    static void ResolverRepository(IServiceCollection services)
    {
        services.AddTransient(typeof(IBaseEntityRepository<,>), typeof(BaseEntityRepository<,>));
        services.AddTransient<IUsuarioRepository, UsuarioRepository>();
        services.AddTransient<IEmpresaRepository, EmpresaRepository>();
        services.AddTransient<IChatRepository, ChatRepository>();
        services.AddTransient<IChatConfigRepository, ChatConfigRepository>();
        services.AddTransient<IBaseDeDadosRepository, BaseDeDadosRepository>();
    }

    static void ResolveConexaoBanco(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ChatBiDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}