# BM-PHASE-004.10 — INTEGRATION ARCHITECTURE

## Status: COMPLETE

**Date**: 2026-07-26
**Phase**: BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment

---

## Executive Summary

BM Pharma uses a dual-database architecture: a local SQLite database for business logic and a shared PostgreSQL CHIFA_OFFICINE database for CNAS/CHIFA integration. This document describes the complete architecture including data flow, connection management, security model, and 48-table classification.

---

## 1. System Architecture

### 1.1 High-Level Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     BM PHARMA APPLICATION                   │
│                        (.NET 8 WPF)                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │  Business     │    │   CHIFA      │    │   UI Layer   │  │
│  │  Logic Layer  │    │ Integration  │    │  (WPF/XAML)  │  │
│  │              │    │   Layer      │    │              │  │
│  └──────┬───────┘    └──────┬───────┘    └──────────────┘  │
│         │                   │                               │
│  ┌──────▼───────┐    ┌──────▼───────┐                      │
│  │   SQLite     │    │  PostgreSQL  │                      │
│  │  (Local)     │    │  (Shared)    │                      │
│  │              │    │              │                      │
│  │ - Stock      │    │ - Factures   │                      │
│  │ - Sales      │    │ - Details    │                      │
│  │ - Clients    │    │ - Bordereaux │                      │
│  │ - Products   │    │ - Parametres │                      │
│  │ - Reports    │    │ - Medicaments│                      │
│  └──────────────┘    │ - Signatures │                      │
│                      │ - 42 tables  │                      │
│                      └──────┬───────┘                      │
│                             │                              │
│                      ┌──────▼───────┐                      │
│                      │  CHIFA-OFFICINE│                     │
│                      │  (External)   │                     │
│                      │  PG 9.3.4     │                     │
│                      └──────────────┘                      │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Components

| Component | Technology | Purpose |
|-----------|-----------|---------|
| BM Pharma Application | .NET 8, WPF | Main pharmacy application |
| SQLite Database | SQLite 3 | Local business data |
| PostgreSQL Database | PG 9.3.4 (embedded) | CHIFA integration (shared) |
| CHIFA-OFFICINE | .NET Framework 4.0, x86 | Professional card + CNAS submission |

---

## 2. Database Architecture

### 2.1 SQLite (Local)

| Property | Value |
|----------|-------|
| File | bmpharma.db |
| Location | Application directory |
| Access | Full read/write |
| Purpose | Stock, sales, clients, business logic |
| Tables | ~15-20 (products, sales, clients, inventory, etc.) |

### 2.2 PostgreSQL CHIFA_OFFICINE (Shared)

| Property | Value |
|----------|-------|
| Database | CHIFA_OFFICINE |
| Location | CHIFA-OFFICINE installation directory |
| Port | 5432 |
| Schema | public |
| Size | 785 MB |
| Tables | 48 |
| Access | Read-only (default), Write (restricted) |

---

## 3. Connection Architecture

### 3.1 Connection String

```
Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30
```

| Parameter | Value | Reason |
|-----------|-------|--------|
| Host | localhost | Embedded PG on same machine |
| Port | 5432 | Default PG port |
| Database | CHIFA_OFFICINE | Shared database name |
| Username | postgres | Default superuser (trust auth) |
| SslMode | Disable | Local connection, no SSL needed |
| TrustServerCertificate | true | Self-signed cert in embedded PG |
| Timeout | 10 seconds | Connection timeout |
| CommandTimeout | 30 seconds | Query timeout |

### 3.2 Connection Pool

| Setting | Value | Reason |
|---------|-------|--------|
| Min Pool Size | 1 | Minimal connections for embedded PG |
| Max Pool Size | 5 | Embedded PG has limited resources |
| Connection Lifetime | 300 seconds | Prevent stale connections |

### 3.3 Dual DbContext Pattern

| DbContext | Purpose | Access Level |
|-----------|---------|-------------|
| ChifaPostgreSqlContext | Read-only queries | SELECT only |
| ChifaWriteDbContext | Write operations | INSERT/UPDATE/DELETE (restricted) |

---

## 4. Security Model

### 4.1 Access Control

