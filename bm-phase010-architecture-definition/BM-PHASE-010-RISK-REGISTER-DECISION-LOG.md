# BM-PHASE-010 — RISK REGISTER & DECISION LOG

**Version:** 1.0 (Phase 010 creation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** All Phases 001-009-C

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-29 | BM Pharma | Consolidated risk register from all phases |

---

## Part 1: Risk Register

### Critical Risks (Impact: HIGH)

| ID | Risk | Impact | Probability | Detection | Mitigation | Status |
|----|------|--------|-------------|-----------|------------|--------|
| R01 | PG 9.3.4 crash during write | Data loss / partial write | LOW | Transaction rollback | Auto-rollback on any exception | CONFIRMED |
| R02 | PG 9.3.4 EOL (no patches) | Security vulnerability | MEDIUM | — | Accept for localhost embedded use | CONFIRMED |
| R03 | Trust auth (no password) | Unauthorized DB access | MEDIUM | — | Restricted to localhost only | CONFIRMED |
| R04 | Credentials in CHIFA process memory | Credential leakage | HIGH | Windbg scan (Phase 009-C) | Accept — single-pharmacist workstation | CONFIRMED |
| R05 | SQL queries in CHIFA process memory | Reverse engineering | MEDIUM | Windbg scan (Phase 009-C) | Accept — compiled constants, no secrets in SQL | CONFIRMED |
| R06 | Bordereau invisible in CHIFA UI | User cannot process bordereau | HIGH | TST003 (Phase 009) | Option D: BM Pharma does not create bordereaux | CONFIRMED |
| R07 | detail_bord algorithm unknown | Cannot replicate BM Pharma's bordereau workflow | HIGH | Phase 009-C | Accept — CHIFA handles natively | CONFIRMED |
| R08 | Npgsql 8.x incompatible with PG 9.3 | Connection/write failure | LOW | Phase 004.12 | CONFIRMED working via TCP | CONFIRMED |
| R09 | FK ordering bug (facture before detail_fact) | FK violation 23503 | LOW | Phase 007-G | Two SaveChangesAsync calls | CONFIRMED FIXED |
| R10 | DateTime Kind mismatch (Local vs Unspecified) | Write error | LOW | Phase 007-G | DateTime.SpecifyKind | CONFIRMED FIXED |
| R11 | Column type mismatch (xml, timestamp, date) | Write error | LOW | Phase 007-G | HasColumnType fluent API | CONFIRMED FIXED |

### Medium Risks

| ID | Risk | Impact | Probability | Mitigation | Status |
|----|------|--------|-------------|------------|--------|
| R12 | Counter collision (same number assigned twice) | Data integrity | LOW | Atomic UPDATE...RETURNING | CONFIRMED |
| R13 | No RBAC implemented | Unauthorized operations | MEDIUM | Documented limitation; implement before production | NOT IMPLEMENTED |
| R14 | AdminPassword hardcoded in appsettings | Credential exposure | MEDIUM | Externalize to UserSecrets before production | NOT IMPLEMENTED |
| R15 | Token values in parametre table in plaintext | Token exposure | MEDIUM | Read-only access; do not log | PARTIAL |
| R16 | PG single-user mode (postgres --single) bypasses auth | Unrestricted access | LOW | BM Pharma uses TCP, not single-user mode | CONFIRMED |
| R17 | Facture written but not visible in CHIFA UI | Operational confusion | MEDIUM | Documented state distinction (Persisted ≠ Visible) | CONFIRMED |
| R18 | Counter consumed but write fails | "Lost" invoice number | LOW | By design — prevents future collisions | CONFIRMED |

### Low Risks

| ID | Risk | Impact | Probability | Mitigation | Status |
|----|------|--------|-------------|------------|--------|
| R19 | 7 dropped columns in facture (attnum gaps) | Schema mismatch | LOW | Not mapped; columns don't exist | CONFIRMED |
| R20 | 42 parametre columns unmapped | Cannot read all config | LOW | Not needed for BM Pharma operations | CONFIRMED |
| R21 | 48 tables discovered, only 6 mapped | Unknown schema surface | LOW | Remaining tables read-only | CONFIRMED |
| R22 | InMemory tests ≠ real PG behavior | False confidence | LOW | Real PG tests executed (TST001, TST002) | CONFIRMED |

---

## Part 2: Architecture Decision Log

### Decision D01: Architecture Selection

| Aspect | Detail |
|--------|--------|
| Decision | Adopt Option D (Hybrid) as recommended architecture |
| Options considered | A (Direct write all), B (Staging tables), C (Replicate detail_bord), D (Hybrid) |
| Rationale | TST003 confirmed bordereau invisibility; detail_bord is DataTable (not PG table); algorithm unknown |
| Risk | Less automation; pharmacist must create bordereaux in CHIFA natively |
| Source | Phase 009-ARCHITECTURE-RECOMMENDATION.md |
| Date | 2026-07-28 |
| Status | CONFIRMED |

### Decision D02: BM Pharma Writes Only facture + detail_fact

| Aspect | Detail |
|--------|--------|
| Decision | BM Pharma inserts facture and detail_fact only. Does NOT create bordereaux. |
| Rationale | Bordereau writing leads to invisibility in Visualiser Bordereau (TST003) |
| Risk | Pharmacist needs to create bordereau manually in CHIFA |
| Source | Phase 009, Phase 009-C |
| Date | 2026-07-29 |
| Status | CONFIRMED |

### Decision D03: Two SaveChangesAsync Calls

| Aspect | Detail |
|--------|--------|
| Decision | Save facture first, then detail_fact (separate SaveChangesAsync calls) |
| Rationale | EF Core alphabetical ordering causes FK violation (detail_fact before facture) |
| Alternative | Could configure FK in OnModelCreating to control ordering, but two calls is simpler |
| Source | Phase 007-G (bug fix) |
| Date | 2026-07-27 |
| Status | CONFIRMED |

### Decision D04: Atomic Counters via UPDATE...RETURNING

| Aspect | Detail |
|--------|--------|
| Decision | Use atomic UPDATE...RETURNING instead of SELECT FOR UPDATE for counter management |
| Rationale | Single atomic operation, no TOCTOU race condition, returns new value directly |
| Alternative | SELECT FOR UPDATE + separate UPDATE (more round trips, more locking) |
| Source | Phase 006-D |
| Date | 2026-07-27 |
| Status | CONFIRMED |

### Decision D05: ReadOnly Default

| Aspect | Detail |
|--------|--------|
| Decision | BM Pharma ships in ReadOnly mode. Writes require explicit user action. |
| Rationale | Safety: prevent accidental writes to production CHIFA database |
| Implementation | 3-layer protection: DI wiring, WriteGuard, UI |
| Source | Phase 002, Phase 004.12 |
| Date | 2026-07-25 |
| Status | CONFIRMED |

### Decision D06: DateTime.SpecifyKind(..., Unspecified)

| Aspect | Detail |
|--------|--------|
| Decision | All DateTime values written to CHIFA PG must use DateTimeKind.Unspecified |
| Rationale | PG `timestamp without time zone` rejects Local and Utc kinds in Npgsql 8.x |
| Bug found | Phase 007-G — DateTime.Now returns Local, causing EF Core error |
| Source | Phase 007-G |
| Date | 2026-07-27 |
| Status | CONFIRMED FIXED |

### Decision D07: HasColumnType for xml, timestamp, date

| Aspect | Detail |
|--------|--------|
| Decision | Use HasColumnType fluent API for columns with non-default PG types |
| Columns | signature (xml), fact_xml (xml), date_fact (timestamp without time zone), date_soin (date) |
| Rationale | EF Core defaults to varchar/nvarchar for string fields and timestamp for DateTime |
| Source | Phase 007-G |
| Date | 2026-07-27 |
| Status | CONFIRMED FIXED |

### Decision D08: User `pharm` (not `postgres`)

| Aspect | Detail |
|--------|--------|
| Decision | Connect to CHIFA PG as user `pharm` |
| Rationale | `pharm` is the database owner and superuser. `postgres` initially had no CONNECT privilege. |
| Alternative | `postgres` after GRANT CONNECT (Phase 004.11), but `pharm` is the production user |
| Source | Phase 004.11 |
| Date | 2026-07-27 |
| Status | CONFIRMED |

### Decision D09: Schema `public` (not `cnas`)

| Aspect | Detail |
|--------|--------|
| Decision | All tables are in the `public` schema, not `cnas` |
| Rationale | Initial spec (ANALYSE_PROJET.md) suggested `cnas` schema. Real DB confirmed `public`. |
| Source | Phase 004.9, Phase 004.10 |
| Date | 2026-07-26 |
| Status | CONFIRMED |

### Decision D10: 6 EF Core Entities (not 4)

| Aspect | Detail |
|--------|--------|
| Decision | Map 6 entities: ChifaFacture, ChifaDetailFact, ChifaBordereau, ChifaParametre, ChifaMedicament, ChifaSignature |
| Rationale | Real PG discovery revealed 48 tables. These 6 are critical for BM Pharma operations. |
| Source | Phase 004.10 |
| Date | 2026-07-26 |
| Status | CONFIRMED |

### Decision D11: Never Automate Signature/Closure/Transmission

| Aspect | Detail |
|--------|--------|
| Decision | BM Pharma NEVER attempts signing, closure, or CNAS transmission. All delegated to CHIFA-OFFICINE. |
| Rationale | Regulatory: professional token (Identiv uTrust 3512), PKCS#7, cloturerbord(), FTP to CNAS |
| Risk | Pharmacist must manually perform these steps in CHIFA |
| Source | Phase 008-A |
| Date | 2026-07-28 |
| Status | CONFIRMED |

### Decision D12: Keep v1.0 Contracts as Archive

| Aspect | Detail |
|--------|--------|
| Decision | Preserve original 8 contracts as historical reference. Create updated v2.0 versions. |
| Rationale | v1.0 contracts represent the initial architecture based on BM-SPEC (before real discovery). v2.0 reflects reconciled reality. |
| Source | Phase 010 |
| Date | 2026-07-29 |
| Status | CONFIRMED |

---

## Part 3: Certitude Matrix (Complete)

| Element | Status | Source | Limitation |
|---------|--------|--------|------------|
| PostgreSQL real (9.3.4 32-bit) | CONFIRMED | Phase 004.12 | EOL, crash-prone |
| EF Core mapping (6 entities) | CONFIRMED | Phase 004.10 | 42 unmapped parametre columns |
| Write facture + detail_fact via EF Core | CONFIRMED | Phase 007 | Requires mode ≠ ReadOnly |
| Rollback to exact baseline | CONFIRMED | Phases 005, 007 | Manual DELETE FROM |
| Atomic counters | CONFIRMED | Phase 006 | — |
| ReadOnly 3-layer protection | CONFIRMED | Phase 004.12 | — |
| Invoice visible in CHIFA UI | CONFIRMED | Phase 008 | Requires user to open CHIFA |
| Bordereau visible in CHIFA UI | PARTIAL | TST003 | Cause not fully determined |
| detail_bord = PostgreSQL table | FALSE | Phase 009-C | Confirmed .NET DataTable |
| detail_bord = .NET DataTable | CONFIRMED | Phase 009-C | Filling algorithm partially unknown |
| SQL bordereau listing (4 families) | CONFIRMED | Phase 009-C | Runtime parameters partially unknown |
| Signature from BM Pharma | NOT SUPPORTED | Phase 008-A | Hardware token required |
| Cloture from BM Pharma | NOT SUPPORTED | Phase 008-A | cloturerbord() PG function |
| CNAS transmission from BM Pharma | NOT SUPPORTED | Phase 008-A | FTP workflow in CHIFA |
| Credentials in CHIFA memory | CONFIRMED | Phase 009-C | Plaintext connection string |
| SQL in CHIFA memory | CONFIRMED | Phase 009-C | Compiled constants at startup |
| 509 tests passing | CONFIRMED | Phase 004.12 | — |
| TST001 rollback (baseline restored) | CONFIRMED | Phase 005 | 12/12 checks |
| TST002 rollback (baseline restored) | CONFIRMED | Phase 007 | 10/10 checks |
| TST003 rollback (baseline restored) | CONFIRMED | Phase 008 | — |
| RBAC implemented | NOT IMPLEMENTED | Phase 004.7 | Required before production |
| AdminPassword externalized | NOT IMPLEMENTED | Phase 004.7 | Currently hardcoded |
