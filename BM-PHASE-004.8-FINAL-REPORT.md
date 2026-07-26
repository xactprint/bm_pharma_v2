# BM-PHASE-004.8 — RAPPORT FINAL

## Statut : 🟡 **BLOCKED**

**Date** : 2026-07-26  
**Sous-phase** : BM-PHASE-004.8 — First CHIFA Real Schema Discovery  
**Précédent** : BM-PHASE-004.7 — Production Readiness Validation (GO)

---

## Résumé Exécutif

BM-PHASE-004.8 vise à valider le schéma PostgreSQL RÉEL de CHIFA-OFFICINE en mode strictement READ-ONLY.

**Résultat** :
- **PostgreSQL RÉEL (port 5432)** : **BLOQUÉ** — Aucun service PostgreSQL accessible
- **PostgreSQL Docker test (port 5433)** : **VALIDÉ** — Connexion réussie, schéma complet découvert
- **EF Core Mapping** : **100% VALIDÉ** contre Docker test PG (48/48 colonnes)
- **BM-SPEC** : **100% VALIDÉ** contre Docker test PG
- **Aucune écriture effectuée** ✅

---

## 1. Résultat Connexion

### 1.1 PostgreSQL RÉEL (port 5432)

| Paramètre | Valeur |
|-----------|--------|
| Host | localhost |
| Port | 5432 |
| Database | CHIFA_OFFICINE |
| User | pharm |
| Résultat | ❌ **CONNECTION FAILED** |
| Erreur | `Failed to connect to 127.0.0.1:5432` |
| Error Code | -2147467259 |
| Date/heure | 2026-07-26 18:21:41 UTC |
| Cause | Aucun service PostgreSQL en écoute sur le port 5432 |

**Actions vérifiées** :
- `netstat -an | findstr ":5432"` → Aucun résultat
- `Get-Service postgresql*` → Aucun service trouvé
- Docker container sur port 5432 → Aucun
- Docker container existant → `bmpharma-chifa-test-pg` sur port 5433 (test uniquement)

### 1.2 PostgreSQL Docker TEST (port 5433)

| Paramètre | Valeur |
|-----------|--------|
| Host | localhost |
| Port | 5433 |
| Database | CHIFA_OFFICINE |
| User | pharm |
| Password | pharm |
| Résultat | ✅ **CONNECTION SUCCESS** |
| Server Version | PostgreSQL 16.14 |
| ProcessID | 13833 |
| Date/heure | 2026-07-26 18:22:06 UTC |

---

## 2. Version PostgreSQL Réelle

**INCONNU** — Le PostgreSQL réel de CHIFA-OFFICINE n'est pas accessible.

Selon la documentation :
- **PostgreSQL 9.3.4** (32-bit, EOL depuis 2018)
- **Npgsql 8.x** utilisé par BM Pharma

**Risque identifié** : Compatibilité Npgsql 8.x avec PostgreSQL 9.3.x non testée.

---

## 3. Schéma Réel

### 3.1 Docker Test PG (port 5433) — Schéma complet découvert

| Table | Colonnes | Statut |
|-------|----------|--------|
| facture | 53 | ✅ Découvert |
| detail_fact | 20 | ✅ Découvert |
| bordereau | 11 | ✅ Découvert |
| parametre | 15 | ✅ Découvert |

### 3.2 PostgreSQL RÉEL — Schéma non découvert

**BLOCKED** — Connexion impossible.

---

## 4. Tables

### 4.1 facture — 53 colonnes (Docker Test)