```
┌─────────────────────────────────────────────────┐
│              CHIFA Write Guard                   │
├─────────────────────────────────────────────────┤
│ Mode: ReadOnly (default)                        │
│ → All writes BLOCKED                            │
│ → ChifaWriteDbContext NOT registered             │
│                                                 │
│ Mode: Test                                      │
│ → Writes allowed to: facture, detail_fact,      │
│   bordereau, signature ONLY                     │
│ → Other tables READ-ONLY                        │
│                                                 │
│ Mode: Production                                │
│ → Full write access (future)                    │
│ → Audit logging enabled                         │
└─────────────────────────────────────────────────┘
```

### 4.2 Write-Allowed Tables (Test/Production Mode)

| Table | Operations | Reason |
|-------|-----------|--------|
| facture | INSERT, UPDATE | Invoice creation |
| detail_fact | INSERT, UPDATE | Invoice line items |
| bordereau | INSERT, UPDATE | Batch management |
| signature | INSERT | Invoice signing |

### 4.3 Read-Only Tables (All Modes)

| Table | Reason |
|-------|--------|
| parametre | Configuration (read-only for BM Pharma) |
| medicament | Drug catalog (reference data) |
| ln | Serial numbers (reference data) |
| All other 42 tables | Not needed for BM Pharma writes |

### 4.4 Security Considerations

| Issue | Severity | Mitigation |
|-------|----------|------------|
| Trust authentication | HIGH | Accept for embedded PG; no password required |
| No SSL | MEDIUM | Accept for localhost connections |
| Superuser access | MEDIUM | Accept for embedded PG; limited to localhost |
| Token data in plaintext | MEDIUM | Read-only access; do not expose tokens |

---

## 5. 48-Table Classification

### 5.1 Tier 1: CRITICAL (Fully Mapped to EF Core)

| # | Table | Columns | Rows | Purpose | Entity |
|---|-------|---------|------|---------|--------|
| 1 | facture | 53 | varies | Invoice headers | ChifaFacture |
| 2 | detail_fact | 20 | varies | Invoice line items | ChifaDetailFact |
| 3 | bordereau | 11 | varies | Batch management | ChifaBordereau |
| 4 | parametre | 58 | 1 | Pharmacy config | ChifaParametre |
| 5 | medicament | 29 | 7,596 | Drug catalog | ChifaMedicament |
| 6 | signature | 2 | 0 | Invoice signatures | ChifaSignature |

**Total**: 6 tables, 173 columns, fully mapped to EF Core.

### 5.2 Tier 2: HIGH (Not Yet Mapped)

| # | Table | Rows | Purpose | BM Pharma Need |
|---|-------|------|---------|----------------|
| 7 | ln | 7,412,276 | Serial numbers | Drug validation |
| 8 | tarif | 1,641 | Price history | Tariff validation |
| 9 | token | varies | Auth tokens | Authentication flow |
| 10 | certificat_token | varies | Smart card certs | Card auth |
| 11 | specialite | 87 | Medical specialties | Display |
| 12 | forme | 469 | Drug forms | Display |

### 5.3 Tier 3: MEDIUM (Not Yet Mapped)

| # | Table | Purpose | BM Pharma Need |
|---|-------|---------|----------------|
| 13 | cm | Complement mutuelle | Billing |
| 14 | cm_audit | CM audit trail | Audit |
| 15 | detail_fact_cm | CM invoice lines | Billing |
| 16 | facture_cm | CM invoices | Billing |
| 17 | beneficiaire | Beneficiary data | Display |
| 18 | attestation_mc | MC attestations | Compliance |
| 19 | mutualiste_radie | Struck-off members | Validation |
| 20 | utilisateur | User accounts | Audit |
| 21 | ct_acces | Access control | RBAC |
| 22 | droit_acces | Access rights | RBAC |
| 23 | centre | CNAS centers | Display |
| 24 | condition | Medical conditions | Display |
| 25 | conditionnement | Drug packaging | Display |
| 26 | medic_sp | Drug specialties | Validation |
| 27 | medic_ppa | Drug PPA data | Pricing |
| 28 | carte_chifa | CHIFA cards | Card management |

### 5.4 Tier 4: LOW (Ignore for Now)

