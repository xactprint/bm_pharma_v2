# BM-PHASE-009-C — Confidence Matrix

## Classification Levels

| Level | Meaning |
|-------|---------|
| CONFIRMED | Evidence from two independent scans, cross-referenced |
| PARTIALLY CONFIRMED | Evidence supports but gaps remain |
| NOT DETERMINED | Insufficient evidence to confirm or deny |
| FALSE | Hypothesis disproven by evidence |
| OUT OF SCOPE | Deliberately excluded from this investigation |

## Matrix

| Element | Level | Evidence |
|---------|-------|----------|
| Process CHIFA (PID 16376) identified | **CONFIRMED** | Task Manager, WMI query, consistent across sessions |
| Process is 32-bit WOW64 | **CONFIRMED** | WMI `OSArchitecture` + `ProcessBitDepth` |
| Assembly name `CHIFA_OFFICINE_INSECURE` | **CONFIRMED** | Exception dump text in process memory |
| ConfuserEx v1.0.0 protects main EXE | **CONFIRMED** | CLR header, ConfuserEx resources in memory |
| Npgsql.dll version 2.0.14.3 | **CONFIRMED** | Assembly metadata, IL disassembly |
| NpgsqlEventLog does not log CommandText | **CONFIRMED** | IL code review of NpgsqlEventLog class |
| PostgreSQL native logging not usable | **CONFIRMED** | Server crash (0xC0000142) on config change |
| MemoryScanner can read process memory | **CONFIRMED** | ~374K findings per scan, read-only success |
| Scan A vs Scan B: zero new SQL strings | **CONFIRMED** | Python comparison of deduplicated content |
| SQL: Bordereau listing query | **CONFIRMED** | Present in both scans, all 4 variants found |
| SQL: detail_bord population query | **CONFIRMED** | `SELECT * FROM facture WHERE num_bord='.' AND CODE_CENTRE='.' AND etat='S' ORDER BY TP,NUM_ASSURE,NUM_FACT` |
| SQL: Detail medicaments query | **CONFIRMED** | `detail_fact LEFT OUTER JOIN medicament` pattern |
| SQL: Consultative facture query | **CONFIRMED** | Dual beneficiaire + bordereau JOIN pattern |
| detail_bord is a .NET DataTable | **CONFIRMED** | DBNull error messages, DataSet schema strings |
| detail_bord is a PostgreSQL table | **FALSE** | Directly disproven by DBNull error format |
| All SQL strings are compiled constants | **PARTIALLY CONFIRMED** | Present at startup; dynamic construction not ruled out |
| Exact runtime parameters causing TST003 invisibility | **NOT DETERMINED** | Multiple plausible hypotheses, none confirmed |
| Signature mechanism | **OUT OF SCOPE** | Not part of this phase |
| Bordereau closure mechanism | **OUT OF SCOPE** | Not part of this phase |
| CNAS transmission | **OUT OF SCOPE** | Not part of this phase |
| Token/certificate handling | **OUT OF SCOPE** | Not part of this phase |

## Summary

14 elements confirmed, 1 partially confirmed, 1 not determined, 1 disproven, 4 out of scope.

The core objective — identifying the SQL queries used by Visualiser Bordereau — has been achieved with high confidence.