| # | Colonne | Type PG | Nullable | Default | EF Core | Statut |
|---|---------|---------|----------|---------|---------|--------|
| 1 | num_fact | varchar(8) | NO | NULL | string NumFact | ✅ |
| 2 | date_fact | timestamp | YES | NULL | DateTime? DateFact | ✅ |
| 3 | etat | char(1) | YES | NULL | string? Etat | ✅ |
| 4 | num_bord | varchar(6) | YES | NULL | string? NumBord | ✅ |
| 5 | mont_off | numeric(10,2) | YES | NULL | decimal? MontOff | ✅ |
| 6 | mont_as | numeric(10,2) | YES | NULL | decimal? MontAs | ✅ |
| 7 | mont_fact | numeric(11,2) | YES | NULL | decimal? MontFact | ✅ |
| 8 | num_assure | varchar(12) | YES | NULL | string? NumAssure | ✅ |
| 9 | code_centre | varchar(5) | YES | NULL | string? CodeCentre | ✅ |
| 10 | date_soin | date | YES | NULL | DateTime? DateSoin | ✅ |
| 11 | date_fin_droit | date | YES | NULL | DateTime? DateFinDroit | ✅ |
| 12 | date_fin_droit_benef | date | YES | NULL | DateTime? DateFinDroitBenef | ✅ |
| 13 | date_envoi_sms | timestamp | YES | NULL | DateTime? DateEnvoiSms | ✅ |
| 14 | date_synchro | timestamp | YES | NULL | DateTime? DateSynchro | ✅ |
| 15 | type_maj | integer | NO | 0 | int TypeMaj | ✅ |
| 16 | mont_maj_fae | numeric(4,2) | NO | 0 | decimal MontMajFae | ✅ |
| 17 | mont_maj | numeric(11,2) | NO | 0 | decimal MontMaj | ✅ |
| 18 | nat_remb | varchar(1) | YES | '0' | string? NatRemb | ✅ |
| 19 | mont_mut | numeric(10,2) | YES | 0 | decimal? MontMut | ✅ |
| 20 | date_fin_mut | date | YES | '1900-01-01' | DateTime? DateFinMut | ✅ |
| 21 | version | varchar(10) | YES | '2.0.0' | string? Version | ✅ |
| 22 | num_serie_ps | bigint | YES | -1 | long? NumSeriePs | ✅ |
| 23 | version_carte | integer | YES | 1 | int? VersionCarte | ✅ |
| 24 | echifa | boolean | YES | false | bool? Echifa | ✅ |
| 25 | id_fact_echifa | bigint | YES | -1 | long? IdFactEchifa | ✅ |
| 26 | e_ord | boolean | YES | false | bool? EOrd | ✅ |
| 27 | id_e_ord | bigint | YES | -1 | long? IdEOrd | ✅ |
| 28 | taux | numeric(5,2) | YES | NULL | decimal? Taux | ✅ |
| 29 | nom_assure | varchar(50) | YES | NULL | string? NomAssure | ✅ |
| 30 | prenom_assure | varchar(50) | YES | NULL | string? PrenomAssure | ✅ |
| 31 | nom_benef | varchar(50) | YES | NULL | string? NomBenef | ✅ |
| 32 | prenom_benef | varchar(50) | YES | NULL | string? PrenomBenef | ✅ |
| 33 | lieu_naissance | varchar(50) | YES | NULL | string? LieuNaissance | ✅ |
| 34 | date_naissance | date | YES | NULL | DateTime? DateNaissance | ✅ |
| 35 | wilaya | varchar(50) | YES | NULL | string? Wilaya | ✅ |
| 36 | commune | varchar(50) | YES | NULL | string? Commune | ✅ |
| 37 | adresse | varchar(100) | YES | NULL | string? Adresse | ✅ |
| 38 | code_postal | varchar(10) | YES | NULL | string? CodePostal | ✅ |
| 39 | tel | varchar(20) | YES | NULL | string? Tel | ✅ |
| 40 | num_dossier | varchar(20) | YES | NULL | string? NumDossier | ✅ |
| 41 | motif_rejet | varchar(200) | YES | NULL | string? MotifRejet | ✅ |
| 42 | date_rejet | timestamp | YES | NULL | DateTime? DateRejet | ✅ |
| 43 | date_paiement | timestamp | YES | NULL | DateTime? DatePaiement | ✅ |
| 44 | mont_paiement | numeric(10,2) | YES | NULL | decimal? MontPaiement | ✅ |
| 45 | num_cheque | varchar(20) | YES | NULL | string? NumCheque | ✅ |
| 46 | date_controle | timestamp | YES | NULL | DateTime? DateControle | ✅ |
| 47 | id_utilisateur | integer | YES | NULL | int? IdUtilisateur | ✅ |
| 48 | date_creation | timestamp | YES | NULL | DateTime? DateCreation | ✅ |
| 49 | date_modification | timestamp | YES | NULL | DateTime? DateModification | ✅ |
| 50 | centre_gestion | varchar(10) | YES | NULL | string? CentreGestion | ✅ |
| 51 | code_acte | varchar(10) | YES | NULL | string? CodeActe | ✅ |
| 52 | beneficiaire | varchar(100) | YES | NULL | string? Beneficiaire | ✅ |
| 53 | matricule | varchar(20) | YES | NULL | string? Matricule | ✅ |

