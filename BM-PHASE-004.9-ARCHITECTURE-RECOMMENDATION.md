# BM-PHASE-004.9 — ARCHITECTURE RECOMMENDATION

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

Based on the complete discovery of the CHIFA_OFFICINE environment (48 tables, PostgreSQL 9.3.4, 785 MB database), this document provides architecture recommendations for BM Pharma integration.

---

## 1. Architecture Constraints

### 1.1 Hard Constraints

| Constraint | Detail | Impact |
|-----------|--------|--------|
| PostgreSQL 9.3.4 | EOL since 2018, 32-bit | Limited feature set |
| Embedded PG | Crash-prone (0xC0000142) | Must handle connection failures |
| Trust Authentication | No password required | Security risk |
| Single-User Mode | Read-only fallback | Limited write capability |
| No Schema Isolation | All tables in public | No schema-based separation |
| 48 Tables | Only 4 documented in spec | Integration surface area large |

### 1.2 Soft Constraints

| Constraint | Detail | Impact |
|-----------|--------|--------|
| Existing CHIFA-OFFICINE | Must not modify | Integration only |
| BM Pharma .NET 8 | Different runtime | Separate process |
| Npgsql 8.x vs 2.x | DLL version conflict | Separate DLLs |
| Production Database | Active data | Read-only until validated |

---

## 2. Integration Architecture Options

### 2.1 Option A: Direct Database Access (RECOMMENDED)

```
┌──────────────────────────────────────────────────────────┐
│                    BM Pharma (.NET 8)                     │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  ChifaRead   │  │  ChifaWrite  │  │  SQLite      │  │
│  │  DbContext    │  │  DbContext   │  │  DbContext   │  │
│  │  (Read-Only)  │  │  (Write)     │  │  (Local)     │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────────┘  │
│         │                  │                             │
│  ┌──────┴──────────────────┴───────────────────────┐    │
│  │              Npgsql 8.x (EF Core 8)             │    │
│  └──────────────────────┬──────────────────────────┘    │
└─────────────────────────┼────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│           PostgreSQL 9.3.4 (CHIFA-OFFICINE)              │
│           Port: 5432 | DB: CHIFA_OFFICINE                │
│           User: postgres | Auth: trust                   │
└──────────────────────────────────────────────────────────┘
```

**Advantages**:
- Direct access, minimal latency
- Full EF Core support
- Read/write in single connection

**Risks**:
- Direct access to production database
- Crash-prone backend
- No data isolation

### 2.2 Option B: Read Replica with Sync

```
┌──────────────────────────────────────────────────────────┐
│                    BM Pharma (.NET 8)                     │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Local SQLite │  │  Sync Engine │  │  ChifaWrite  │  │
│  │  (Read-Only)  │  │  (Periodic)  │  │  DbContext   │  │
│  └──────────────┘  └──────┬───────┘  └──────┬───────┘  │
│                           │                  │           │
│  ┌────────────────────────┴──────────────────┴─────┐    │
│  │              PostgreSQL 16.x (Docker)            │    │
│  │              Port: 5433 | Sync from CHIFA        │    │
│  └──────────────────────┬──────────────────────────┘    │
└─────────────────────────┼────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│           PostgreSQL 9.3.4 (CHIFA-OFFICINE)              │
│           Port: 5432 | Read-Only Sync                   │
└──────────────────────────────────────────────────────────┘
```

**Advantages**:
- No direct access to production database
- Modern PostgreSQL for BM Pharma features
- Data isolation

**Risks**:
- Data staleness (sync delay)
- Complexity of sync engine
- Dual PostgreSQL maintenance

### 2.3 Option C: API Layer

```
┌──────────────────────────────────────────────────────────┐
│                    BM Pharma (.NET 8)                     │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  ChifaAPI    │  │  ChifaWrite  │  │  SQLite      │  │
│  │  Client      │  │  DbContext   │  │  DbContext   │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────────┘  │
│         │                  │                             │
│         │ REST/GraphQL     │ Direct DB                   │
└─────────┼──────────────────┼─────────────────────────────┘
          │                  │
┌─────────▼──────────────────▼─────────────────────────────┐
│           CHIFA-OFFICINE Wrapper API                      │
│           (Separate service, reads/writes CHIFA DB)      │
└──────────────────────────┬───────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────┐
│           PostgreSQL 9.3.4 (CHIFA-OFFICINE)              │
└──────────────────────────────────────────────────────────┘
```

**Advantages**:
- Clean API separation
- No direct DB access from BM Pharma
- Version-independent

**Risks**:
- Additional service to maintain
- Performance overhead
- More moving parts

---

## 3. Recommended Architecture: Option A (Direct Access)

