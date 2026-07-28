# BM-PHASE-009-C — detail_bord DataTable Analysis

## Nature

`detail_bord` is a **.NET DataTable** created in memory by CHIFA-OFFICINE.

It is **NOT** a PostgreSQL table.

It is used by:
- `FBordereau` (Visualiser Bordereau form)
- Report RDLC `Report_Bord_1.rdlc`
- Report RDLC `Report_Bord_2.rdlc`

## Column Structure

Confirmed by DBNull error messages found in process memory and DataSet schema strings:

| Column | Likely Type | Source Table |
|--------|-------------|--------------|
| `num_fact` | string | facture |
| `date_fact` | date | facture |
| `type_maj` | int/string | facture |
| `mont_as` | decimal | facture |
| `mont_ps` | decimal | facture |
| `mont_maj` | decimal | facture |
| `mont_total_ps` | decimal | facture |
| `num_page` | int | facture |
| `mont_mut` | decimal | facture |
| `remarque` | string | facture |
| `bord_clos` | bool | Computed/internal |
| `qr_code` | string | Computed/internal |

### Additional DataSet Columns (from schema strings)

Related DataTables in the same DataSet may also include:

```
nom_com, qte, ppa, montant, montant_as, montant_ps, montant_pharm,
montant_maj, type_maj, code_centre_as, code_centre_ps
```

These correspond to `detail_fact` and `beneficiaire` data, stored in separate DataTables within the same typed DataSet (`DataSet1`).

## Population Query

The DataTable is populated by the following SQL query (**CONFIRMED**):

```sql
SELECT * FROM facture
WHERE num_bord = '.'
  AND CODE_CENTRE = '.'
  AND etat = 'S'
ORDER BY TP, NUM_ASSURE, NUM_FACT
```

### How the DataTable is used by the Form

Based on UI control names found in memory, the form flow is:

1. **Left panel — Bordereau list** (`dgv_suivi_liste_bord`):
   - Populated by the borderau listing query (Section A of SQL Findings)
   - Shows open/closed/all bordereaux

2. **Right panel — Facture list** (`dgv_factures` inside `groupBox7` "Liste des Factures du Bordereau N°"):
   - Data source: `detail_bord` DataTable
   - Filtered when a bordereau is selected in the left panel
   - Columns shown likely include: num_fact, date_fact, mont_as, mont_ps, etc.

3. **Detail medicaments** (`dgv_detail_medic` inside `groupBox9` "Médicaments de la Facture N°"):
   - Populated by the detail medicaments query when a facture is selected

## Relationship to Visualiser Bordereau (FBordereau)

The `FBordereau` form:
- Has a top section with `tb_suivi_numbord` (bordereau number input) and `tb_suivi_centre` (centre code)
- Has a "Rechercher Détail" button that triggers `button3_Click`
- `button3_Click` populates `detail_bord` by executing the facture-by-bordereau query
- If the query returns no rows (empty DataTable), the form throws a `NullReferenceException` when trying to access DataTable cells
- The `NullReferenceException` observed during testing is consistent with an empty database scenario

## Relationship to Reports

The RDLC reports `Report_Bord_1.rdlc` and `Report_Bord_2.rdlc` use `detail_bord` as their data source. The columns match the report layout for printing bordereau details.

## Summary

| Aspect | Conclusion |
|--------|------------|
| Is detail_bord a PostgreSQL table? | NO |
| Is detail_bord a .NET DataTable? | YES |
| Is the population query identified? | YES, CONFIRMED |
| Are the columns confirmed? | YES (via DBNull errors and schema) |
| Does the DataTable hold facture-level data? | YES (one row per facture) |
| Does it hold detail medicament data? | NO (separate DataTable for that) |
