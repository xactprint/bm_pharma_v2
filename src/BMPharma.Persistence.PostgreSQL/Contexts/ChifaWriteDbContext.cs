using Microsoft.EntityFrameworkCore;

namespace BMPharma.Persistence.PostgreSQL.Contexts;

/// <summary>
/// Write DbContext for CHIFA PostgreSQL database.
/// Used for CHIFA Bordereau creation, signature, and closure operations.
/// </summary>
public class ChifaWriteDbContext : DbContext
{
    public ChifaWriteDbContext(DbContextOptions<ChifaWriteDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // CHIFA write entity configurations will be added during Phase 4
    }
}