### 3.1 Rationale

1. **Simplicity**: Direct DB access is simplest for a pharmacist workstation
2. **Performance**: No intermediate layers
3. **Existing Code**: ChifaWriteDbContext already exists
4. **Low Usage**: Single pharmacist, not high-concurrency
5. **Production Readiness**: BM-SPEC-028-032 already define the schema

### 3.2 Implementation Plan

#### Phase 1: Fix Connection (CRITICAL)

```csharp
// ChifaConnectionConfig.cs
public static class ChifaConnectionConfig
{
    public static string GetConnectionString(string serverIp = "localhost")
    {
        return $"Host={serverIp};Port=5432;Database=CHIFA_OFFICINE;" +
               $"Username=postgres;SSL Mode=Disable;" +
               $"Command Timeout=30;Pooling=true;Max Pool Size=5";
    }
}
```

#### Phase 2: Fix EF Core Entities

| Entity | Action | Priority |
|--------|--------|----------|
| ChifaFacture | Remove 26 phantom properties, add 26 missing, fix taux type | CRITICAL |
| ChifaDetailFact | No changes needed | — |
| ChifaBordereau | No changes needed | — |
| ChifaParametre | Remove 5 phantom properties, add 45 missing, fix names | CRITICAL |
| ChifaMedicament | **NEW** — Create entity for 29-column drug catalog | CRITICAL |
| ChifaSignature | **NEW** — Create entity for 2-column signature table | CRITICAL |

#### Phase 3: Add Missing Entities

| Entity | Table | Columns | Priority |
|--------|-------|---------|----------|
| ChifaMedicament | medicament | 29 | CRITICAL |
| ChifaSignature | signature | 2 | CRITICAL |
| ChifaSpecialite | specialite | ~5 | MEDIUM |
| ChifaForme | forme | ~3 | MEDIUM |
| ChifaTarif | tarif | ~5 | MEDIUM |
| ChifaCentre | centre | ~5 | MEDIUM |
| ChifaUtilisateur | utilisateur | ~10 | MEDIUM |

#### Phase 4: Add Missing DbSets

```csharp
// ChifaReadDbContext.cs (NEW — read-only context)
public class ChifaReadDbContext : DbContext
{
    public DbSet<ChifaFacture> Factures => Set<ChifaFacture>();
    public DbSet<ChifaDetailFact> DetailFacts => Set<ChifaDetailFact>();
    public DbSet<ChifaBordereau> Bordereaus => Set<ChifaBordereau>();
    public DbSet<ChifaParametre> Parametres => Set<ChifaParametre>();
    public DbSet<ChifaMedicament> Medicaments => Set<ChifaMedicament>();
    public DbSet<ChifaSignature> Signatures => Set<ChifaSignature>();
    public DbSet<ChifaSpecialite> Specialites => Set<ChifaSpecialite>();
    public DbSet<ChifaForme> Formes => Set<ChifaForme>();
    public DbSet<ChifaTarif> Tarifs => Set<ChifaTarif>();
    public DbSet<ChifaCentre> Centres => Set<ChifaCentre>();
    public DbSet<ChifaUtilisateur> Utilisateurs => Set<ChifaUtilisateur>();
    
    // Configuration...
}
```

---

## 4. Data Access Pattern

### 4.1 Read Operations

```csharp
// Drug lookup
var medicament = await context.Medicaments
    .FirstOrDefaultAsync(m => m.NumEnr == numEnr);

// Invoice validation
var facture = await context.Factures
    .Include(f => f.DetailFacts)
    .FirstOrDefaultAsync(f => f.NumFact == numFact);

// Bordereau status
var bordereau = await context.Bordereaus
    .FirstOrDefaultAsync(b => b.NumBord == numBord);

// Pharmacy parameters
var parametre = await context.Parametres
    .FirstOrDefaultAsync();
```

### 4.2 Write Operations

```csharp
// Create invoice (only to facture, detail_fact, bordereau, signature)
using var transaction = await context.Database.BeginTransactionAsync();

var facture = new ChifaFacture
{
    NumFact = nextNumFact,
    DateFact = DateTime.Now,
    NumBord = bordereau.NumBord,
    MontFact = totalAmount,
    // ... other fields
};

context.Factures.Add(facture);

// Add invoice lines
foreach (var line in invoiceLines)
{
    context.DetailFacts.Add(new ChifaDetailFact
    {
        NumFact = facture.NumFact,
        NumEnr = line.NumEnr,
        Ppa = line.Ppa,
        Qte = line.Qte,
        Mont = line.Mont,
        // ... other fields
    });
}

await context.SaveChangesAsync();
await transaction.CommitAsync();
```

