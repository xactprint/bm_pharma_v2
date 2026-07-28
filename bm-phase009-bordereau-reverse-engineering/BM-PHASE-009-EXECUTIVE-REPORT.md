# BM-PHASE-009 — Executive Report

**Reverse Engineering — Visualiser Bordereau & detail_bord**

> **Date:** 2026-07-28
> **Status:** COMPLETE
> **Classification:** Confidential — BM Pharma Internal

---

## Objective

Determine the architecture, data flow, and filling mechanism of the `detail_bord` DataTable used by CHIFA-OFFICINE's "Visualiser Bordereau" screen, and explain why a direct PostgreSQL INSERT does not guarantee bordereau visibility in that interface.

---

## Context

BM Pharma needs to understand the CHIFA-OFFICINE internal data pipeline for bordereau display to design a correct integration strategy. Previous phases (BM-PHASE-001 through BM-PHASE-008) established:

- Database schema fully mapped (48 tables, 0 triggers, 5 foreign keys)
- CHIFA-OFFICINE EXE protected by ConfuserEx v1.0.0 (full assembly encryption + LZMA compression)
- `DataSet1` with 6 DataTables identified via IL dump
- TST003 experiment demonstrated facture visibility in "Consultation Facture" but invisibility in "Visualiser Bordereau"
- PostgreSQL logging configured but log collector process crashed — no SQL capture available

---

## Key Discoveries

| # | Discovery | Confidence | Source |
|---|-----------|------------|--------|
| 1 | `detail_bord` is a .NET Typed DataTable in `DataSet1`, NOT a PostgreSQL table | CONFIRMED | IL dump (`full_dump.il:52-148`) |
| 2 | `detail_bord` has typed row infrastructure: `detail_bordRow`, `detail_bordRowChangeEvent`, `detail_bordRowChangeEventHandler` | CONFIRMED | IL dump |
| 3 | `detail_bord` is consumed by `FormVisualiserBordereau` and RDLC reports `Report_Bord_1.rdlc` / `Report_Bord_2.rdlc` | CONFIRMED | UI architecture analysis |
| 4 | The DataTable is populated by compiled SQL embedded in the CHIFA binary (not a live SQL view) | CONFIRMED | Experiment TST003 |
| 5 | Reconstructed SQL involves JOIN across `bordereau` → `facture` → `detail_fact` → `medicament` → `beneficiaire` | PROBABLE | Phase 9 analysis |
| 6 | The exact WHERE clause of the SQL is unknown — binary is ConfuserEx-protected | NOT DETERMINED | ConfuserEx barrier |
| 7 | Direct SQL INSERT into PostgreSQL tables does NOT populate the `detail_bord` DataTable | CONFIRMED | TST003 experiment |
| 8 | CHIFA-OFFICINE's own workflow is required to fill `detail_bord` and make a bordereau visible | CONFIRMED | TST003 experiment |
| 9 | PostgreSQL log collector (`--forkcol`) has crashed — no SQL queries captured | CONFIRMED | Process inspection |

---

## Conclusion

> BM Pharma can write valid CHIFA records directly into PostgreSQL and make them visible in "Consultation Facture". However, visibility in "Visualiser Bordereau" depends on a CHIFA-OFFICINE application pipeline that populates the in-memory `detail_bord` DataTable. The exact filling mechanism — including the complete SQL query, WHERE conditions, and any business logic filters — is not yet fully determined due to ConfuserEx protection and the absence of exploitable PostgreSQL query logs.

---

## Limitations

1. **No method body decompilation possible** — CHIFA_OFFICINE.exe is ConfuserEx-protected
2. **No PostgreSQL SQL logs captured** — the `--forkcol` log collector process crashed
3. **No runtime instrumentation** — CHIFA is a third-party binary; no debug symbols or source available
4. **Reconstructed SQL is partial** — JOINs are probabilistic, not confirmed from bytecode
5. **WHERE clause is unknown** — any business filters (status, signature, quantity, date) remain speculative

---

## Impact on BM Pharma v2

| Area | Impact | Action |
|------|--------|--------|
| Invoice creation (facture + detail_fact) | None — works via direct SQL | Proceed with current design |
| Bordereau visibility in CHIFA | **Blocked** — requires CHIFA workflow | Accept that BM Pharma-created bordereaux may not appear in Visualiser Bordereau |
| Reporting | None — BM Pharma builds own reports from PostgreSQL data | Independent of detail_bord |
| Signature pipeline | None — CHIFA handles PKCS#11 signing | No change to delegation model |
| Integration architecture | **Provisional** — may need revision once detail_bord logic is fully understood | See BM-PHASE-009-ARCHITECTURE-RECOMMENDATION.md |
