using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Persistence.PostgreSQL.Contexts;

namespace BMPharma.CHIFA;

public static class DependencyInjection
{
    public static IServiceCollection AddChifaIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = new ChifaIntegrationConfig();
        configuration.GetSection("CHIFA").Bind(config);
        services.AddSingleton(config);

        var modeProvider = new ChifaIntegrationModeProvider(config);
        services.AddSingleton(modeProvider);
        services.AddSingleton<Func<Task<Domain.Enums.ChifaIntegrationMode>>>(
            () => modeProvider.GetModeAsync());

        services.AddScoped<ChifaWriteGuard>();
        services.AddScoped<ChifaInvoiceValidator>();
        services.AddScoped<ChifaBordereauValidator>();
        services.AddScoped<ChifaInvoiceMapper>();
        services.AddScoped<ChifaBordereauMapper>();
        services.AddScoped<ChifaWorkflowStateMachine>();

        var usePostgres = !modeProvider.IsReadOnly;

        if (usePostgres)
        {
            services.AddDbContext<ChifaPostgreSqlContext>(options =>
                options.UseNpgsql(config.ConnectionString));

            services.AddDbContext<ChifaWriteDbContext>(options =>
                options.UseNpgsql(config.ConnectionString));

            services.AddScoped<IChifaIntegrationService, ChifaIntegrationServiceStub>();
            services.AddScoped<IChifaInvoiceService, ChifaPostgresInvoiceService>();
            services.AddScoped<IChifaBordereauService, ChifaPostgresBordereauService>();
            services.AddScoped<IChifaTokenService, ChifaTokenServiceStub>();
            services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>();
            services.AddScoped<IChifaAuditService, ChifaAuditService>();
        }
        else
        {
            services.AddScoped<IChifaIntegrationService, ChifaIntegrationServiceStub>();
            services.AddScoped<IChifaInvoiceService, ChifaInvoiceServiceStub>();
            services.AddScoped<IChifaBordereauService, ChifaBordereauServiceStub>();
            services.AddScoped<IChifaTokenService, ChifaTokenServiceStub>();
            services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>();
            services.AddScoped<IChifaAuditService, ChifaAuditService>();
        }

        return services;
    }

    public static IServiceCollection AddChifaIntegrationForTests(
        this IServiceCollection services,
        Domain.Enums.ChifaIntegrationMode mode = Domain.Enums.ChifaIntegrationMode.ReadOnly)
    {
        var config = new ChifaIntegrationConfig
        {
            Mode = mode.ToString(),
            ConnectionString = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Trust=true"
        };
        services.AddSingleton(config);

        var modeProvider = new ChifaIntegrationModeProvider(config);
        services.AddSingleton(modeProvider);
        services.AddSingleton<Func<Task<Domain.Enums.ChifaIntegrationMode>>>(
            () => modeProvider.GetModeAsync());

        services.AddScoped<ChifaWriteGuard>();
        services.AddScoped<ChifaInvoiceValidator>();
        services.AddScoped<ChifaBordereauValidator>();
        services.AddScoped<ChifaInvoiceMapper>();
        services.AddScoped<ChifaBordereauMapper>();
        services.AddScoped<ChifaWorkflowStateMachine>();
        services.AddScoped<IChifaAuditService, ChifaAuditService>();

        if (modeProvider.IsReadOnly)
        {
            services.AddScoped<IChifaIntegrationService, ChifaIntegrationServiceStub>();
            services.AddScoped<IChifaInvoiceService, ChifaInvoiceServiceStub>();
            services.AddScoped<IChifaBordereauService, ChifaBordereauServiceStub>();
            services.AddScoped<IChifaTokenService, ChifaTokenServiceStub>();
            services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>();
        }
        else
        {
            services.AddDbContext<ChifaPostgreSqlContext>(options =>
                options.UseInMemoryDatabase("ChifaTestDb"));
            services.AddDbContext<ChifaWriteDbContext>(options =>
                options.UseInMemoryDatabase("ChifaWriteTestDb"));

            services.AddScoped<IChifaIntegrationService, ChifaIntegrationServiceStub>();
            services.AddScoped<IChifaInvoiceService, ChifaPostgresInvoiceService>();
            services.AddScoped<IChifaBordereauService, ChifaPostgresBordereauService>();
            services.AddScoped<IChifaTokenService, ChifaTokenServiceStub>();
            services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>();
        }

        return services;
    }
}
