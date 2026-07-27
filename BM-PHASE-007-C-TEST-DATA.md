# BM-PHASE-007-C — PRÉPARATION FACTURE TEST TST002

**Date:** 2026-07-27

---

## Données préparées

### ChifaInvoiceRequest

| Champ | Valeur | Justification |
|-------|--------|---------------|
| NumFact | TST002 | 6 chars ≤ 8 max, non existant |
| NumAssure | TST99999 | 7 chars ≤ 12 max, synthétique |
| CodeCentre | 11600 | Réel, existant dans parametre |
| DateSoin | 2026-07-27 | Aujourd'hui |

### ChifaInvoiceLineRequest

| Champ | Valeur | Justification |
|-------|--------|---------------|
| NumEnr | 00010 | Médicament ZYRTEC, confirmé existant |
| MedicCode | 10 | Code médicament |
| PrixUnit | 60.00 | tarif_ref de ZYRTEC |
| Quantite | 2 | Test simple |
| NumLot | LOT002 | Lot test |
| Posologie | 1/day | Posologie test |
| InfTr | 1 | Défaut |
| ApplicTr | 1 | Défaut |
| Medic | 1 | Défaut |
| Ts | 4 | Défaut |
| DureeTrait | 30 | 30 jours |

### Champs calculés par le service

| Champ | Calcul | Valeur |
|-------|--------|--------|
| montFact | 2 × 60.00 | 120.00 |
| montAs | 120.00 × 0.70 | 84.00 |
| mont_pharm | 120.00 × 0.30 | 36.00 |
| mont_detail | 2 × 60.00 | 120.00 |
| mont_as_detail | 120.00 × 0.70 | 84.00 |

### Champs fixes par le service

| Champ | Valeur |
|-------|--------|
| etat | 0 (brouillon) |
| num_bord | NULL (pas de bordereau) |
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
| mont_maj_fae | 0 |
| mont_maj | 0 |
| type_maj | 0 |
| version | 2.0.0 |
| signature | NULL |
| fact_xml | NULL |
| echifa | false |
| e_ord | false |

### detail_fact champs fixes

| Champ | Valeur |
|-------|--------|
| num_enr_prescrit | 00010 |
| maj_local | 0 |
| maj_sub | 0 |
| remboursable | true |
| local | false |
| inf_tr | true |
| applic_tr | true |
| medic | true |
| ts | false |

## Médicament utilisé

| Champ | Valeur |
|-------|--------|
| num_enr | 00010 |
| nom_com | ZYRTEC |
| tarif_ref | 60.00 |
| convention | O |
| remboursable | O |

**Vérifié par SELECT : médicament 00010 existe dans la table medicament.**

## Contraintes vérifiées

- num_fact : 6 chars ≤ 8 max ✅
- num_assure : 7 chars ≤ 12 max ✅
- code_centre : 5 chars ≤ 5 max ✅
- num_enr : 5 chars ≤ 5 max ✅
- quantité : 2 (1-999) ✅
- PPA : 60.00 (> 0) ✅
- ≥ 1 ligne ✅