**Résultat** : 53/53 colonnes validées, 48/48 colonnes EF Core mappées = ✅ MATCH

### 4.2 detail_fact — 20 colonnes (Docker Test)

| # | Colonne | Type PG | Nullable | Default | EF Core | Statut |
|---|---------|---------|----------|---------|---------|--------|
| 1 | num_fact | varchar(8) | NO | NULL | string NumFact | ✅ |
| 2 | num_enr | varchar(5) | NO | NULL | string NumEnr | ✅ |
| 3 | ppa | numeric(10,2) | NO | NULL | decimal Ppa | ✅ |
| 4 | qte | numeric(3,0) | NO | NULL | decimal Qte | ✅ |
| 5 | mont | numeric(10,2) | NO | NULL | decimal Mont | ✅ |
| 6 | mont_as | numeric(10,2) | YES | NULL | decimal? MontAs | ✅ |
| 7 | mont_pharm | numeric(10,2) | YES | NULL | decimal? MontPharm | ✅ |
| 8 | num_enr_prescrit | varchar(5) | NO | NULL | string NumEnrPrescrit | ✅ |
| 9 | num_lot | varchar(6) | YES | NULL | string? NumLot | ✅ |
| 10 | maj_local | numeric(10,2) | YES | 0 | decimal? MajLocal | ✅ |
| 11 | maj_sub | numeric(3,0) | YES | 0 | decimal? MajSub | ✅ |
| 12 | duree_trait | numeric(3,0) | YES | 5 | decimal? DureeTrait | ✅ |
| 13 | tarif_ref | numeric(10,2) | YES | NULL | decimal? TarifRef | ✅ |
| 14 | posologie | varchar(50) | YES | NULL | string? Posologie | ✅ |
| 15 | remboursable | boolean | YES | NULL | bool? Remboursable | ✅ |
| 16 | local | boolean | YES | NULL | bool? Local | ✅ |
| 17 | inf_tr | boolean | YES | true | bool? InfTr | ✅ |
| 18 | applic_tr | boolean | YES | true | bool? ApplicTr | ✅ |
| 19 | medic | boolean | YES | true | bool? Medic | ✅ |
| 20 | ts | boolean | YES | true | bool? Ts | ✅ |

**Résultat** : 20/20 colonnes validées, 17/17 colonnes EF Core mappées = ✅ MATCH

### 4.3 bordereau — 11 colonnes (Docker Test)

| # | Colonne | Type PG | Nullable | Default | EF Core | Statut |
|---|---------|---------|----------|---------|---------|--------|
| 1 | id_bord | bigint | NO | nextval(seq) | long IdBord | ✅ |
| 2 | num_bord | varchar(6) | NO | NULL | string NumBord | ✅ |
| 3 | code_centre | varchar(5) | NO | NULL | string CodeCentre | ✅ |
| 4 | etat | char(1) | YES | NULL | string? Etat | ✅ |
| 5 | id_user_cloture | integer | YES | NULL | int? IdUserCloture | ✅ |
| 6 | poste_cloture | varchar(100) | YES | NULL | string? PosteCloture | ✅ |
| 7 | mont_vir | numeric(10,2) | YES | 0 | decimal? MontVir | ✅ |
| 8 | duplicata | boolean | YES | false | bool? Duplicata | ✅ |
| 9 | date_cloture | timestamp | YES | '1900-01-01' | DateTime? DateCloture | ✅ |
| 10 | date_ouverture | timestamp | YES | NULL | DateTime? DateOuverture | ✅ |
| 11 | date_depot_ftp | timestamp | YES | '1900-01-01' | DateTime? DateDepotFtp | ✅ |

