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

        services.AddSingleton<CorrelationContext>();
        services.AddSingleton<ChifaCircuitBreaker>();
        services.AddSingleton<ChifaMetricsService>();
        services.AddScoped<ChifaHealthCheckService>();
        services.AddScoped<StatusEngine>();
        services.AddScoped<InvoiceSynchronizer>();
        services.AddScoped<BordereauSynchronizer>();
        services.AddScoped<StatusSynchronizer>();
        services.AddScoped<ChifaMonitoringService>();
        services.AddScoped<IChifaIntegrationFacade, ChifaIntegrationFacade>();

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
            services.AddScoped<IChifaNumberingService>(sp =>
            {
                var ctx = sp.GetRequiredService<ChifaWriteDbContext>();
                return new ChifaNumberingService(ctx);
            });
        }
        else
        {
            services.AddScoped<IChifaIntegrationService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaInvoiceService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaBordereauService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaTokenService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaSigningService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaAuditService, ChifaAuditService>();
            services.AddScoped<IChifaNumberingService, ChifaNumberingServiceFake>();
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
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=5;CommandTimeout=10"
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

        services.AddSingleton<CorrelationContext>();
        services.AddSingleton<ChifaCircuitBreaker>();
        services.AddSingleton<ChifaMetricsService>();
        services.AddScoped<ChifaHealthCheckService>();
        services.AddScoped<StatusEngine>();
        services.AddScoped<InvoiceSynchronizer>();
        services.AddScoped<BordereauSynchronizer>();
        services.AddScoped<StatusSynchronizer>();
        services.AddScoped<ChifaMonitoringService>();
        services.AddScoped<IChifaIntegrationFacade, ChifaIntegrationFacade>();

        if (modeProvider.IsReadOnly)
        {
            services.AddScoped<IChifaIntegrationService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaInvoiceService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaBordereauService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaTokenService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaSigningService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
            services.AddScoped<IChifaNumberingService, ChifaNumberingServiceFake>();
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
            services.AddScoped<IChifaNumberingService, ChifaNumberingServiceFake>();
        }

        return services;
    }
}
