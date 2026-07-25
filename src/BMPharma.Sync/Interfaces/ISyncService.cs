namespace BMPharma.Sync.Interfaces;

public interface ISyncService
{
    Task<bool> SyncProductsAsync(CancellationToken cancellationToken = default);
    Task<bool> SyncMedicinesFromChifaAsync(CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastSyncDateAsync(CancellationToken cancellationToken = default);
}
