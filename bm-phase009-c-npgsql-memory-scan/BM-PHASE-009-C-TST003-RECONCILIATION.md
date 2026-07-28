# BM-PHASE-009-C — TST003 Reconciliation

## Background

During the experimental phase (TST003), a test facture was inserted into the PostgreSQL database. The facture was:
- Present in the `facture` table
- Correctly linked to a bordereau (`num_bord`)
- Retrievable by direct SQL queries matching the identified pattern

However, TST003 was **not visible** in the `Visualiser Bordereau` form (`FBordereau`), although it was visible in `Consultation Facture`.

## Identified Query

The SQL query confirmed to populate the `detail_bord` DataTable is:

```sql
SELECT * FROM facture
WHERE num_bord = '.'
  AND CODE_CENTRE = '.'
  AND etat = 'S'
ORDER BY TP, NUM_ASSURE, NUM_FACT
```

## Observed Facts

| Fact | Source |
|------|--------|
| The query uses `num_bord`, `CODE_CENTRE`, and `etat = 'S'` as filters | Memory scan (CONFIRMED) |
| TST003 was present in PostgreSQL and correctly linked | TST003 test result |
| TST003 was not visible in Visualiser Bordereau | TST003 test result |
| TST003 was visible in Consultation Facture | TST003 test result |
| Zero new SQL strings appeared after opening FBordereau | Scan A vs Scan B comparison |

## Hypotheses

The following are plausible explanations for why TST003 was not visible. **None are confirmed as facts.**

### Hypothesis 1: Incompatible `etat`

The query filters on `etat = 'S'` (signé). If TST003 had an `etat` value other than `'S'` at the time the form was opened, it would not appear in the DataTable.

- Plausible if TST003 was in `'I'` (inserted) state
- Would explain why Consultation Facture (which uses a different query not filtering on `etat`) could still display it

### Hypothesis 2: Mismatched `CODE_CENTRE`

The query filters on `CODE_CENTRE = '.'`. The value is dynamically injected at runtime based on the logged-in pharmacist's centre. If TST003's `CODE_CENTRE` differed from the centre code used by the form, it would be excluded.

- Plausible if the test facture was associated with a different centre than the one selected in the form
- Would explain selective visibility

### Hypothesis 3: Mismatched `num_bord`

The query filters on the bordereau number entered or selected in the form. If TST003 was linked to a different `num_bord` than the one selected in the form's `tb_suivi_numbord`, it would not appear.

- The form requires the user to select a bordereau from the list or enter a number
- If no bordereau was selected, no query would execute, and the DataTable would remain empty
- The observed `NullReferenceException` in `button3_Click` is consistent with an empty or uninitialized DataTable

### Hypothesis 4: Logique complémentaire après le SELECT

The code may apply additional filtering or transformation to the DataTable after the SQL query executes, such as:

- Removing rows based on business rules not reflected in the SQL WHERE clause
- Applying additional sorting or grouping that affects row visibility
- Checking for related data that may be missing

### Hypothesis 5: Problème de contexte du formulaire ou du cycle de chargement

The form may have a multi-step loading process:

1. First load the bordereau list
2. Then (on bordereau selection) load the facture list
3. The NullReferenceException may have occurred before step 2 completed

If the bordereau list query also returned no results (because the bordereau for TST003 didn't match the list query criteria), then no bordereau would be selected and the detail query would never execute.

### Hypothesis 6: Bordereau statut incompatible

The bordereau list query also has its own filters. If TST003's bordereau was in an `etat` that the list query excluded (e.g., not `'O'` or `'C'`), the bordereau would not appear in the list, preventing the user from selecting it and triggering the detail query.

## Conclusion

The fact that TST003 was not visible in Visualiser Bordereau while the reconstructed SQL query shows a straightforward `SELECT ... WHERE num_bord, CODE_CENTRE, etat = 'S'` pattern is **not a contradiction**.

The identified query is confirmed as the SQL statement used to populate `detail_bord`. The invisibility of TST003 is most likely caused by one or more of the runtime conditions described above — particularly the `etat` value, the `CODE_CENTRE` value, or the `num_bord` selection/availability at the moment the form was used.

**The exact cause has NOT been determined.** No hypothesis listed above should be treated as confirmed without further investigation (which is outside the scope of this phase).

## What Is Confirmed

- The SQL query used to populate `detail_bord` is identified
- The filters are: `num_bord`, `CODE_CENTRE`, and `etat = 'S'`
- Any facture that does not satisfy ALL three runtime conditions will not appear in the DataTable

## What Is Not Determined

- Which specific condition caused TST003 to be excluded
- Whether additional code-level filtering occurs after the query
- The exact runtime values of `num_bord` and `CODE_CENTRE` at the time of the test