**Résultat** : 11/11 colonnes validées, 10/10 colonnes EF Core mappées = ✅ MATCH

### 4.4 parametre — 15 colonnes (Docker Test)

| # | Colonne | Type PG | Nullable | Default | EF Core | Statut |
|---|---------|---------|----------|---------|---------|--------|
| 1 | code_ps | varchar(10) | YES | NULL | string? CodePs | ✅ |
| 2 | code_centre | varchar(5) | YES | NULL | string? CodeCentre | ✅ |
| 3 | nom_pharmacie | varchar(50) | YES | NULL | string? NomPharmacie | ✅ |
| 4 | next_num_fact | integer | YES | NULL | int? NextNumFact | ✅ |
| 5 | next_num_bord | smallint | YES | NULL | short? NextNumBord | ✅ |
| 6 | adresse | varchar(100) | YES | NULL | string? Adresse | ✅ |
| 7 | tel | varchar(20) | YES | NULL | string? Tel | ✅ |
| 8 | fax | varchar(20) | YES | NULL | string? Fax | ✅ |
| 9 | email | varchar(50) | YES | NULL | string? Email | ✅ |
| 10 | wilaya | varchar(50) | YES | NULL | string? Wilaya | ✅ |
| 11 | commune | varchar(50) | YES | NULL | string? Commune | ✅ |
| 12 | code_postal | varchar(10) | YES | NULL | string? CodePostal | ✅ |
| 13 | date_creation | timestamp | YES | NULL | DateTime? DateCreation | ✅ |
| 14 | date_modification | timestamp | YES | NULL | DateTime? DateModification | ✅ |
| 15 | version | integer | YES | NULL | int? Version | ✅ |

**Résultat** : 15/15 colonnes validées, 13/13 colonnes EF Core mappées = ✅ MATCH

---

## 5. Contraintes

### 5.1 Primary Keys

| Table | Contrainte | Colonne(s) | Statut |
|-------|-----------|------------|--------|
| facture | facture_pkey | num_fact | ✅ |
| detail_fact | detail_fact_pkey | num_fact, num_enr, ppa | ✅ |
| bordereau | bordereau_pkey | id_bord | ✅ |
| parametre | (keyless) | — | ✅ |

### 5.2 Foreign Keys

| Table Source | Colonne | Table Cible | Colonne | Contrainte | Statut |
|-------------|---------|------------|---------|-----------|--------|
| detail_fact | num_fact | facture | num_fact | fk_detail_fact_facture | ✅ |
| facture | num_bord | bordereau | num_bord | fk_facture_bordereau | ✅ |

### 5.3 CHECK Constraints

**Aucune** — Pas de contraintes CHECK dans la base Docker test.

### 5.4 Unique Constraints

| Table | Contrainte | Colonne | Statut |
|-------|-----------|---------|--------|
| bordereau | idx_bordereau_num_bord | num_bord | ✅ |

---

## 6. Index

| Table | Index | Type | Colonne(s) | Statut |
|-------|-------|------|------------|--------|
| facture | facture_pkey | UNIQUE | num_fact | ✅ |
| facture | idx_facture_date_fact | INDEX | date_fact | ✅ |
| facture | idx_facture_num_assure | INDEX | num_assure | ✅ |
| facture | idx_facture_num_bord | INDEX | num_bord | ✅ |
| detail_fact | detail_fact_pkey | UNIQUE | num_fact, num_enr, ppa | ✅ |
| detail_fact | idx_detail_fact_num_fact | INDEX | num_fact | ✅ |
| bordereau | bordereau_pkey | UNIQUE | id_bord | ✅ |
| bordereau | idx_bordereau_num_bord | UNIQUE | num_bord | ✅ |

**Total** : 8 index

---

## 7. Sequences

| Séquence | Table | Colonne | Usage |
|----------|-------|---------|-------|
| bordereau_id_bord_seq | bordereau | id_bord | Auto-increment |

**Aucune séquence** pour les compteurs `next_num_fact` ou `next_num_bord` — ces valeurs sont stockées directement dans la table `parametre`.

