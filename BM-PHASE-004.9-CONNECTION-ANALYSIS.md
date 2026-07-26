# BM-PHASE-004.9 — CONNECTION ANALYSIS

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

Complete analysis of the connection path from BM Pharma to the CHIFA_OFFICINE PostgreSQL database, including all configuration files, authentication mechanisms, network settings, and the actual connection parameters needed.

---

## 1. Connection Path

```
BM Pharma (.NET 8)
    │
    │  Npgsql 8.x Connection String
    │  Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres
    │
    ▼
PostgreSQL 9.3.4 (embedded, x86, port 5432)
    │
    │  pg_hba.conf authentication check
    │  Method: trust (no password)
    │
    ▼
CHIFA_OFFICINE database (public schema, 48 tables)
```

---

## 2. Configuration Files (Discovery Order)

### 2.1 CONFIG.xml

**Location**: CHIFA-OFFICINE installation directory
**Purpose**: Primary application configuration

```xml
<configuration>
  <appSettings>
    <add key="SERVER_IP" value="localhost" />
  </appSettings>
</configuration>
```

**Analysis**:
- Only specifies server IP: `localhost`
- Does NOT specify database name, username, port, or password
- These are likely hardcoded in the application or derived from other sources

### 2.2 Lancer_Serveur.bat

**Location**: CHIFA-OFFICINE installation directory
**Purpose**: Start embedded PostgreSQL server

```batch
@echo off
set PGUSER=postgres
set PGDATABASE=postgres
cd /d "%~dp0"
pg_ctl start -D . -w -t 30
```

**Analysis**:
- `PGUSER=postgres` — The PostgreSQL superuser
- `PGDATABASE=postgres` — Default maintenance database
- `pg_ctl start -D .` — Starts PG from current directory
- `-w` — Wait for startup
- `-t 30` — 30 second timeout

**Key Discovery**: The username is `postgres`, NOT `pharm` as BM Pharma assumed.

### 2.3 pg_hba.conf

**Location**: CHIFA-OFFICINE data directory
**Purpose**: Client authentication configuration

```
# TYPE  DATABASE  USER      ADDRESS           METHOD
local   all       all                       trust
host    all       all       127.0.0.1/32      trust
host    all       all       0.0.0.0/0         trust
host    all       all       ::1/128           trust
```

**Analysis**:
- **Method**: `trust` — No password required for ANY connection
- **Database**: `all` — All databases allowed
- **User**: `all` — All users allowed
- **Address**: `0.0.0.0/0` — From ANY IP address
- **Security implication**: Any device on the network can connect without credentials

### 2.4 postgresql.conf

**Location**: CHIFA-OFFICINE data directory
**Purpose**: PostgreSQL server configuration

| Parameter | Value | Impact |
|-----------|-------|--------|
| `listen_addresses` | `*` | Listens on ALL network interfaces |
| `port` | 5432 | Default PostgreSQL port |
| `shared_buffers` | 128MB | Memory allocation |
| `wal_level` | `minimal` | Reduced WAL for crash recovery |
| `max_wal_senders` | 0 | No replication allowed |
| `log_connections` | off (default) | Connection logging not enabled |
| `log_disconnections` | off (default) | Disconnection logging not enabled |

### 2.5 settings.cfg

**Location**: CHIFA-OFFICINE installation directory
**Purpose**: Smart card reader configuration

Contains hardware settings for:
- Gemalto smart card readers
- Identiv uTrust 3512 SAM readers

**Purpose**: Reads CNAS beneficiary cards for identity verification.

### 2.6 NLog.config

**Location**: CHIFA-OFFICINE installation directory
**Purpose**: Application logging configuration

### 2.7 CHIFA_OFFICINE.exe.config

**Location**: CHIFA-OFFICINE installation directory
**Purpose**: .NET Framework 4.0 application configuration

Standard .NET config file for the CHIFA-OFFICINE executable.

---

## 3. Actual Connection Parameters

### 3.1 Discovered via Single-User Mode

```sql
-- From pg_database:
SELECT datname, datdba, encoding, datcollate FROM pg_database;
-- Result: CHIFA_OFFICINE, OID 16394, owner postgres, UTF8, French_France.1252

-- From Lancer_Serveur.bat:
-- PGUSER=postgres, PGDATABASE=postgres

-- From pg_hba.conf:
-- host all all 0.0.0.0/0 trust
```

### 3.2 Final Connection String

```
Host=localhost
Port=5432
Database=CHIFA_OFFICINE
Username=postgres
Password=(none — trust auth)
SSL Mode=Disable
```

**Npgsql 8.x connection string**:
```
Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;SSL Mode=Disable
```

---

## 4. BM Pharma's Assumed Connection vs Reality

### 4.1 Side-by-Side Comparison

| Parameter | BM Pharma Assumed | Actual (Discovered) | Status |
|-----------|------------------|---------------------|--------|
| Host | localhost | localhost | ✅ CORRECT |
| Port | 5432 | 5432 | ✅ CORRECT |
| Database | CHIFA_OFFICINE | CHIFA_OFFICINE | ✅ CORRECT |
| Username | **pharm** | **postgres** | ❌ **WRONG** |
| Password | **pharm** | *(none)* | ❌ **WRONG** |
| Schema | **cnas** | **public** | ❌ **WRONG** |
| SSL | Unknown | Disable | ⚠️ UNCONFIRMED |

