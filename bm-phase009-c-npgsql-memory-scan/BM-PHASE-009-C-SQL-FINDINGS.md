# BM-PHASE-009-C — SQL Findings

## Reconstructed SQL Queries

All queries were reconstructed from plaintext strings found in the process memory of CHIFA-OFFICINE (PID 16376). The character `'.'` represents a placeholder whose value is injected at runtime.

---

### A. Bordereau Listing

**Purpose**: Populate the bordereau list (used in `FSuiviBordereau`/`FBordereau`).

**Status**: CONFIRMED

```sql
SELECT b.num_bord, b.code_centre, b.etat, b.duplicata,
       substr(b.num_bord,5,2) || substr(b.num_bord,1,4) AS num_ordre,
       count(f.num_fact) AS nb,
       sum(CASE WHEN (signature IS NOT NULL)
                AND (length(signature::xml::text) > 0)
           THEN 1 ELSE 0 END) AS nb_sign,
       sum(CASE WHEN f.mont_fact ~ '^\d+(\.\d+)?$'
           THEN f.mont_fact::numeric ELSE 0 END) AS mont_bord
FROM bordereau b, facture f
WHERE b.num_bord = f.num_bord
  AND f.etat = 'S'
GROUP BY b.num_bord, b.code_centre, b.etat, b.duplicata
ORDER BY num_ordre DESC, b.code_centre
```

**Variants**:

| Variant | Condition |
|---------|-----------|
| All bordereaux | `f.etat = 'S'` |
| Open bordereaux | `f.etat = 'S' AND b.etat = 'O'` |
| Closed bordereaux | `f.etat = 'S' AND b.etat = 'C'` |
| Using facture2 | Same structure but from `bordereau b, facture2 f` |

---

### B. detail_bord DataTable Population

**Purpose**: Load factures for a selected bordereau into the `detail_bord` DataTable.

**Status**: CONFIRMED

```sql
SELECT * FROM facture
WHERE num_bord = '.'
  AND CODE_CENTRE = '.'
  AND etat = 'S'
ORDER BY TP, NUM_ASSURE, NUM_FACT
```

**Runtime parameters**:

| Parameter | Source |
|-----------|--------|
| `num_bord` | Input from form (e.g., `tb_suivi_numbord`) |
| `CODE_CENTRE` | Pharmacist's centre code (from configuration or login) |
| `etat` | Hard-coded to `'S'` (signé) |

**Variant using facture2**:

```sql
SELECT * FROM facture2
WHERE num_bord = '.'
  AND etat = 'S'
ORDER BY TP, NUM_ASSURE, NUM_FACT
```

---

### C. Detail Medicaments

**Purpose**: Load medication line items for a selected facture.

**Status**: CONFIRMED

```sql
SELECT num_enr, m.nom_com, m.dosage, qte, ppa, duree_trait,
       mont, mont_as, mont_pharm
FROM detail_fact f
LEFT OUTER JOIN medicament m ON f.num_enr = m.num_enr
WHERE num_fact = '.'
ORDER BY f.num_enr
```

**Variant using detail_fact2**:

```sql
SELECT num_enr, m.nom_com, m.dosage, qte, ppa, duree_trait,
       mont, mont_as, mont_pharm
FROM detail_fact2 f
LEFT OUTER JOIN medicament m ON f.num_enr = m.num_enr
WHERE num_fact = '.'
ORDER BY f.num_enr
```

---

### D. Consultative Facture View

**Purpose**: Display facture with beneficiary names, insured person name, and bordereau info.

**Status**: CONFIRMED

```sql
SELECT * FROM facture f
LEFT OUTER JOIN beneficiaire b
    ON f.num_assure = b.num_assure
    AND f.rang_ad = b.rang_ad
LEFT OUTER JOIN beneficiaire c
    ON f.num_assure = c.num_assure
    AND c.rang_ad = '00'
LEFT OUTER JOIN bordereau t
    ON f.num_bord = t.num_bord
ORDER BY f.date_fact DESC, f.num_fact ASC
LIMIT 1000
```

**JOIN explanation**:

| Alias | Table | Role |
|-------|-------|------|
| `f` | `facture` | The facture record |
| `b` | `beneficiaire` | Patient's name (using the facture's `rang_ad`) |
| `c` | `beneficiaire` | Primary insured person's name (`rang_ad = '00'`) |
| `t` | `bordereau` | Bordereau metadata |

**Variant**: Uses `facture2` instead of `facture`.

---

### E. Additional Queries Observed

| Query | Purpose |
|-------|---------|
| `SELECT * FROM centre ORDER BY code_centre, nom` | Centre list population |
| `SELECT code_centre FROM bordereau WHERE etat='C' ORDER BY num_bord` | Closed bordereau centres |
| `SELECT num_assure FROM attestation_mc WHERE num_assure='.'` | Mutualist card validation |
| `SELECT count(*) FROM ln` | Blacklist count check |
| `SELECT sign FROM signature WHERE num_fact='.'` | Signature retrieval |

### Note on Placeholders

The `'.'` character in all reconstructed queries represents a placeholder whose actual value is substituted at runtime by the application code. The exact substitution mechanism (e.g., `NpgsqlCommand.Parameters.AddWithValue`, string concatenation, etc.) has not been determined and is out of scope for this investigation.
