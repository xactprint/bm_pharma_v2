# BM-PHASE-004.9 — REAL CHIFA-OFFICINE ENVIRONMENT

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery
**Previous**: BM-PHASE-004.8 — First CHIFA Real Schema Discovery (BLOCKED)

---

## Executive Summary

BM-PHASE-004.9 successfully discovered the complete CHIFA-OFFICINE environment by using PostgreSQL `--single` (single-user mode) to bypass the crashed multi-user backend. This document captures the full runtime environment of the CHIFA-OFFICINE application as deployed on the production workstation.

---

## 1. CHIFA-OFFICINE Application

### 1.1 Executable

| Property | Value |
|----------|-------|
| File | `CHIFA_OFFICINE.exe` |
| Framework | .NET Framework 4.0 (Client Profile) |
| Runtime | CLR 4.0 |
| Architecture | 32-bit (x86) |
| Purpose | Pharmacist workstation client for CNAS/CHIFA claims management |

### 1.2 Embedded PostgreSQL

| Property | Value |
|----------|-------|
| Version | **9.3.4** |
| Architecture | 32-bit (x86) |
| EOL Status | **End-of-Life since 2018-11-12** |
| Embedded In | `CHIFA_OFFICINE.exe` (Bundled via pgEmbed or similar) |
| Data Directory | CHIFA-OFFICINE installation folder |
| Port | 5432 (default) |
| Supplied Extensions | `dblink`, `pg_stat_statements` |

### 1.3 Npgsql DLL

| Property | Value |
|----------|-------|
| File | `Npgsql.dll` |
| Version | **2.x** (circa 2013) |
| Target | .NET Framework |
| Incompatibility | **INCOMPATIBLE** with Npgsql 8.x used by BM Pharma |
| Impact | BM Pharma cannot use the same Npgsql.dll; must use its own Npgsql 8.x |

---

## 2. Configuration Files

### 2.1 CONFIG.xml

```xml
<!-- Primary application configuration -->
<SERVER_IP>localhost</SERVER_IP>
<!-- NO database name specified -->
<!-- NO username specified -->
<!-- NO port specified -->
<!-- NO password specified -->
```

**Analysis**: The CONFIG.xml only specifies the server IP. All other connection parameters are either hardcoded in the application or derived from other config files.

### 2.2 settings.cfg

Contains smart card reader configuration for CNAS card authentication:

| Reader Brand | Model | Purpose |
|-------------|-------|---------|
| Gemalto | (various) | CNAS beneficiary card reading |
| Identiv | uTrust 3512 SAM | CNAS smart card reader |

**Purpose**: Reads beneficiary identity cards for CNAS claims processing.

### 2.3 CHIFA_OFFICINE.exe.config

.NET application configuration file. Standard .NET Framework 4.0 Client Profile config.

### 2.4 NLog.config

Logging configuration for the CHIFA-OFFICINE application. Logs are written to local files.

### 2.5 Lancer_Serveur.bat

```batch
@echo off
set PGUSER=postgres
set PGDATABASE=postgres
cd /d "%~dp0"
pg_ctl start -D . -w -t 30
```

**Key Parameters**:
- `PGUSER=postgres` — Default superuser
- `PGDATABASE=postgres` — Default maintenance database
- Starts PostgreSQL using `pg_ctl` from the current directory

### 2.6 pg_hba.conf

```
# TYPE  DATABASE  USER      ADDRESS           METHOD
host    all       all       0.0.0.0/0         trust
host    all       all       192.168.0.0/16    trust
host    all       all       10.0.0.0/8        trust
```

**Analysis**:
- **Authentication method**: `trust` (no password required)
- **Network scope**: Accepts connections from ALL hosts (0.0.0.0/0)
- **No password protection**: Any client can connect as any user
- **No SSL/TLS**: Unencrypted connections

### 2.7 postgresql.conf

| Parameter | Value | Analysis |
|-----------|-------|----------|
| `port` | 5432 | Default PostgreSQL port |
| `listen_addresses` | `*` | Listens on ALL network interfaces |
| `shared_buffers` | 128MB | Modest buffer size |
| `wal_level` | `minimal` | Minimal WAL — reduces crash recovery capability |
| `max_wal_senders` | 0 | No replication |

---

## 3. Connection String Analysis

### 3.1 Actual Connection Parameters (Discovered)

| Parameter | Value | Source |
|-----------|-------|--------|
| Server | localhost | CONFIG.xml |
| Port | 5432 | postgresql.conf |
| Database | **CHIFA_OFFICINE** | pg_database (OID 16394) |
| User | **postgres** | pg_hba.conf / Lancer_Serveur.bat |
| Password | *(none)* | trust auth in pg_hba.conf |
| Schema | **public** | Real schema discovery |

### 3.2 BM Pharma's Assumed Connection (WRONG)

| Parameter | BM Pharma Value | Correct Value |
|-----------|----------------|---------------|
| Server | localhost | localhost ✅ |
| Port | 5432 | 5432 ✅ |
| Database | CHIFA_OFFICINE | CHIFA_OFFICINE ✅ |
| User | **pharm** ❌ | **postgres** ✅ |
| Password | **pharm** ❌ | *(none)* — trust auth ✅ |

