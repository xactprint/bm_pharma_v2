# BM-PHASE-008-A — CYCLE DE VIE D'UNE FACTURE CHIFA

**Document:** 008-A-INVOICE-LIFECYCLE
**Date:** 2026-07-28

---

## Schéma simplifié

```mermaid
flowchart TD
    VENTE[Vente BM Pharma] --> FACTURE_INIT[Création facture<br/>état='0', num_bord=NULL]
    
    FACTURE_INIT --> ASSIGN_BORD[Association bordereau<br/>UPDATE facture SET num_bord = X]
    
    ASSIGN_BORD --> VISUALISER[Visualiser Facture<br/>FConsultation_Facture]
    ASSIGN_BORD --> VISU_BORD[Visualiser Bordereau<br/>FBordereau]
    
    VISUALISER --> SIGNER[Signature token<br/>Identiv uTrust 3512]
    
    SIGNER --> CLOTURER[Clôture bordereau<br/>cloturerbord()]
    
    CLOTURER --> TRANSMETTRE[Transmission CNAS<br/>FTP 41.111.149.250]
```

---

## Structure de la table `facture`

| Grille | Colonnes |
|--------|----------|
| **Identifiants** | `num_fact` (PK, VARCHAR(8)), `num_assure` (VARCHAR(12)) |
| **Médical** | `code_centre` (VARCHAR(5)), `tp` (CHAR(1)), `taux` (CHAR(1)), `conv` (CHAR(1)), `type_consult` (VARCHAR(2)), `prescripteur` (VARCHAR(50)) |
| **Dates** | `date_fact` (TIMESTAMP), `date_soin` (DATE) |
| **Montants** | `mont_off` (NUMERIC(10,2)), `mont_as` (NUMERIC(10,2)), `mont_fact` (NUMERIC(11,2)), `mont_maj` (NUMERIC(11,2)), `mont_maj_fae` (NUMERIC(4,2)), `mont_mut` (NUMERIC(10,2)) |
| **Statut** | `etat` (CHAR(1)), `risque` (CHAR(1)), `statut_fact` (CHAR(1)) |
| **Bordereau** | `num_bord` (VARCHAR(6), FK → bordereau) |
| **Signature** | `signature` (XML), `fact_xml` (XML), `type_signature` (CHAR(1)) |
| **Mutuelle** | `nat_remb` (VARCHAR(1)), `mont_mut` (NUMERIC), `date_fin_mut` (DATE), `code_mut` (VARCHAR(2)) |
| **Technique** | `type_maj` (INTEGER NOT NULL), `version` (VARCHAR(10)), `date_synchro` (TIMESTAMP), `adresse_ip` (VARCHAR(15)), `nom_pc` (VARCHAR(30)) |
| **E-ordonnance** | `echifa` (BOOLEAN), `e_ord` (BOOLEAN) |

53 colonnes physiques (découvertes lors de BM-PHASE-003).

---

## Valeurs attendues par CHIFA vs ce que BM Pharma envoie

| Champ | CHIFA attend | BM Pharma envoie | Problème ? |
|-------|-------------|-----------------|------------|
| `etat` | `'0'` (brouillon) | `"0"` | ✅ OK |
| `num_bord` | NULL ou VARCHAR(6) | `null` | ✅ OK |
| `rang_ad` | VARCHAR(2) | `"1"` | ✅ OK |
| `tp` | CHAR(1) | `"1"` | ✅ OK |
| `taux` | CHAR(1) | `"3"` | 🟡 CHIFA utilise `'1'` ou `'2'` |
| `code_affect` | VARCHAR(2) | `"01"` | ✅ OK |
| `conv` | CHAR(1) | `"1"` | ✅ OK |
| `type_consult` | VARCHAR(2) | `"01"` | ✅ OK |
| `prescripteur` | VARCHAR(50) | `"BM PHARMA"` | ✅ OK |
| `risque` | CHAR(1) | `"0"` | ✅ OK |
| `statut_fact` | CHAR(1) | `"1"` | ✅ OK |
| `verifcms` | CHAR(1) | `"0"` | ✅ OK |
| `type_signature` | CHAR(1) | `"0"` (unsigned) | ✅ OK |
| `verif_fact` | CHAR(1) | `"0"` | ✅ OK |
| `mont_maj_fae` | NUMERIC(4,2) | `0` | ✅ OK |
| `mont_maj` | NUMERIC(11,2) | `0` | ✅ OK |
| `type_maj` | INTEGER NOT NULL | `0` | ✅ OK |
| `version` | VARCHAR(10) | `"2.0.0"` | ✅ OK |
| `signature` | XML | `null` | ✅ OK |
| `fact_xml` | XML | `null` | ✅ OK |
| `echifa` | BOOLEAN | `false` | ✅ OK |
| `e_ord` | BOOLEAN | `false` | ✅ OK |
| **`nat_remb`** | VARCHAR(1), **non null** | **NULL** | 🔴 Corriger |
| **`mont_mut`** | NUMERIC(10,2), **default 0** | **NULL** | 🔴 Corriger |
| **`date_fin_mut`** | DATE, **default '1900-01-01'** | **NULL** | 🔴 Corriger |
| **`date_synchro`** | TIMESTAMP, **default '1900-01-01'** | **NULL** | 🔴 Corriger |

---

## Le bug `Ts`

Dans `ChifaPostgresInvoiceService.cs:143` :
```csharp
Ts = false  // Toujours false
```

Alors que la requête porte `Ts = 4` et `ApplyLineDefaults` met `Ts` à 4 si 0.
`Ts` est mappé à `bool` dans l'entité → `4 == 1` donne `false`.
C'est un bug de type : la requête utilise `int` (4=tarif spécial, 1=tarif normal)
mais l'entité utilise `bool`.

---

## Ce que l'import CHIFA fait après écriture

La fonction `importdata()` normalise :

```sql
UPDATE facture SET nat_remb='0', mont_mut=0, date_fin_mut='01/01/1900'
WHERE taux='1';

UPDATE facture SET nat_remb='2', mont_mut=0, date_fin_mut='01/01/1900'
WHERE taux='2' AND (nat_remb IS NULL OR nat_remb <> '1');

UPDATE facture SET mont_mut=0 WHERE mont_mut IS NULL;
UPDATE facture SET date_fin_mut='01/01/1900' WHERE date_fin_mut IS NULL;
UPDATE facture SET date_synchro='01/01/1900';
```

BM Pharma doit appliquer ces mêmes normalisations **avant** l'insertion.
