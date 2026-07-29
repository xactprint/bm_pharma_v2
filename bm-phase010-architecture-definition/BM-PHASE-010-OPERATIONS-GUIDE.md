# BM-PHASE-010 — OPERATIONS GUIDE

**Version:** 1.0 (Phase 010 creation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phases 001-009-C (all investigations)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-29 | BM Pharma | Initial operations guide |

---

## 1. Environment

### PostgreSQL CHIFA-OFFICINE

| Parameter | Value | Notes |
|-----------|-------|-------|
| Host | 127.0.0.1 | localhost only |
| Port | 5432 | CHIFA embedded PG |
| Database | CHIFA_OFFICINE | |
| Username | pharm | Superuser, DB owner |
| Password | (empty) | Trust auth |
| Version | 9.3.4 32-bit | EOL since 2018 |
| Schema | public | All tables in public |

### Symptoms of PG Service Down

- BM Pharma Dashboard shows "PostgreSQL: Déconnecté"
- Connection attempt returns NpgsqlException
- `netstat -an | findstr ":5432"` shows nothing listening
- Check if PG service exists: `Get-Service postgresql*`
- Check for embedded PG in CHIFA directory: `pg_ctl status`

### Starting PostgreSQL

```powershell
# If PG is installed as a service
Start-Service postgresql-9.3

# If PG is started manually from CHIFA directory
& "C:\Program Files (x86)\CHIFA_OFFICINE\postgresql\bin\pg_ctl" start -D "C:\Program Files (x86)\CHIFA_OFFICINE\postgresql\data"
```

---

## 2. Development Workflow

### Building

```bash
dotnet build BMPharma.sln
```

Expected: 0 errors, 0 warnings.

### Testing

```bash
dotnet test BMPharma.sln
```

Expected: 509 tests, ALL PASSING.

### Adding a New Entity

1. Create entity class in `src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/`
2. Create configuration class in `src/BMPharma.Persistence.PostgreSQL/Configurations/`
3. Add DbSet to both DbContexts (`ChifaPostgreSqlContext` and `ChifaWriteDbContext`)
4. Add tests in `tests/BMPharma.CHIFA.Tests/`
5. Run `dotnet build && dotnet test`

### Schema Discovery

```bash
# Against test Docker PG (port 5433)
cd tools/BMPharma.ChifaSchemaDiscovery
dotnet run -- test

# Against real CHIFA PG (port 5432)
dotnet run -- real
```

---

## 3. Write Operations

### Pre-Write Checklist

- [ ] Mode is NOT ReadOnly (must be Test or Production)
- [ ] Connection to PG is available
- [ ] Baseline documented (next_num_fact, next_num_bord, table counts)
- [ ] Backup taken (pg_dump)
- [ ] Admin password entered
- [ ] Warning dialog acknowledged

### Write Protocol (Proven — Phase 007-G)

1. Begin transaction (ReadCommitted)
2. Validate invoice (ChifaInvoiceValidator)
3. Apply defaults (ApplyDefaults, ApplyLineDefaults)
4. INSERT facture (SaveChangesAsync — single row)
5. INSERT detail_fact (SaveChangesAsync — separate call)
6. Commit transaction
7. Log audit (ChifaAuditService)

### Critical Rules

- **Write facture FIRST**, then detail_fact (FK ordering)
- **DateTime.SpecifyKind(value, DateTimeKind.Unspecified)** for timestamp columns
- **HasColumnType("xml")** for signature and fact_xml columns
- **HasColumnType("timestamp without time zone")** for timestamp columns
- **HasColumnType("date")** for date columns

---

## 4. Rollback Operations

### Rollback Protocol (Proven — Phase 005, 007)

```sql
BEGIN TRANSACTION;
DELETE FROM detail_fact WHERE num_fact = 'XXX';
DELETE FROM facture WHERE num_fact = 'XXX';
COMMIT;
```

### Post-Rollback Verification

```sql
SELECT count(*) FROM facture;          -- expect 0
SELECT count(*) FROM detail_fact;       -- expect 0
SELECT count(*) FROM bordereau;         -- expect 0
SELECT count(*) FROM signature;         -- expect 0
SELECT next_num_fact FROM parametre;    -- expect baseline value
SELECT next_num_bord FROM parametre;    -- expect baseline value
SELECT count(*) FROM medicament;        -- expect 7596
SELECT count(*) FROM ln;                -- expect 7,412,276
```

---

## 5. Mode Management

### Switching Modes

Edit `src/BMPharma.UI/appsettings.json`:

```json
{
  "CHIFA": {
    "Enabled": true,
    "Mode": "ReadOnly"  // ReadOnly | Test | Production
  }
}
```

### Mode Behavior

| Mode | Reads | Writes | Use |
|------|-------|--------|-----|
| ReadOnly | SELECT only | Blocked by WriteGuard | Default, safe |
| Test | SELECT only | INSERT allowed (synthetic data only) | Testing |
| Production | SELECT only | INSERT allowed | Real pharmacy use |

---

## 6. Known Good Baseline

After Phase 007-I rollback, the CHIFA_OFFICINE database is in its verified baseline state:

| Table | Row Count |
|-------|-----------|
| facture | 0 |
| detail_fact | 0 |
| bordereau | 0 |
| signature | 0 |
| medicament | 7,596 |
| ln | 7,412,276 |

| Counter | Value |
|---------|-------|
| next_num_fact | 1 |
| next_num_bord | 215 |
| code_centre | 11600 |
| code_ps | 1234567890 |

---

## 7. Testing Against Real CHIFA PG

### Connection Verification

```csharp
// Verify connection works
var available = await chifaIntegrationService.HealthCheckAsync();
// Returns true if PG is accessible
```

### Read-Only Verification

```csharp
// Verify ReadOnly mode blocks writes
var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
await Assert.ThrowsAsync<ChifaWriteBlockedException>(
    () => guard.EnsureWriteAllowedAsync());
```

### Write Verification (Test Mode Only)

```csharp
// Requires Test mode + backup
var result = await invoiceService.CreateInvoiceAsync(request);
// result.Success == true → row exists in facture table
```

---

## 8. Production Constraints

| Constraint | Detail | Status |
|------------|--------|--------|
| Never write to 42 non-critical tables | Read-only access only | CONFIRMED |
| Never modify CHIFA-OFFICINE.exe | Legal/compatibility | CONFIRMED |
| Never copy CHIFA DLLs | Version conflict | CONFIRMED |
| Never bypass token/signing | Regulatory | CONFIRMED |
| Never transmit to CNAS | Requires CHIFA workflow | CONFIRMED |
| Never automate signature | Hardware token required | CONFIRMED |
| Always verify baseline before writes | Data integrity | CONFIRMED |
| Always rollback test data | Baseline restoration | CONFIRMED |

---

## 9. Monitoring

### BM Pharma Dashboard Indicators

| Indicator | Source | What to Check |
|-----------|--------|--------------|
| PostgreSQL status | IChifaIntegrationService | Green = connected |
| CHIFA-OFFICINE status | IChifaIntegrationService | Green = available |
| Token status | IChifaTokenService | Present = detected |
| Signature status | IChifaSigningService | Signed = confirmed |
| Invoice count | SELECT facture | Number of persisted invoices |
| Bordereau count | SELECT bordereau | Number of bordereaux |
| Mode | ChifaIntegrationModeProvider | ReadOnly / Test / Production |

### Audit Log Example

```
[CHIFA-AUDIT] Op=CREATE_INVOICE Entity=facture Key=TST002
Details=Lines: 1, Total: 120.00, CorrelationId: 3c97fd4c
Success=True Duration=877ms Error=none User=system
```

---

## 10. FAQ

### Q: PostgreSQL connection fails. What do I check?

A: Verify PG is running (`netstat -an | findstr ":5432"`), check user is `pharm`, check host is `127.0.0.1`.

### Q: Write fails with FK error 23503?

A: Ensure facture is saved BEFORE detail_fact (two SaveChangesAsync calls).

### Q: DateTime error when writing?

A: Use `DateTime.SpecifyKind(value, DateTimeKind.Unspecified)` for `timestamp without time zone` columns.

### Q: data written but not visible in CHIFA?

A: DATABASE WRITE ≠ CHIFA VISIBILITY. Guide user to open CHIFA, navigate to Consultation Facture, and refresh. Bordereau may remain invisible (known TST003 limitation).

### Q: Counter was incremented but write failed. What happens?

A: If within the same transaction, counter is rolled back. If counter was committed before the write failure, the number is "consumed" — this is correct by design to prevent collisions.

### Q: Can I reuse a previously rolled-back invoice number?

A: No. Once consumed from parametre, invoice numbers are not reused (prevents collisions with potentially transmitted invoices).