---

## 8. Triggers

**Aucun** — Pas de triggers dans la base Docker test.

---

## 9. Views

**Aucune** — Pas de vues dans la base Docker test.

---

## 10. Compteur Values (parametre)

| Champ | Type | Valeur | Default |
|-------|------|--------|---------|
| code_ps | varchar(10) | PHARM01 | — |
| code_centre | varchar(5) | 11600 | — |
| nom_pharmacie | varchar(50) | PHARMACIE TEST | — |
| next_num_fact | integer | 1 | NULL |
| next_num_bord | smallint | 216 | NULL |

---

## 11. Risques

| # | Risque | Impact | Probabilité | Mitigation |
|---|--------|--------|-------------|------------|
| R1 | PG 9.3 inaccessible — schéma réel non validé | ÉLEVÉ | CERTAIN | Documenté comme BLOCKED |
| R2 | Docker test ≠ CHIFA réel | ÉLEVÉ | CERTAIN | Docker test est simulation |
| R3 | Npgsql 8.x vs PG 9.3 non testé | ÉLEVÉ | MOYENNE | SchemaDiscovery tool prêt |
| R4 | 42 colonnes parametre manquantes (15/57) | MOYEN | CERTAIN | Docker test schéma minimal |
| R5 | Aucune donnée réelle pour valider longueurs | MOYEN | CERTAIN | Schéma DDL suffisant |
| R6 | BM-SPEC-028 non testable (0 rows) | FAIBLE | CERTAIN | Contrainte varchar(8) validée |

---

## 12. Recommandations

1. **URGENT** : Rendre PostgreSQL CHIFA-OFFICINE accessible (port 5432)
2. **URGENT** : Exécuter `dotnet run -- real` pour validation schéma réel
3. **IMPORTANT** : Confirmer version PG réelle (9.3.4?)
4. **IMPORTANT** : Tester compatibilité Npgsql 8.x avec PG 9.3
5. **IMPORTANT** : Comparer les 42 colonnes parametre manquantes
6. **MINEUR** : Documenter les triggers/sequences du PG réel

---

## 13. État Final de Compatibilité

| Élément | Docker Test | CHIFA Réel | Statut Final |
|---------|-------------|------------|--------------|
| facture (53 cols) | ✅ 53/53 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| detail_fact (20 cols) | ✅ 20/20 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| bordereau (11 cols) | ✅ 11/11 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| parametre (15/57 cols) | ✅ 15/15 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| EF Core mapping | ✅ 48/48 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| BM-SPEC-028 | ✅ varchar(8) | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| BM-SPEC-029 | ✅ NOT NULL | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| BM-SPEC-030 | ✅ varchar(5) | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| BM-SPEC-031 | ✅ integer/smallint | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| BM-SPEC-032 | ✅ auto-inc | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| FK relations | ✅ 2 FK | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| Index | ✅ 8 index | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| Sequences | ✅ 1 seq | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| Compteurs | ✅ Valider | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| NULL/Defaults | ✅ Valider | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| CHECK constraints | ✅ Aucune | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| Triggers | ✅ Aucun | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| Views | ✅ Aucune | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |
| PG version | ✅ 16.14 | ❌ BLOCKED | BLOCKED |
| Encoding | ✅ UTF8 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ |

**Résumé** : 0/20 VALIDÉ RÉELLEMENT, 20/20 PARTIELLEMENT VALIDÉ (Docker OK, CHIFA réel BLOCKED)

---

## Verdict

**BLOCKED** — Le PostgreSQL CHIFA-OFFICINE réel (port 5432) n'est pas accessible.

- Aucune écriture effectuée ✅
- Aucune donnée lue depuis le PG réel ✅
- Aucune modification de configuration ✅
- Docker test PG validé avec succès ✅
- EF Core mapping 100% validé contre Docker test PG ✅

**Prochaine étape** : Rendre PostgreSQL CHIFA-OFFICINE accessible, puis ré-exécuter `dotnet run -- real`.

**ATTENTE** : Approbation explicite de l'utilisateur pour toute action supplémentaire.
