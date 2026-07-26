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
        services.AddScoped<BordereauWorkflowStateMachine>();
        services.AddSingleton<FakeChifaIntegrationProvider>();
        services.AddScoped<IChifaInvoiceWorkflowService, ChifaInvoiceWorkflowService>();
        services.AddScoped<IBordereauStatusService, BordereauStatusService>();

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
            services.AddScoped<IChifaIntegrationService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaInvoiceService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaBordereauService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaTokenService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaSigningService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
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
            ConnectionString = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;TrustServerCertificate=true"
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
        services.AddScoped<BordereauWorkflowStateMachine>();
        services.AddSingleton<FakeChifaIntegrationProvider>();
        services.AddScoped<IChifaInvoiceWorkflowService, ChifaInvoiceWorkflowService>();
        services.AddScoped<IBordereauStatusService, BordereauStatusService>();
        services.AddScoped<IChifaAuditService, ChifaAuditService>();

        if (modeProvider.IsReadOnly)
        {
            services.AddScoped<IChifaIntegrationService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaInvoiceService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaBordereauService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaTokenService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaSigningService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
        }
        else
        {
            services.AddDbContext<ChifaPostgreSqlContext>(options =>
                options.UseInMemoryDatabase("ChifaTestDb"));
            services.AddDbContext<ChifaWriteDbContext>(options =>
                options.UseInMemoryDatabase("ChifaWriteTestDb"));

            services.AddScoped<IChifaIntegrationService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaInvoiceService, ChifaPostgresInvoiceService>();
            services.AddScoped<IChifaBordereauService, ChifaPostgresBordereauService>();
            services.AddScoped<IChifaTokenService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaSigningService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
        }

        return services;
    }
}
