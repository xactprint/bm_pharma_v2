# BM-PHASE-009 — SQL Capture Report

**Runtime SQL Monitoring for Visualiser Bordereau / detail_bord**

> **Date:** 2026-07-28
> **Status:** COMPLETE — SQL NOT CAPTURED
> **Classification:** Confidential — BM Pharma Internal

---

## 1. Methods Used

| # | Method | Status | Result |
|---|--------|--------|--------|
| 1 | `pg_stat_activity` polling (5 sessions, 2–5 min, 50–500ms) | COMPLETE | No SQL beyond `unlisten *` |
| 2 | PostgreSQL native logging (`logging_collector=off`, `log_min_duration_statement=0`) | COMPLETE | Startup queries captured; server crashed 2× (0xC0000142) |
| 3 | PostgreSQL native logging restored (original config) | COMPLETE | Server stable; logging inoperable (log collector crash) |
| 4 | Npgsql direct polling via PowerShell | PARTIAL | DataReader error in tight loop; no SQL captured |
| 5 | CGAPXUDN.dll / cgapxutl.dll IL analysis | COMPLETE | No SQL — CNAS API only |
| 6 | NLog analysis | COMPLETE | ERROR-level only; no SQL |

---

## 2. Configuration Changes

### Config Modified (temporary, since restored)

```ini
logging_collector = off              # was: on
log_min_duration_statement = 0       # was: -1 (disabled)
log_line_prefix = '%t [%p] %u@%d '  # was: '%t '
log_connections = on                 # was: off (commented)
log_disconnections = on              # was: off (commented)
```

**Original config restored** from `postgresql.conf.bak` on 2026-07-28 19:42.

### Crash Details

- **Exception:** `0xC0000142` (STATUS_DLL_INIT_FAILED)
- **Crash 1:** 19:39:36 (server process PID 20888, then launch process PID 16092)
- **Crash 2:** 19:42:01 (server process PID 1412, then launch process PID 12032)
- **Root cause:** Probably `logging_collector=off` incompatibility with this PostgreSQL 9.3.4 embedded instance on Windows

---

## 3. SQL Captured (Startup Queries Only)

These were captured during the brief window when `logging_collector=off` was active and the server was responsive:

### Query 1: Bordereau List (facture table) — 42ms

```sql
SELECT b.num_bord, b.code_centre, b.etat,
       COUNT(f.num_fact) AS nb,
       SUM(CASE WHEN (signature IS NOT NULL) AND (LENGTH(signature::xml::text)>0) THEN 1 ELSE 0 END) AS nb_sign,
       SUM(f.mont_fact) mont_bord,
       SUM(f.mont_off) mont_off,
       SUM(f.mont_as) mont_as,
       SUM(f.mont_maj_fae) mont_maj_fae,
       SUM(f.mont_maj) mont_maj,
       b.duplicata,
       SUBSTR(b.num_bord,5,2)||SUBSTR(b.num_bord,1,4) AS num_ordre
FROM bordereau b, facture f
WHERE b.num_bord = f.num_bord
  AND f.etat = 'S'
GROUP BY b.num_bord, b.code_centre, b.etat, b.duplicata
ORDER BY num_ordre DESC, b.code_centre
```

### Query 2: Bordereau List (facture2 table) — 1ms

```sql
SELECT b.num_bord, b.code_centre, b.etat,
       COUNT(f.num_fact) AS nb,
       SUM(CASE WHEN (signature IS NOT NULL) AND (LENGTH(signature::xml::text)>0) THEN 1 ELSE 0 END) AS nb_sign,
       SUM(CASE WHEN f.mont_fact ~ '^\d+(\.\d+)?$' THEN f.mont_fact::numeric ELSE 0 END) mont_bord,
       SUM(CASE WHEN f.mont_off ~ '^\d+(\.\d+)?$' THEN f.mont_off::numeric ELSE 0 END) mont_off,
       SUM(CASE WHEN f.mont_as ~ '^\d+(\.\d+)?$' THEN f.mont_as::numeric ELSE 0 END) mont_as,
       SUM(CASE WHEN f.mont_maj_fae ~ '^\d+(\.\d+)?$' THEN f.mont_maj_fae::numeric ELSE 0 END) mont_maj_fae,
       SUM(CASE WHEN f.mont_maj ~ '^\d+(\.\d+)?$' THEN f.mont_maj::numeric ELSE 0 END) mont_maj,
       b.duplicata
FROM bordereau b, facture2 f
WHERE b.num_bord = f.num_bord
  AND f.etat = 'S'
GROUP BY b.num_bord, b.code_centre, b.etat, b.duplicata
ORDER BY b.num_bord, b.code_centre
```

### Query 3: Centre List — 3ms

```sql
SELECT * FROM centre ORDER BY code_centre, nom
```

### Query 4: Facture List (facture table) — 4ms

```sql
SELECT f.*, b.*, t.*,
       c.nom AS nom_as, c.prenom AS prenom_as
FROM facture f
LEFT OUTER JOIN beneficiaire b ON f.num_assure = b.num_assure AND f.rang_ad = b.rang_ad
LEFT OUTER JOIN beneficiaire c ON f.num_assure = c.num_assure AND c.rang_ad = '00'
LEFT OUTER JOIN bordereau t ON f.num_bord = t.num_bord
ORDER BY f.date_fact DESC, f.num_fact ASC
LIMIT 1000
```

### Query 5: Facture List (facture2 table) — 1ms

Same pattern as Query 4 for `facture2`.

### SET commands

```sql
SET ssl_renegotiation_limit=0
SET extra_float_digits=2;SET extra_float_digits=3;
```