### 4.3 Write-Safe Tables

| Table | Safe to Write? | Operations |
|-------|---------------|------------|
| facture | ✅ YES | INSERT, UPDATE |
| detail_fact | ✅ YES | INSERT, UPDATE, DELETE |
| bordereau | ✅ YES | INSERT, UPDATE |
| signature | ✅ YES | INSERT |
| All others | ❌ NO | READ-ONLY |

---

## 5. Error Handling

### 5.1 Connection Failures

```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (NpgsqlException ex) when (i < maxRetries - 1)
        {
            // Log connection failure
            Logger.LogWarning($"Connection attempt {i+1} failed: {ex.Message}");
            await Task.Delay(1000 * (i + 1)); // Exponential backoff
        }
    }
    throw new InvalidOperationException("CHIFA database unavailable");
}
```

### 5.2 Backend Crash Handling

```csharp
public bool IsChifaAvailable()
{
    try
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return true;
    }
    catch (NpgsqlException)
    {
        return false;
    }
}
```

---

## 6. Security Architecture

### 6.1 Current State (INSECURE)

| Issue | Risk | Mitigation |
|-------|------|------------|
| Trust authentication | Any device can connect | Restrict network access |
| No SSL | Data in plaintext | Use localhost only |
| No password | No authentication | Accept for embedded PG |
| 0.0.0.0/0 in pg_hba | Network-wide access | Firewall rules |

### 6.2 Recommended Security Posture

| Layer | Action |
|-------|--------|
| Network | Restrict to localhost (127.0.0.1) only |
| Application | BM Pharma handles authentication |
| Database | Trust auth is acceptable for embedded PG |
| Data | Encrypt sensitive fields at application level |
| Audit | Log all write operations |

---

## 7. Performance Considerations

### 7.1 Embedded PG Limitations

| Limitation | Impact | Mitigation |
|-----------|--------|------------|
| 32-bit process | Max 2GB memory | Optimize queries |
| 128MB shared_buffers | Limited cache | Use appropriate indexes |
| Minimal WAL | No crash recovery | Regular backups |
| Single-user mode | No concurrent writes | Serialize writes |

### 7.2 Recommended Optimizations

1. **Connection Pooling**: Min=1, Max=5
2. **Query Timeout**: 30 seconds
3. **Lazy Loading**: Disabled (use Include)
4. **Change Tracking**: Minimal for reads
5. **No Tracking Queries**: For read-only operations

```csharp
// Read-only optimized query
var factures = await context.Factures
    .AsNoTracking()
    .Where(f => f.NumBord == numBord)
    .ToListAsync();
```

---

## 8. Migration Path

### 8.1 Immediate (Week 1)

1. Fix connection string (username: postgres, database: CHIFA_OFFICINE, schema: public)
2. Fix ChifaFacture phantom properties
3. Fix ChifaParametre phantom properties
4. Test connection with real database

### 8.2 Short-term (Week 2-3)

1. Add ChifaMedicament entity
2. Add ChifaSignature entity
3. Add missing columns to ChifaFacture
4. Add missing columns to ChifaParametre
5. Implement read-only DbContext

### 8.3 Medium-term (Week 4+)

1. Add Tier 2 entities (specialite, forme, tarif, centre, utilisateur)
2. Implement drug validation logic
3. Implement bordereau closure via cloturerbord()
4. Add error handling and retry logic

### 8.4 Long-term (Future)

1. Consider Option B (sync to modern PG) if embedded PG is too unstable
2. Consider Option C (API layer) if multi-workstation deployment needed
3. Monitor PostgreSQL 9.3.4 stability

---

## 9. Testing Strategy

### 9.1 Unit Tests

- Entity mapping validation (all columns match)
- Connection string configuration
- Read-only operations (SELECT queries)
- Write operations (INSERT/UPDATE to facture/detail_fact)

### 9.2 Integration Tests

- Connection to real CHIFA database (single-user mode)
- Drug lookup (medicament table)
- Invoice creation (facture + detail_fact)
- Bordereau creation and closure
- Signature insertion

### 9.3 Security Tests

- No writes to reference tables
- No credential exposure
- Connection failure handling

---

## 10. Summary

| Item | Recommendation |
|------|---------------|
| Architecture | **Option A: Direct DB Access** |
| Connection | Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres |
| EF Core Entities | Fix phantom properties, add missing columns |
| New Entities | ChifaMedicament, ChifaSignature + 5 more |
| Write Tables | facture, detail_fact, bordereau, signature ONLY |
| Read Tables | All 48 tables |
| Error Handling | Retry with exponential backoff |
| Security | Localhost only, trust auth acceptable |
| Performance | Connection pooling, no-tracking queries |

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
