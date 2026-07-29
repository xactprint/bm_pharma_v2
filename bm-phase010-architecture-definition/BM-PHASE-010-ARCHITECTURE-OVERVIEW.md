# BM-PHASE-010 — ARCHITECTURE OVERVIEW

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE — RECOMMENDED ARCHITECTURE
**Source of Truth:** Phases 001–009-C investigations
**Previous Version:** BM_PHARMA_CHIFA_INTEGRATION_ARCHITECTURE.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial architecture based on BM-SPEC-028-032 |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with Phases 004–009-C real findings |

---

## Architectural Principle

> BM Pharma automates everything that can be automated.
> CHIFA-OFFICINE remains the authority for proprietary operations requiring the professional token.

## Recommended Architecture: OPTION D (Hybrid)

**Status: RECOMMENDED ARCHITECTURE** — CONFIRMED by Phases 005, 007, 008, 009, 009-C.

Option D is the final recommended architecture. BM Pharma writes `facture` and `detail_fact` directly to PostgreSQL. CHIFA-OFFICINE retains exclusive control over bordereau management, signing, closure, and CNAS transmission.

### Why Option D

| Factor | Assessment | Source |
|--------|-----------|--------|
| BM Pharma writes facture/detail_fact | CONFIRMED — works via EF Core | Phase 007-G |
| BM Pharma creates bordereau | PARTIAL — INSERT works but invisible in CHIFA UI | Phase 009 (TST003) |
| BM Pharma signs | NOT SUPPORTED — requires hardware token | Phase 008-A |
| BM Pharma closes | NOT SUPPORTED — requires cloturerbord() | Phase 008-A |
| BM Pharma transmits to CNAS | NOT SUPPORTED — requires CHIFA workflow | Phase 008-A |
| detail_bord = PostgreSQL table | FALSE — it is a .NET DataTable | Phase 009-C |
| detail_bord filling algorithm | UNKNOWN — runtime mechanics not fully determined | Phase 009-C |
| Option A (full BM Pharma control) | NOT RECOMMENDED — bordereau invisibility risk confirmed | Phase 009 |

---

## Integration Model

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        BM Pharma (.NET 8)                               │
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌───────────┐  │
│  │ Domain (DDD) │  │ Application  │  │ Persistence  │  │ CHIFA     │  │
│  │ Entities     │  │ MediatR      │  │ SQLite (BM)  │  │ Layer     │  │
│  │ Rules        │  │ Validation   │  │ PostgreSQL   │  │ Services  │  │
│  └──────────────┘  └──────────────┘  └──────┬───────┘  └─────┬─────┘  │
│                                              │                │        │
│                                     ┌───────┴────────────────┴──┐     │
│                                     │   Npgsql 8.x (EF Core 8) │     │
│                                     └───────┬───────────────────┘     │
└─────────────────────────────────────────────┼─────────────────────────┘
                                               │
┌──────────────────────────────────────────────▼─────────────────────────┐
│              PostgreSQL 9.3.4 (CHIFA-OFFICINE)                         │
│              Host: 127.0.0.1:5432 | DB: CHIFA_OFFICINE                │
│              User: pharm | Schema: public                               │
│                                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────┐  │
│  │ facture  │  │detail_fact│ │ bordereau │  │medicament│  │parametre│ │
│  │ (53 cols)│  │ (20 cols)│  │ (11 cols) │  │ (29 cols)│  │(58 cols)│ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └────────┘  │
│                                                                         │
│  ┌──────────┐  ┌──────────┐  ┌────── + 42 more tables ──────┐        │
│  │ signature │  │ ln       │  │ (48 total discovered)        │        │
│  │ (2 cols)  │  │(9,7M rows)│ └───────────────────────────────┘        │
│  └──────────┘  └──────────┘                                            │
└──────────────────────────────────────┬──────────────────────────────────┘
                                       │