---

## 4. Queries NOT Captured

The following were targeted but NOT captured:

| Query | Target | Status |
|-------|--------|--------|
| `detail_bord` population SQL | Visualiser Bordereau | NOT CAPTURED |
| Any bordereau detail query | Visualiser Bordereau | NOT CAPTURED |
| Any query for specific `num_bord` | Visualiser Bordereau | NOT CAPTURED |

### Why Not Captured

The most likely explanation is that **Npgsql connection-pool cleanup** sends `unlisten *` immediately after each query (0-1ms), overwriting the actual query in `pg_stat_activity.query` before polling can capture it. The sequence is:

1. User opens Visualiser Bordereau
2. CHIFA executes `detail_bord` SQL (< 50ms)
3. Npgsql returns connection to pool
4. Npgsql sends `unlisten *` (< 1ms)
5. pg_stat_activity shows `unlisten *`

Alternative explanations:
- The user may not have opened Visualiser Bordereau during monitoring windows
- The data may be filtered client-side from already-loaded DataSets (no SQL needed)

---

## 5. CGAPXUDN/cgapxutl Analysis

Both DLLs were fully decompiled and analyzed for SQL content.

**Result: NO SQL queries found.**

These are C++/CLI mixed-mode assemblies for CNAS electronic invoice submission:
- `CGAPXUDN.dll` — CNAS API client (signing, validation, `APICNAS.Facture` class)
- `cgapxutl.dll` — CNAS XML processing (beneficiary data, acte management, `SignerEtHistoriserFacture`)

Neither interacts with the local CHIFA PostgreSQL database.

---

## 6. Comparison with Phase 009 Reconstructed SQL

| Aspect | Phase 009 Reconstruction | Reality (from captured startup queries) |
|--------|-------------------------|----------------------------------------|
| JOIN between `bordereau` and `facture` | CONFIRMED | `bordereau b, facture f WHERE b.num_bord = f.num_bord` |
| `facture2` table | NOT CONSIDERED | EXISTS — duplicate schema with numeric-safe casting |
| `LIMIT 1000` on facture list | NOT PRESENT | EXISTS in facture list queries |
| `signature::xml::text` | NOT IN RECONSTRUCTION | EXISTS — signature XML length check |
| Safe numeric casting (`~ '^\d+(\.\d+)?$'`) | NOT PRESENT | EXISTS for `facture2` queries |
| `SUBSTR(b.num_bord,5,2)||SUBSTR(b.num_bord,1,4)` AS `num_ordre` | NOT PRESENT | EXISTS — reordering key |
| `LEFT JOIN beneficiaire` on `num_assure` + `rang_ad` | CONFIRMED | EXISTS |
| `c.rang_ad = '00'` for `nom_as` | NOT PRESENT | EXISTS — separate join for insured name |
| `ORDER BY ... LIMIT 1000` | NOT IN RECONSTRUCTION | EXISTS |

The `detail_bord` SQL (Phase 009 reconstruction) remains **unverified** and is at best a **hypothesis** based on RDLC report bindings and schema analysis.

---

## 7. Confidence Level

| Aspect | Confidence |
|--------|-----------|
| Startup queries (bordereau list, facture list, centre) | **CONFIRMED** — captured from PostgreSQL log |
| `detail_bord` population SQL | **NOT DETERMINED** — not captured |
| Reconstructed SQL (Phase 009) | **LOW-MEDIUM** — hypothesis only |
| CGAPXUDN/cgapxutl contains no SQL | **CONFIRMED** |
| Npgsql `unlisten *` overwrites query | **LIKELY HYPOTHESIS** |

---

## 8. Limitations

1. **PostgreSQL 9.3.4** on Windows has a defective `logging_collector` (`--forkcol` crash)
2. **ConfuserEx v1.0.0** prevents direct EXE decompilation
3. **pg_stat_activity polling** insufficient for sub-50ms queries overwritten by `unlisten *`
4. **No external tools** (API Monitor, Wireshark, ProcMon) were available/installed
5. **Config modification** caused PostgreSQL crashes (0xC0000142) — had to restore original config
6. **CGAPXUDN/cgapxutl** are CNAS API DLLs, not CHIFA SQL sources

---

## 9. Next Options

If exact `detail_bord` SQL is still required:

1. **API Monitor** (rohitab.com) — intercept Npgsql `ExecuteReader` / `CommandText` calls
2. **Wireshark/tshark portable** — capture PostgreSQL protocol on loopback :5432
3. **.NET runtime memory dump** — extract decrypted assemblies from running CHIFA process
4. **de4dot / dnSpy** — deobfuscate ConfuserEx-protected executable

---

## 10. Conclusion

> Avons-nous maintenant le SQL exact qui alimente `detail_bord` ?

**NON — impossible à capturer avec les méthodes actuellement disponibles.**

PostgreSQL monitoring ne peut pas capturer les requêtes suffisamment rapidement avant que Npgsql ne les écrase avec `unlisten *`. Les DLLs CGAPXUDN/cgapxutl ne contiennent pas de SQL. Le SQL exact reste dans le binaire ConfuserEx-protégé.

Le SQL reconstruit dans BM-PHASE-009-SQL-ANALYSIS.md reste notre meilleure approximation (confiance **LOW-MEDIUM**), basée sur l'analyse de schéma et les bindings RDLC, non sur une capture réelle.

**SQL exact du startup OK** (bordereau list, facture list, centre) — 5 requêtes confirmées ci-dessus.
**SQL exact de detail_bord** — toujours non déterminé.
