# BM-PHASE-009-C — Memory Scan Results

## Methodology

### Tool

A C# MemoryScanner utility was compiled at runtime using PowerShell `Add-Type`. The scanner:

1. Opens the target process with `OpenProcess` (PROCESS_VM_READ | PROCESS_QUERY_INFORMATION)
2. Iterates virtual memory regions with `VirtualQueryEx`
3. Reads each committed, readable region with `ReadProcessMemory`
4. Searches the buffer for 47 predefined SQL and UI keyword patterns
5. Matches are recorded in UTF-16 and ANSI encoding
6. Output format: `[U|A] 0xADDRESS 'preview' -> full_content`

### Scan Parameters

| Parameter | Value |
|-----------|-------|
| Memory regions | MEM_COMMIT only |
| Page protection | PAGE_READWRITE |
| Encoding | UTF-16 (U) and ANSI (A) |
| Keyword patterns | SELECT, FROM, WHERE, JOIN, ORDER BY, GROUP BY, UPDATE, INSERT, DELETE, CREATE, ALTER, COPY, TRUNCATE, VACUUM, plus UI keywords (Visualiser, Bordereau, Facture, detail_bord, etc.) |
| Mode | Read-only |

### Scan A — Baseline

| Metric | Value |
|--------|-------|
| File | `raw_scan_a_baseline.txt` |
| Total regions | 1940 |
| Scanned regions | 1228 |
| Skipped (empty/guarded) | 712 |
| Total bytes scanned | ~248 MB |
| Total findings | 373,760 |

### Scan B — After FBordereau Opened

| Metric | Value |
|--------|-------|
| File | `raw_scan_b_after_open.txt` |
| Total regions | 1894 |
| Scanned regions | 1191 |
| Skipped (empty/guarded) | 703 |
| Total bytes scanned | ~248 MB |
| Total findings | 361,707 |

### Comparison Result

After deduplication of SQL-related content (SELECT/FROM/WHERE/JOIN/etc.), the comparison yielded:

**ZERO new SQL strings in Scan B that were not already present in Scan A.**

All SQL queries found after opening Visualiser Bordereau were already in the process memory at baseline.

### Interpretation

This confirms that all SQL strings are stored as constants or compiled resources within the CHIFA-OFFICINE assembly. They are loaded into memory at application startup and remain resident regardless of which form is opened. The ConfuserEx obfuscation does NOT encrypt or dynamically decrypt SQL command texts — they exist as plaintext in process memory.

### Raw Data Files

- `raw_scan_a_baseline.txt` — 66,932,438 bytes, 373,760 lines (all findings, Scan A)
- `raw_scan_b_after_open.txt` — 64,754,850 bytes, 361,707 lines (all findings, Scan B)

Both files are single-finding-per-line format: `[U|A] 0xADDRESS 'preview' -> content`
