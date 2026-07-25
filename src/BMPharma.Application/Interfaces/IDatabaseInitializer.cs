namespace BMPharma.Application.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SeedDefaultDataAsync(CancellationToken cancellationToken = default);
}
