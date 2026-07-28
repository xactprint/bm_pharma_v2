# BM-PHASE-009-C — Executive Report

## Npgsql Memory Scan Investigation

### Objective

Identify the exact SQL queries used by `CHIFA-OFFICINE` to populate the in-memory `detail_bord` DataTable used by the `Visualiser Bordereau` form (`FBordereau`), without modifying the protected EXE or the PostgreSQL database.

### Approach

Because the main EXE (`CHIFA_OFFICINE.exe`, assembly name `CHIFA_OFFICINE_INSECURE`) is protected with ConfuserEx v1.0.0 — preventing static decompilation — a dynamic memory-scanning approach was used:

1. A C# MemoryScanner (compiled via `Add-Type`) read the process memory of the running CHIFA instance (PID 16376, 32-bit WOW64).
2. Two scans were performed: Scan A (baseline at process idle) and Scan B (after opening `FBordereau` form and clicking "Rechercher Détail").
3. The scanner used `OpenProcess`, `VirtualQueryEx`, `ReadProcessMemory`, and `CloseHandle` in read-only mode.
4. 47 SQL and UI keyword patterns were searched in both UTF-16 and ANSI encoding, yielding ~374,000 findings per scan.
5. The results were deduplicated and compared to isolate new SQL strings.

### Key Findings

| Finding | Status |
|---------|--------|
| All SQL queries were in memory at startup — none appeared after opening FBordereau | CONFIMED |
| The `detail_bord` DataTable is populated by `SELECT * FROM facture WHERE num_bord = '.' AND CODE_CENTRE = '.' AND etat = 'S' ORDER BY TP, NUM_ASSURE, NUM_FACT` | CONFIRMED |
| Bordereau listing query (4 variants: all, open, closed, using facture2) | CONFIRMED |
| Detail medicaments query (`detail_fact` + `medicament` JOIN) | CONFIRMED |
| Consultative facture query (with dual `beneficiaire` JOIN and `bordereau` JOIN) | CONFIRMED |
| Npgsql 2.0.14.3 does NOT log `CommandText` values | CONFIRMED |
| `detail_bord` is a .NET DataTable, not a PostgreSQL table | CONFIRMED |
| Exact runtime clause causing TST003 invisibility | NOT DETERMINED |

### Confidence Summary

All reconstructed SQL queries are **CONFIRMED** via two memory scans showing identical results. The query structures were verified by cross-referencing with UI control names, DataTable column lists, DBNull error messages, and DataSet schema strings found in the same memory regions.

### Files Produced

| # | File | Description |
|---|------|-------------|
| 1 | BM-PHASE-009-C-EXECUTIVE-REPORT.md | This document |
| 2 | BM-PHASE-009-C-PROCESS-IDENTIFICATION.md | Process details |
| 3 | BM-PHASE-009-C-MEMORY-SCAN-RESULTS.md | Memory scan methodology and results |
| 4 | BM-PHASE-009-C-SQL-FINDINGS.md | All reconstructed SQL queries |
| 5 | BM-PHASE-009-C-DETAIL-BORD-ANALYSIS.md | detail_bord DataTable analysis |
| 6 | BM-PHASE-009-C-CONFIDENCE-MATRIX.md | Confidence matrix |
| 7 | BM-PHASE-009-C-SECURITY-EVIDENCE.md | Security implications |
| 8 | BM-PHASE-009-C-RAW-SCAN-LOG.txt | Raw scan notes |
| 9 | BM-PHASE-009-C-TST003-RECONCILIATION.md | TST003 case reconciliation |

### Out of Scope

- Signature operations
- Bordereau closure
- CNAS transmission
- Token/certificate usage
- Any CHIFA or BM Pharma code modifications
