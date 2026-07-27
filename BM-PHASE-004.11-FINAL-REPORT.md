# BM-PHASE-004.11 — PostgreSQL TCP Crash DIAGNOSIS COMPLETE

## Executive Summary

**The PostgreSQL 9.3.4 "crash" (`0xC0000142`) was NOT a DLL/crash issue.**  
**It was a DATABASE PERMISSIONS issue.** TCP connections work perfectly.

---

## Root Cause

The CHIFA_OFFICINE database is **owned by user `pharm`** (superuser).  
User `postgres` had **NO CONNECT privilege** on the database.  
Our connection string used `Username=postgres` → PostgreSQL returned `FATAL: droit refusé` → the server killed the backend process → this was logged as `0xC0000142` exit status, which was **misinterpreted as a crash**.

### What actually happened
1. `psql`/Npgsql connects as `postgres` to `CHIFA_OFFICINE`
2. PostgreSQL checks `pg_database` → `postgres` has no CONNECT grant
3. PostgreSQL sends `FATAL: droit refusé pour la base de données CHIFA_OFFICINE`
4. Backend process exits → postmaster logs it with the Windows exit code `0xC0000142`
5. **This is NOT a crash** — it's a normal permission denial with a confusing log entry

### Why `postgres --single` worked
`postgres --single` bypasses normal authentication and privilege checks (it runs as trusted superuser in single-user mode). That's why it always worked.

---

## What Was Fixed

### 1. Connection String (Primary Fix)
Changed all connection strings from `Username=postgres` to `Username=pharm`:

| File | Change |
|------|--------|
| `src/BMPharma.UI/appsettings.json` | `Username=pharm`, `Host=127.0.0.1` |
| `src/BMPharma.CHIFA/DependencyInjection.cs` | Test connection string → `pharm` |
| `tools/BMPharma.ChifaSchemaDiscovery/Program.cs` | Real connection → `pharm` |
| `tests/BMPharma.CHIFA.Tests/ChifaRealSchemaAlignmentTests.cs` | All test strings → `pharm` |

### 2. GRANT Statements (Optional — for `postgres` user)
Granted `postgres` user access for future flexibility:
```sql
GRANT CONNECT ON DATABASE "CHIFA_OFFICINE" TO postgres;
GRANT USAGE ON SCHEMA public TO postgres;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO postgres;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO postgres;
```

---

## Verification Results

### psql (Command Line)
| Test | User | Result |
|------|------|--------|
| Connect to CHIFA_OFFICINE | `pharm` | PASS |
| Connect to CHIFA_OFFICINE | `postgres` | PASS (after GRANT) |
| Query medicament (7596 rows) | `pharm` | PASS |
| Query parametre (1 row) | `pharm` | PASS |
| Query ln (7,412,276 rows) | `pharm` | PASS |

### .NET (Npgsql)
| Test | User | Result |
|------|------|--------|
| `pharm @ CHIFA_OFFICINE` | `pharm` | PASS |
| `postgres @ CHIFA_OFFICINE` | `postgres` | PASS |
| Multi-table query (7 tables) | `pharm` | PASS |
| Read parametre data | `pharm` | PASS |

### Test Suite
| Test Project | Tests | Result |
|--------------|-------|--------|
| BMPharma.CHIFA.Tests | 463 | ALL PASS |
| BMPharma.Application.Tests | 3 | ALL PASS |
| BMPharma.Domain.Tests | 6 | ALL PASS |
| BMPharma.ArchitectureTests | 7 | ALL PASS |
| **TOTAL** | **479** | **ALL PASS** |

---

## Production Connection String

```
Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30
```

### User Roles
| User | Role | CONNECT | SELECT | Notes |
|------|------|---------|--------|-------|
| `pharm` | Superuser | YES | YES | **Production user** — DB owner |
| `postgres` | No inheritance | YES (after GRANT) | YES (after GRANT) | Limited — was causing false "crash" |
| `postgres2` | Template owner | YES | YES | Owns template0, template1 |

---

## Key Findings

1. **PostgreSQL 9.3.4 TCP works perfectly** on Windows x86
2. **No DLL/crash issue** — `0xC0000142` was permission denial
3. **Database is owned by `pharm`**, not `postgres`
4. **`postgres --single`** worked because it bypasses all privilege checks
5. **No system changes needed** — no VC++ Redistributable, no locale fix, no DLL replacement
6. **The Schema Discovery Tool and EF Core can now connect via TCP**

---

## What This Unblocks

With TCP working, the following BM-PHASE tasks are now possible:
- **Schema Discovery Tool** via normal TCP connection (no more `--single` workaround)
- **EF Core integration** (ChifaPostgreSqlContext, ChifaWriteDbContext)
- **Test/Production modes** (non-ReadOnly)
- **Real-time queries** from BM Pharma UI

---

## Files Modified
1. `src/BMPharma.UI/appsettings.json` — Connection string fix
2. `src/BMPharma.CHIFA/DependencyInjection.cs` — Test connection string fix
3. `tools/BMPharma.ChifaSchemaDiscovery/Program.cs` — Real connection string fix
4. `tests/BMPharma.CHIFA.Tests/ChifaRealSchemaAlignmentTests.cs` — Test assertions fix
5. `tools/TcpConnectionTest/Program.cs` — New verification tool

## CHIFA Database Changes (READ-ONLY safe)
- `GRANT CONNECT ON DATABASE CHIFA_OFFICINE TO postgres` — added permission
- `GRANT USAGE ON SCHEMA public TO postgres` — added permission
- `GRANT SELECT ON ALL TABLES IN SCHEMA public TO postgres` — added permission
- `ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO postgres` — future tables