### 4.2 Impact of Wrong Username

BM Pharma attempts to connect as `pharm` user:
- If `pharm` user does not exist → Connection refused
- If `pharm` user exists but has no privileges → Authorization failure
- The real username is `postgres` (superuser) with trust authentication

### 4.3 Impact of Wrong Password

BM Pharma sends password `pharm`:
- With trust authentication, the password is **ignored** — any password works
- However, if the username is wrong, the password is irrelevant

### 4.4 Impact of Wrong Schema

BM Pharma references tables in `cnas` schema:
- All tables are in `public` schema
- Any query referencing `cnas.tablename` will fail with "relation does not exist"
- BM Pharma must use `public.tablename` or just `tablename` (default search_path)

---

## 5. Network Configuration

### 5.1 Port Binding

```
Port: 5432
Listen: * (all interfaces)
Protocol: TCP/IP
```

**Verification** (from BM-PHASE-004.8):
- `netstat -an | findstr ":5432"` → AUCUN RÉSULTAT (port not open in multi-user mode)
- The PG backend crashes with 0xC0000142, so the port never opens

### 5.2 Connection Options

| Option | Status | Notes |
|--------|--------|-------|
| TCP/IP (port 5432) | ❌ BLOCKED | Backend crash (0xC0000142) |
| Unix socket | ❌ BLOCKED | Same crash |
| Single-user mode | ✅ WORKS | Read-only, local only |
| Named pipe | ❓ UNKNOWN | Not tested |

### 5.3 Firewall

No Windows firewall rules were discovered blocking port 5432. The issue is the PostgreSQL backend crash, not firewall.

---

## 6. DLL Version Matrix

| Component | CHIFA-OFFICINE | BM Pharma | Compatible? |
|-----------|---------------|-----------|-------------|
| Npgsql.dll | 2.x (2013) | 8.x (2024) | ❌ NO |
| .NET Framework | 4.0 Client Profile | .NET 8+ | ❌ NO (different runtime) |
| PostgreSQL | 9.3.4 (x86) | Client: 16.x (via Npgsql) | ✅ YES (Npgsql handles protocol) |
| Entity Framework | None (ADO.NET) | EF Core 8.x | N/A |

### 6.1 Npgsql Protocol Compatibility

Despite the version difference, Npgsql 8.x is backward-compatible with PostgreSQL 9.3.4 for basic operations:
- SELECT queries ✅
- INSERT/UPDATE/DELETE ✅
- Parameterized queries ✅
- Transactions ✅

**Not compatible**:
- Logical replication ❌
- Partitioning ❌
- Newer data types (JSONB, etc.) ❌
- pg_notify/LISTEN ❓

---

## 7. Connection Failure Scenarios

### 7.1 Backend Crash (0xC0000142)

**Scenario**: CHIFA-OFFICINE is running but PostgreSQL backend crashes
**Symptom**: Connection refused on port 5432
**Workaround**: Use `postgres --single` mode

### 7.2 CHIFA-OFFICINE Not Running

**Scenario**: The application hasn't been started
**Symptom**: No process listening on port 5432
**Workaround**: Start CHIFA-OFFICINE or run `Lancer_Serveur.bat`

### 7.3 Wrong Username

**Scenario**: BM Pharma connects as `pharm` instead of `postgres`
**Symptom**: `FATAL: role "pharm" does not exist`
**Fix**: Change connection string to use `Username=postgres`

### 7.4 Schema Not Found

**Scenario**: BM Pharma references `cnas.tablename`
**Symptom**: `ERROR: relation "cnas.tablename" does not exist`
**Fix**: Use `public.tablename` or just `tablename`

---

## 8. Recommended Connection Configuration

### 8.1 For BM Pharma Integration

```json
{
  "ConnectionStrings": {
    "ChifaDatabase": "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;SSL Mode=Disable"
  }
}
```

### 8.2 For Read-Only Operations

```csharp
optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.CommandTimeout(30);
    // No need for transactions in read-only mode
});
```

### 8.3 For Write Operations (Future)

```csharp
// Only write to: facture, detail_fact, bordereau, signature
// Never write to: parametre, medicament, ln, or any reference table
```

---

## 9. Connection Pool Settings

| Setting | Recommended Value | Reason |
|---------|-------------------|--------|
| Min Pool Size | 1 | Low usage environment |
| Max Pool Size | 5 | Embedded PG, limited resources |
| Connection Idle Lifetime | 300s | Avoid stale connections |
| Command Timeout | 30s | Adequate for most queries |
| Keepalive | 30s | Detect dead connections |

---

## 10. Summary

| Item | Value |
|------|-------|
| Database Server | PostgreSQL 9.3.4 (embedded, x86) |
| Database Name | CHIFA_OFFICINE |
| Host | localhost |
| Port | 5432 |
| Username | **postgres** |
| Password | **(none)** — trust auth |
| Schema | **public** |
| SSL | Not configured |
| Protocol | Npgsql 8.x compatible |
| Current Status | Backend crash (0xC0000142) |
| Workaround | `postgres --single` mode |
| Critical Fixes Needed | Username, Schema, Connection string |

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