### 3.3 Recommended Connection String (Npgsql 8.x)

```
Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;SSL Mode=Disable
```

**No password required** due to trust authentication in pg_hba.conf.

---

## 4. DLL Version Conflict

### 4.1 The Conflict

| Component | CHIFA-OFFICINE | BM Pharma |
|-----------|---------------|-----------|
| Npgsql.dll | **2.x** (2013) | **8.x** |
| .NET Framework | 4.0 Client Profile | .NET 8+ |
| Target | x86 | AnyCPU / x64 |

### 4.2 Impact

- CHIFA-OFFICINE ships its own Npgsql.dll v2.x
- BM Pharma uses Npgsql 8.x which is a complete rewrite
- The two cannot share the same Npgsql.dll in the same process
- BM Pharma must ship its own Npgsql 8.x alongside the CHIFA-OFFICINE installation

### 4.3 Resolution

BM Pharma uses **Npgsql 8.x** via EF Core 8.x / NuGet. This is loaded into the BM Pharma process (separate from CHIFA-OFFICINE.exe). The two processes connect to the same PostgreSQL server on port 5432 independently.

---

## 5. The 0xC0000142 Crash Issue

### 5.1 Problem

When attempting to start the CHIFA-OFFICINE embedded PostgreSQL in multi-user mode, backend processes crash with:

```
Error code: 0xC0000142
Description: DLL initialization failed
```

### 5.2 Root Cause

- PostgreSQL 9.3.4 is a 32-bit binary
- Modern Windows 10/11 64-bit has issues loading 32-bit DLLs in certain contexts
- The crash occurs during DLL initialization in the PostgreSQL backend process
- This prevents any normal TCP/IP connection to the database

### 5.3 Workaround: Single-User Mode

```bash
# Start PostgreSQL in single-user mode (no TCP/IP, no multi-user access)
postgres --single -D <datadir> CHIFA_OFFICINE

# Then run SQL queries directly:
SELECT version();
SELECT * FROM pg_database;
```

**Limitations**:
- Only one process can connect at a time
- No TCP/IP connections allowed
- Must be run from the PostgreSQL installation directory
- Write operations are possible but dangerous — **we only performed READ operations**

---

## 6. Production Database State

### 6.1 pg_controldata

| Parameter | Value |
|-----------|-------|
| Database System Identifier | 6766513065647143912 |
| pg_control version | 937 |
| CATALOG_VERSION_NO | 201309031 |
| State | **in production** |
| NextOID | **8,835,983** |
| Time of last checkpoint | Recent |

### 6.2 Implications

- The database is **actively in production** — not a test/development instance
- High OID count (8.8M) indicates significant object creation over time
- **Read-only discovery is the ONLY acceptable approach** until integration is fully validated

---

## 7. Environment Diagram

```
┌──────────────────────────────────────────────────────┐
│                    WORKSTATION                        │
│                                                      │
│  ┌──────────────────────────────────────────────┐    │
│  │           CHIFA-OFFICINE.exe                  │    │
│  │           (.NET 4.0, x86)                     │    │
│  │                                               │    │
│  │  ┌─────────────┐  ┌──────────────────────┐   │    │
│  │  │ Npgsql 2.x  │  │   Smart Card Reader  │   │    │
│  │  └──────┬──────┘  │   (Gemalto/Identiv)  │   │    │
│  │         │         └──────────────────────┘   │    │
│  └─────────┼────────────────────────────────────┘    │
│            │                                          │
│  ┌─────────▼────────────────────────────────────┐    │
│  │     Embedded PostgreSQL 9.3.4 (x86)          │    │
│  │     Port: 5432                                │    │
│  │     Auth: trust (no password)                 │    │
│  │     Database: CHIFA_OFFICINE                  │    │
│  └─────────▲────────────────────────────────────┘    │
│            │                                          │
│  ┌─────────┴────────────────────────────────────┐    │
│  │           BM Pharma (.NET 8+)                │    │
│  │           Npgsql 8.x + EF Core 8             │    │
│  │           Read: SELECT only                   │    │
│  │           Write: INSERT INTO facture/detail.. │    │
│  └──────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

---

## 8. Key Findings Summary

| # | Finding | Impact |
|---|---------|--------|
| 1 | Database name is `CHIFA_OFFICINE` (not unknown) | Connection string correctable |
| 2 | Username is `postgres` (not `pharm`) | **CRITICAL** — BM Pharma uses wrong user |
| 3 | No password (trust auth) | Simplifies connection |
| 4 | PostgreSQL 9.3.4 (x86, EOL) | Compatibility risk |
| 5 | Npgsql 2.x vs 8.x conflict | DLL isolation required |
| 6 | 0xC0000142 crash in multi-user mode | Single-user mode workaround |
| 7 | Production database (8.8M OIDs) | Read-only until validated |
| 8 | Schema is `public` (not `cnas`) | All tables in public schema |

---

## 9. Security Implications

See **BM-PHASE-004.9-SECURITY-REPORT.md** for complete security analysis.

Key concern: `trust` authentication + `listen_addresses='*'` + `0.0.0.0/0` in pg_hba.conf means **any device on the network can connect without a password**.

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