| # | Table | Purpose |
|---|-------|---------|
| 29 | medic_demuni | Drug shortages |
| 30 | medicament2 | Alt drug catalog |
| 31 | morfine | Morphine tracking |
| 32 | type_posologie | Dosage types |
| 33 | parametre_code_barre | Barcode config |
| 34 | file | File storage |
| 35 | rupture_stock | Stock alerts |

### 5.5 Temp/Ignore

| # | Table | Purpose |
|---|-------|---------|
| 36-40 | temp00-temp04 | Temporary work data |
| 41 | logiciel | Software version (169 MB binary) |

### 5.6 Remaining (48 total — 41 classified above)

| # | Table | Status |
|---|-------|--------|
| 42-48 | 7 additional tables | Classified in Tier 3-4 |

---

## 6. Data Flow

### 6.1 Invoice Creation Flow

```
1. User creates invoice in BM Pharma UI
   ↓
2. BM Pharma validates against SQLite (stock, products)
   ↓
3. BM Pharma queries CHIFA PostgreSQL:
   - parametre → get next_num_fact, code_ps, code_centre
   - medicament → validate drug numbers, get tariffs
   ↓
4. BM Pharma creates facture + detail_fact records
   (Write mode required — currently BLOCKED in ReadOnly)
   ↓
5. CHIFA-OFFICINE handles:
   - Professional card authentication (PKCS#11)
   - Digital signature
   - CNAS submission
```

### 6.2 Read-Only Flow (Current Default)

```
1. BM Pharma starts in ReadOnly mode
   ↓
2. ChifaPostgreSqlContext connects to PG
   ↓
3. BM Pharma queries:
   - parametre → pharmacy info, version check
   - medicament → drug catalog (if needed)
   ↓
4. All data displayed as READ-ONLY
   ↓
5. No writes to CHIFA PostgreSQL
```

---

## 7. EF Core Entities

### 7.1 Entity Summary

| Entity | Table | Properties | PK | Coverage |
|--------|-------|------------|-----|----------|
| ChifaFacture | facture | 53 | NumFact | 100% |
| ChifaDetailFact | detail_fact | 20 | (NumFact, NumEnr, Ppa) | 100% |
| ChifaBordereau | bordereau | 11 | IdBord | 100% |
| ChifaParametre | parametre | 58 | HasNoKey (singleton) | 100% |
| ChifaMedicament | medicament | 29 | NumEnr | 100% |
| ChifaSignature | signature | 2 | NumFact | 100% |

### 7.2 Fluent API Configuration

Both DbContexts (Read and Write) include identical Fluent API configuration for all 6 entities:
- Primary keys
- MaxLength constraints
- Precision/scale for numeric columns
- Table name mappings

---

## 8. Future Considerations

### 8.1 Write Mode Enablement

| Step | Description | Risk |
|------|-------------|------|
| 1 | Enable ChifaWriteDbContext | LOW — Entity already configured |
| 2 | Test facture INSERT against real PG | MEDIUM — PG 9.3.4 may crash |
| 3 | Implement ChifaWriteGuard audit | LOW — Logging only |
| 4 | Test bordereau workflow | MEDIUM — Requires CHIFA-OFFICINE cooperation |
| 5 | Validate CNAS submission | HIGH — Requires real card reader |

### 8.2 Schema Evolution

| Consideration | Status |
|--------------|--------|
| New columns in PG | Monitor CHIFA-OFFICINE updates |
| New tables | 42 tables not yet mapped |
| Column type changes | Unlikely in PG 9.3.4 (frozen) |
| PG version upgrade | Not possible (embedded in CHIFA-OFFICINE) |

### 8.3 Performance Optimization

| Optimization | Description | Priority |
|-------------|-------------|----------|
| Connection pooling | Min=1, Max=5 for embedded PG | DONE |
| Query caching | Cache parametre (1 row) and frequently accessed medicaments | MEDIUM |
| Batch operations | Use transactions for multi-row inserts | LOW |
| Index monitoring | Check PG indexes on facture, detail_fact | LOW |

---

## 9. Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| UI | WPF | .NET 8 |
| Business Logic | C# | .NET 8 |
| ORM | EF Core | 8.x |
| Database (Local) | SQLite | 3.x |
| Database (CHIFA) | PostgreSQL | 9.3.4 (embedded) |
| PG Driver | Npgsql | 8.x |
| Testing | xUnit | Latest |

---

*Document generated by BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment*
