using Microsoft.EntityFrameworkCore;

namespace BMPharma.Persistence.PostgreSQL.Contexts;

/// <summary>
/// Read-only DbContext for CHIFA PostgreSQL database.
/// Maps to existing CHIFA tables. No migrations — read-only access.
/// </summary>
public class ChifaPostgreSqlContext : DbContext
{
    // Read-only — no DbSets for write operations
    
    public ChifaPostgreSqlContext(DbContextOptions<ChifaPostgreSqlContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // All entities are read-only, configured via HasNoKey() or ToView()
        // Actual CHIFA table mappings will be added during Phase 4
    }
}
