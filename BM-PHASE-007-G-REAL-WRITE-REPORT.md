# BM-PHASE-007-G — ÉCRITURE RÉELLE EF CORE SUR CHIFA-OFFICINE

**Date:** 2026-07-27 20:14 UTC
**Résultat:** ✅ ÉCRITURE RÉELLE RÉUSSIE

---

## 1. État de la base AVANT

| Table | Lignes |
|-------|--------|
| facture | 0 |
| detail_fact | 0 |
| bordereau | 0 |
| signature | 0 |
| ln | 7 412 276 |
| medicament | 7 596 |

| Compteur | Valeur |
|----------|--------|
| next_num_fact | 1 |
| next_num_bord | 215 |
| code_centre | 11600 |
| code_ps | 1234567890 |

TST002 n'existait PAS dans la base.

---

## 2. Configuration de connexion utilisée

| Paramètre | Valeur |
|-----------|--------|
| Host | 127.0.0.1 |
| Port | 5432 |
| Database | CHIFA_OFFICINE |
| Username | pharm |
| Password | (vide — trust) |
| SslMode | Disable |
| PostgreSQL | 9.3.4 (x86, EOL 2018) |

---

## 3. Mode BM Pharma actif

| Paramètre | Valeur |
|-----------|--------|
| Mode | Test |
| ChifaWriteGuard | AUTORISE (mode ≠ ReadOnly) |

---

## 4. Validation ChifaWriteGuard

```
ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Test))
→ EnsureWriteAllowedAsync() → mode = Test → AUTORISÉ
```

---

## 5. Résultat ChifaInvoiceValidator

| Règle | Valeur | Résultat |
|-------|--------|----------|
| num_fact ≤ 8 | TST002 (6 chars) | ✅ |
| num_assure requis | TST99999 | ✅ |
| code_centre ≤ 5 | 11600 | ✅ |
| ≥ 1 ligne | 1 ligne | ✅ |
| num_enr ≤ 5 | 00010 | ✅ |
| quantité > 0, ≤ 999 | 2 | ✅ |
| PPA > 0 | 60.00 | ✅ |

**Validation: 0 erreurs**

---

## 6. Appel du service EF Core

```
ChifaPostgresInvoiceService.CreateInvoiceAsync(request)
→ ChifaWriteGuard.EnsureWriteAllowedAsync()      ✅
→ ChifaInvoiceValidator.ApplyDefaults()           ✅
→ ChifaInvoiceValidator.ApplyLineDefaults()       ✅
→ ChifaInvoiceValidator.Validate()                ✅ 0 erreurs
→ context.Database.BeginTransactionAsync(ReadCommitted)  ✅
→ context.Factures.Add(facture)                   ✅
→ context.SaveChangesAsync() [facture]            ✅
→ context.DetailFacts.Add(detail)                 ✅
→ context.SaveChangesAsync() [detail_fact]        ✅
→ transaction.CommitAsync()                       ✅
→ ChifaAuditService.LogOperationAsync()           ✅
```

**Durée totale: 877ms**

---

## 7. Résultat de la transaction

**COMMIT réussi.** Aucun rollback nécessaire.

---

## 8. Données réellement insérées

### facture

| Colonne | Valeur |
|---------|--------|
| num_fact | TST002 |
| num_assure | TST99999 |
| code_centre | 11600 |
| etat | 0 |
| mont_fact | 120.00 |
| mont_as | 84.00 |
| mont_off | 120.00 |
| date_fact | 2026-07-27 20:14:06 |
| date_soin | 2026-07-27 |
| num_bord | NULL |
| rang_ad | 1 |
| tp | 1 |
| taux | 3 |
| code_affect | 01 |
| conv | 1 |
| type_consult | 01 |
| prescripteur | BM PHARMA |
| risque | 0 |
| statut_fact | 1 |
| verifcms | 0 |
| type_signature | 0 |
| verif_fact | 0 |
| version | 2.0.0 |

### detail_fact

