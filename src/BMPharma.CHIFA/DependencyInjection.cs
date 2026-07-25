using Microsoft.Extensions.DependencyInjection;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;

namespace BMPharma.CHIFA;

public static class DependencyInjection
{
    public static IServiceCollection AddChifaIntegration(this IServiceCollection services)
    {
        services.AddScoped<IChifaIntegrationService, ChifaIntegrationServiceStub>();
        services.AddScoped<IChifaInvoiceService, ChifaInvoiceServiceStub>();
        services.AddScoped<IChifaBordereauService, ChifaBordereauServiceStub>();
        services.AddScoped<IChifaTokenService, ChifaTokenServiceStub>();
        services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>();
        services.AddScoped<IChifaAuditService, ChifaAuditService>();
        services.AddScoped<ChifaInvoiceValidator>();
        services.AddScoped<ChifaBordereauValidator>();
        services.AddScoped<ChifaWriteGuard>();
        return services;
    }
}