┌──────────────────────────────────────▼──────────────────────────────────┐
│                        CHIFA-OFFICINE (WinForms .NET)                   │
│                                                                         │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────────────┐  │
│  │ FBordereau│  │ VenteChifa│  │Consultation│  │ detail_bord       │  │
│  │ Bordereau │  │ Invoice   │  │ Facture    │  │ (.NET DataTable)  │  │
│  │ Workflow  │  │ Entry     │  │            │  │ (NOT a PG table)  │  │
│  └─────┬─────┘  └───────────┘  └───────────┘  └───────────────────┘  │
│        │                                                               │
│  ┌─────▼─────────────────────────────────────────────────────────┐    │
│  │  Identiv uTrust 3512 / SAM (Hardware Token → PKCS#7 Signing)  │    │
│  └───────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ┌────────────────────────────────────────────────────────┐            │
│  │  cloturerbord() — PostgreSQL function → FTP 41.111.149.250 → CNAS  │
│  └────────────────────────────────────────────────────────┘            │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Layer Responsibilities

| Layer | Responsibility | Read | Write | Status |
|-------|---------------|------|-------|--------|
| Domain | Business entities, DDD rules, Value Objects | Own DB | Own DB | CONFIRMED |
| Application | MediatR use cases, orchestration, validation | Own DB | Own DB | CONFIRMED |
| Persistence.SQLite | BM Pharma local storage (invoices, products, customers) | SQLite | SQLite | CONFIRMED |
| Persistence.PostgreSQL | CHIFA PostgreSQL EF Core context | PG (48 tables) | PG (4 tables only) | CONFIRMED |
| CHIFA Integration | PostgreSQL bridge, WriteGuard, audit | CHIFA tables | facture, detail_fact | CONFIRMED |
| CHIFA-OFFICINE | Bordereau workflow, signing, closure, transmission | PG tables | bordereau, signature | NOT SUPPORTED by BM |
| CNAS | National healthcare reimbursement | FTP | FTP | NOT SUPPORTED by BM |

## CHIFA-OFFICINE Responsibilities (BM Pharma cannot replace)

| Operation | Detail | Evidence | Status |
|-----------|--------|----------|--------|
| Signature hardware | Identiv uTrust 3512, PKCS#7 via p7sign.dll | Phase 008-A | NOT SUPPORTED |
| Bordereau closure | `cloturerbord()` PostgreSQL function | Phase 008-A | NOT SUPPORTED |
| CNAS transmission | FTP to 41.111.149.250:21 | Phase 008-A | NOT SUPPORTED |
| detail_bord population | .NET DataTable runtime algorithm | Phase 009-C | UNKNOWN algorithm |
| Visualiser Bordereau | CHIFA UI for bordereau listing | Phase 009 | PARTIAL visibility |

---

## Data Flow: Invoice Preparation (Recommended)

```
BM Pharma (SQLite)                    PostgreSQL (CHIFA)            CHIFA-OFFICINE
─────────────────────────             ───────────────────            ──────────────
1. Create invoice (local)
2. Validate CHIFA rules
3. INSERT facture              ──►    facture row
4. INSERT detail_fact          ──►    detail_fact row
5. Mark "Persisted"
6. (User opens CHIFA)                                    ◄──       Consultation Facture
                                                                   → Shows BM Pharma invoice
7. (User creates bordereau in CHIFA)                     ◄──       Vente Chifa → FBordereau
                                                                   → detail_bord DataTable
                                                                   → Signature (token)
                                                                   → Cloture
                                                                   → FTP CNAS
8. SELECT facture.etat         ◄──    etat = 'S' (signed) ←──     CHIFA updates etat
9. Mark "Signed"
10. SELECT bordereau.etat      ◄──    etat = 'C' (closed) ←──     CHIFA updates etat
11. Mark "Closed"
12. SELECT date_depot_ftp      ◄──    date_depot_ftp set  ←──     CHIFA updates date
13. Mark "Transmitted"
```

---

## Service Boundaries

| Service | Responsibility | Real Implementation | Status |
|---------|---------------|-------------------|--------|
| IChifaIntegrationService | Health check, mode check | ChifaPostgreSqlContext | CONFIRMED |
| IChifaInvoiceService | Validate + write invoice | ChifaPostgresInvoiceService | CONFIRMED (Phase 007) |
| IChifaBordereauService | Bordereau operations | ChifaPostgresBordereauService | PARTIAL (write ok, visibility limited) |
| IChifaTokenService | Token detection | ChifaSigningServiceStub | CONFIRMED (delegates to CHIFA) |
| IChifaNumberingService | Atomic counter | ChifaNumberingService | CONFIRMED (UPDATE...RETURNING) |
| IChifaAuditService | Structured audit | ChifaAuditService | CONFIRMED |
| IChifaInvoiceWorkflowService | Full workflow orchestration | ChifaInvoiceWorkflowService | CONFIRMED |
| IBordereauStatusService | Bordereau monitoring | BordereauStatusService | CONFIRMED |
| ChifaWriteGuard | Write protection (3 layers) | Runtime guard | CONFIRMED |

---

## State Distinction (Critical)

> DATABASE WRITE ≠ CHIFA-OFFICINE VISIBILITY

BM Pharma MUST distinguish these states explicitly:

| State | Meaning | Evidence |
|-------|---------|----------|
| Persisted | Row exists in CHIFA PostgreSQL | CONFIRMED — Phase 007 |
| VisibleInChifa | Row visible in CHIFA-OFFICINE UI | PARTIAL — TST001 visible, TST003 partial |

---

## Key Certitude Matrix

| Element | Status | Source |
|---------|--------|--------|
| PostgreSQL real (9.3.4, 32-bit) | CONFIRMED | Phase 004.12 |
| EF Core mapping (6 entities) | CONFIRMED | Phase 004.10 |
| Write facture + detail_fact via EF Core | CONFIRMED | Phase 007 |
| Rollback to exact baseline | CONFIRMED | Phase 005, 007 |
| Atomic counters (UPDATE...RETURNING) | CONFIRMED | Phase 006 |
| ReadOnly 3-layer protection | CONFIRMED | Phase 004.8, 004.12 |
| Invoice visible in CHIFA | CONFIRMED | Phase 008 |
| Bordereau visible in CHIFA | PARTIAL | TST003 |
| detail_bord = .NET DataTable | CONFIRMED | Phase 009-C |
| detail_bord filling algorithm | UNKNOWN | Phase 009-C |
| 4 SQL query families | CONFIRMED | Phase 009-C |
| Signing from BM Pharma | NOT SUPPORTED | Phase 008 |
| Cloture from BM Pharma | NOT SUPPORTED | Phase 008 |
| CNAS transmission from BM Pharma | NOT SUPPORTED | Phase 008 |
| Credentials in plaintext (CHIFA memory) | CONFIRMED | Phase 009-C |