| Colonne | Valeur |
|---------|--------|
| num_fact | TST002 |
| num_enr | 00010 |
| ppa | 60.00 |
| qte | 2 |
| mont | 120.00 |
| mont_as | 84.00 |
| mont_pharm | 36.00 |
| num_enr_prescrit | 00010 |
| num_lot | LOT002 |
| duree_trait | 30 |
| tarif_ref | 60.00 |
| posologie | 1/day |

---

## 9. Résultats des vérifications SELECT

| Vérification | Résultat |
|-------------|----------|
| TST002 facture = 1 ligne | ✅ |
| TST002 detail_fact = 1 ligne | ✅ |
| Toutes données correctes | ✅ |
| 0 orphelins | ✅ |

---

## 10. État des compteurs AVANT / APRÈS

| Compteur | Avant | Après | Statut |
|----------|-------|-------|--------|
| next_num_fact | 1 | 1 | ✅ Identique |
| next_num_bord | 215 | 215 | ✅ Identique |

**Les compteurs n'ont PAS été modifiés.**

---

## 11. État des tables bordereau et signature

| Table | Avant | Après | Statut |
|-------|-------|-------|--------|
| bordereau | 0 | 0 | ✅ |
| signature | 0 | 0 | ✅ |

---

## 12. Résultat de l'audit

```
[CHIFA-AUDIT] Op=CREATE_INVOICE Entity=facture Key=TST002
Details=Lines: 1, Total: 120.00, CorrelationId: 3c97fd4c
Success=True Duration=877ms Error=none User=system
```

---

## 13. Confirmation qu'aucune autre donnée n'a été modifiée

| Table | Avant | Après |
|-------|-------|-------|
| ln | 7 412 276 | 7 412 276 |
| medicament | 7 596 | 7 596 |

**Aucune autre donnée n'a été créée ou modifiée.**

---

## 14. État final de la base

| Table | Lignes |
|-------|--------|
| facture | 1 (TST002) |
| detail_fact | 1 (TST002-00010) |
| bordereau | 0 |
| signature | 0 |
| ln | 7 412 276 |
| medicament | 7 596 |

---

## 15. Corrections appliquées pendant le test

Trois bugs ont été découverts et corrigés pendant Phase 007-G :

1. **DateTime Kind mismatch** — `DateTime.Today` et `DateTime.Now` produisent des DateTimes de kind `Local`, mais PostgreSQL exige `Unspecified` pour `timestamp without time zone`. Corrigé avec `DateTime.SpecifyKind(..., DateTimeKind.Unspecified)`.

2. **FK ordering** — EF Core insérait `detail_fact` avant `facture` car la FK n'était pas configurée. Corrigé avec deux appels `SaveChangesAsync()` séparés (facture d'abord, puis details).

3. **Column type mismatches** — Les colonnes `signature` et `fact_xml` sont de type `xml` en PostgreSQL mais mappées comme `varchar` par EF Core. La colonne `date_fact` est `timestamp without time zone` et `date_soin` est `date`. Corrigé avec `HasColumnType()` dans `ChifaFactureConfiguration`.

**Fichiers modifiés:**
- `src/BMPharma.CHIFA/Services/ChifaPostgresInvoiceService.cs` — DateTime fixes + split SaveChanges
- `src/BMPharma.Persistence.PostgreSQL/Configurations/ChifaFactureConfiguration.cs` — Column type fixes

---

## 16. Tests existants

```
509/509 tests — 0 erreurs — 0 warnings
```

- Domain: 6/6
- Application: 3/3
- Architecture: 7/7
- CHIFA: 493/493

---

## CONCLUSION

**ÉCRITURE RÉELLE RÉUSSIE**

La facture TST002 a été créée via EF Core sur la base PostgreSQL CHIFA_OFFICINE en utilisant le service applicatif `ChifaPostgresInvoiceService.CreateInvoiceAsync()`.

Le chemin complet fonctionne :
`BM Pharma → EF Core → Npgsql → PostgreSQL 9.3.4 → CHIFA_OFFICINE`

---

**⛔ STOP — EN ATTENTE D'APPROBATION POUR LE ROLLBACK**
