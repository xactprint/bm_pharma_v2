# BM-PHASE-004.3 — REPORT: PostgreSQL Skeleton Context Wiring & Schema Discovery

**Date**: 2026-07-25
**Status**: PARTIAL — PostgreSQL not running on machine
**Decision**: **GO WITH RESTRICTIONS** (see Recommendation)

---

## 1. Résultat de la connexion PostgreSQL

| Item | Résultat |
|------|----------|
| Hôte | localhost:5432 |
| Base | CHIFA_OFFICINE |
| User | pharm |
| psql client | **Non installé** sur cette machine |
| Docker | **Installé mais Docker Desktop arrêté** (WSL: docker-desktop Stopped) |
| Service PostgreSQL | **Non trouvé** (ni service Windows, ni processus, ni installation native) |
| Diagnostic final | **PostgreSQL n'est pas accessible sur cette machine** |

**Action effectuée** : Un outil de schema discovery (`BMPharma.ChifaSchemaDiscovery`) a été créé et est prêt à être exécuté dès que PostgreSQL sera disponible.

---

## 2. Version PostgreSQL détectée

Non déterminable — le serveur n'est pas accessible.

**Note du DATABASE_CONTRACT** : La version documentée est PostgreSQL **9.3.4** (32-bit, EOL). Cette version est obsolète depuis 2018. Npgsql 8.x supporte PostgreSQL 9.3+ mais avec des limitations.

---

## 3. Tables réellement découvertes

**Basé sur le DATABASE_CONTRACT (BM_PHARMA_CHIFA_DATABASE_CONTRACT.md) :**

| Table | Colonnes (documentées) | Utilisation BM Pharma |
|-------|----------------------|----------------------|
| `facture` | 53 (34 write) | Écriture factures + read complet |
| `detail_fact` | 20 | Toutes colonnes requises en write |
| `bordereau` | 11 | Création, clôture bordereaux |
| `parametre` | 57 (5 critiques) | Lecture counters + pharmacie info |

**Note** : Les 53 colonnes de `facture` incluent des champs non documentés dans le contrat (motif_rejet, date_rejet, date_paiement, etc.) qui seront découverts lors du schema discovery réel.

---

## 4. Mapping PostgreSQL → EF Core (basé sur le contrat)

### 4.1 ChifaFacture → facture

| Propriété C# | Colonne PG | Type PG | Type C# | Nullable | Precision |
|-------------|-----------|---------|---------|----------|-----------|
| NumFact | num_fact | varchar(8) | string | NO | — |
| DateFact | date_fact | timestamp | DateTime? | YES | — |
| Etat | etat | char(1) | string? | YES | — |
| NumBord | num_bord | varchar(6) | string? | YES | — |
| MontOff | mont_off | numeric(10,2) | decimal? | YES | 10,2 |
| MontAs | mont_as | numeric(10,2) | decimal? | YES | 10,2 |
| MontFact | mont_fact | numeric(11,2) | decimal? | YES | 11,2 |
| NumAssure | num_assure | varchar(12) | string? | YES | — |
| CodeCentre | code_centre | varchar(5) | string? | YES | — |
| TypeMaj | type_maj | integer | int | **NO** | — |
| MontMajFae | mont_maj_fae | numeric(4,2) | decimal | **NO** | 4,2 |
| MontMaj | mont_maj | numeric(11,2) | decimal | **NO** | 11,2 |
| MontMut | mont_mut | numeric(10,2) | decimal? | YES | 10,2 |
| DateFinMut | date_fin_mut | date | DateTime? | YES | — |
| Version | version | varchar(10) | string? | YES | — |
| NumSeriePs | num_serie_ps | bigint | long? | YES | — |
| VersionCarte | version_carte | integer | int? | YES | — |
| Echifa | echifa | boolean | bool? | YES | — |
| IdFactEchifa | id_fact_echifa | bigint | long? | YES | — |
| EOrd | e_ord | boolean | bool? | YES | — |
| IdEOrd | id_e_ord | bigint | long? | YES | — |

### 4.2 ChifaDetailFact → detail_fact

| Propriété C# | Colonne PG | Type PG | Type C# | PK? |
|-------------|-----------|---------|---------|-----|
| NumFact | num_fact | varchar(8) | string | PK1 |
| NumEnr | num_enr | varchar(5) | string | PK2 |
| Ppa | ppa | numeric(10,2) | decimal | PK3 |
| Qte | qte | numeric(3,0) | decimal | — |
| Mont | mont | numeric(10,2) | decimal | — |
| MontAs | mont_as | numeric(10,2) | decimal? | — |
| MontPharm | mont_pharm | numeric(10,2) | decimal? | — |
| NumEnrPrescrit | num_enr_prescrit | varchar(5) | string | — |
| NumLot | num_lot | varchar(6) | string? | — |
| MajLocal | maj_local | numeric(10,2) | decimal? | — |
| MajSub | maj_sub | numeric(3,0) | decimal? | — |
| DureeTrait | duree_trait | numeric(3,0) | decimal? | — |
| TarifRef | tarif_ref | numeric(10,2) | decimal? | — |
| Posologie | posologie | varchar(50) | string? | — |
| Remboursable | remboursable | boolean | bool? | — |
| Local | local | boolean | bool? | — |
| InfTr | inf_tr | boolean | bool? | — |
| ApplicTr | applic_tr | boolean | bool? | — |
| Medic | medic | boolean | bool? | — |
| Ts | ts | boolean | bool? | — |

### 4.3 ChifaBordereau → bordereau

| Propriété C# | Colonne PG | Type PG | Type C# | Notes |
|-------------|-----------|---------|---------|-------|
| IdBord | id_bord | bigint | long | PK, auto-incrément |
| NumBord | num_bord | varchar(6) | string | Business PK |
| CodeCentre | code_centre | varchar(5) | string | NOT NULL |
| Etat | etat | char(1) | string? | NULL=open, 'C'=closed |
| IdUserCloture | id_user_cloture | integer | int? | NULL |
| PosteCloture | poste_cloture | varchar(100) | string? | NULL |
| MontVir | mont_vir | numeric(10,2) | decimal? | Default 0 |
| Duplicata | duplicata | boolean | bool? | Default false |
| DateCloture | date_cloture | timestamp | DateTime? | Default '1900-01-01' |
| DateOuverture | date_ouverture | timestamp | DateTime? | UtcNow |
| DateDepotFtp | date_depot_ftp | timestamp | DateTime? | Default '1900-01-01' |

### 4.4 ChifaParametre → parametre

| Propriété C# | Colonne PG | Type PG | Type C# | Notes |
|-------------|-----------|---------|---------|-------|
| CodePs | code_ps | varchar(10) | string? | — |
| CodeCentre | code_centre | varchar(5) | string? | — |
| NomPharmacie | nom_pharmacie | varchar(50) | string? | — |
| NextNumFact | next_num_fact | integer | int? | Counter |
| NextNumBord | next_num_bord | smallint | short? | Counter |

**Note** : parametre est une table **keyless** (single-row) dans notre modèle EF Core.

---

## 5. Liste complète des divergences potentielles

### Divergence 1: PostgreSQL 9.3.4 (EOL) vs Npgsql 8.x
- **Risque** : Npgsql 8.x supporte PostgreSQL 9.3 mais certaines fonctionnalités avancées (JSONB, tableaux, etc.) peuvent ne pas fonctionner
- **Impact** : Faible pour nos opérations CRUD simples
- **Action** : Valider avec schema discovery réel

### Divergence 2: Champs non documentés dans le contrat
- **Risque** : Le contrat documente 53 colonnes pour facture mais certaines pourraient porter des noms différents dans le schéma réel
- **Impact** : Les entités EF Core pourraient avoir des propriétés manquantes ou mal nommées
- **Action** : Le schema discovery tool validera chaque colonne

### Divergence 3: Type_maj — NOT NULL mais entity nullable
- **Risque** : `type_maj` est NOT NULL dans le contrat mais pourrait être nullable dans le schéma réel (les anciennes données pourraient avoir des NULLs)
- **Impact** : Exception EF Core si le schéma réel est plus permissif
- **Action** : Vérification via schema discovery — currently mappé comme `int` (non nullable) dans l'entité

### Divergence 4: Connection string — Trust=true vs TrustServerCertificate=true
- **Risque** : `Trust=true` n'est pas un paramètre valide pour Npgsql
- **Impact** : Erreur de connexion
- **Action** : **CORRIGÉ** — Tous les fichiers modifiés utilisent maintenant `TrustServerCertificate=true`

### Divergence 5: parametre — 57 colonnes vs 15 mappées
- **Risque** : Nous ne mappons que 15 des 57 colonnes documentées. Des colonnes critiques pourraient être manquantes
- **Impact** : Lecture partielle des paramètres
- **Action** : Le schema discovery ajoutera les colonnes manquantes

---

## 6. Colonnes incompatibles ou manquantes

** potentiellement manquantes (à valider) :**
- `facture` : Plusieurs colonnes non documentées dans le contrat pourraient exister dans le schéma réel
- `parametre` : 42 colonnes non mappées (sur 57 documentées)
- Le mapping détaillé doit être validé par le schema discovery réel

---

## 7. Problèmes de NULL/defaults

**Valeurs critiques NOT NULL (BM-SPEC-029) :**
- `facture.type_maj` → NOT NULL, default 0 ✅ mappé comme `int` non nullable
- `facture.mont_maj_fae` → NOT NULL, default 0 ✅ mappé comme `decimal` non nullable
- `facture.mont_maj` → NOT NULL, default 0 ✅ mappé comme `decimal` non nullable

**Valeurs avec defaults documentés :**
- `facture.nat_remb` → default '0'
- `facture.mont_mut` → default 0
- `facture.date_fin_mut` → default '1900-01-01'
- `facture.version` → '2.0.0'
- `bordereau.mont_vir` → default 0
- `bordereau.duplicata` → default false
- `bordereau.date_cloture` → default '1900-01-01'
- `bordereau.date_depot_ftp` → default '1900-01-01'

**Risque** : Si les defaults du schéma réel diffèrent des documentés, les entités EF Core pourraient recevoir des valeurs inattendues.

---

## 8. Résultat des lectures via EF Core

| Test | Résultat | Notes |
|------|----------|-------|
| CTX001-Création contexte ReadOnly | ✅ PASS | InMemory OK |
| CTX002-Création contexte Write | ✅ PASS | InMemory OK |
| CTX003-06-Table mappings | ✅ PASS | facture, detail_fact, bordereau, parametre |
| CTX007-10-PK configuration | ✅ PASS | PK unique, composite, bigint, no-key |
| CTX011-TypeMaj NOT NULL | ✅ PASS | Confirmé |
| CTX012-14-CRUD InMemory | ✅ PASS | Insert+Read facture, bordereau, detail_fact |
| CTX015-Parametre keyless | ✅ PASS | Confirmé keyless |
| CTX016-18-Column count | ✅ PASS | 48+, 20, 11 colonnes |
| CTX019-Write context same mappings | ✅ PASS | Cohérent |
| CTX020-Precision validation | ✅ PASS | 10/2, 11/2, 4/2 |

**Note** : Ces tests utilisent InMemory provider. La validation réelle contre PostgreSQL nécessite le schema discovery tool.

---

## 9. Résultat des tests ReadOnly

| Test | Résultat |
|------|----------|
| DI008-WriteGuard blocks in ReadOnly | ✅ PASS |
| DI009-WriteGuard allows in Test | ✅ PASS |
| DI010-WriteGuard allows in Production | ✅ PASS |
| DI001-ReadOnly mode registers stubs | ✅ PASS |

**Aucune écriture réelle n'a été tentée sur PostgreSQL.**

---

## 10. Confirmation des tests

| Suite | Tests | Résultat |
|-------|-------|----------|
| Domain.Tests | 6 | ✅ All pass |
| Application.Tests | 3 | ✅ All pass |
| ArchitectureTests | 7 | ✅ All pass |
| CHIFA.Tests (total) | 192 | ✅ All pass |
| **TOTAL** | **208** | **✅ 0 failures** |

---

## 11. Fichiers modifiés

| Fichier | Action | Description |
|---------|--------|-------------|
| `src/BMPharma.Persistence.PostgreSQL/Contexts/ChifaPostgreSqlContext.cs` | **Réécrit** | DbSets + entity configuration complète |
| `src/BMPharma.Persistence.PostgreSQL/Contexts/ChifaWriteDbContext.cs` | **Réécrit** | DbSets + entity configuration complète |
| `src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/ChifaFacture.cs` | **Créé** | 48 colonnes mappées |
| `src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/ChifaDetailFact.cs` | **Créé** | 20 colonnes mappées |
| `src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/ChifaBordereau.cs` | **Créé** | 11 colonnes mappées |
| `src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/ChifaParametre.cs` | **Créé** | 15 colonnes mappées |
| `src/BMPharma.CHIFA/DependencyInjection.cs` | **Modifié** | Trust→TrustServerCertificate |
| `src/BMPharma.UI/appsettings.json` | **Modifié** | Trust→TrustServerCertificate |
| `tests/BMPharma.CHIFA.Tests/ChifaDbContextTests.cs` | **Créé** | 20 tests EF Core |
| `tests/BMPharma.CHIFA.Tests/ChifaDependencyInjectionTests.cs` | **Créé** | 14 tests DI switching |
| `tools/BMPharma.ChifaSchemaDiscovery/Program.cs` | **Créé** | Outil schema discovery complet |
| `tools/BMPharma.ChifaSchemaDiscovery/BMPharma.ChifaSchemaDiscovery.csproj` | **Créé** | Projet console + Npgsql |

---

## 12. Risques identifiés

| # | Risque | Sévérité | Probabilité | Action requise |
|---|--------|----------|-------------|----------------|
| R1 | PostgreSQL non accessible — schema non validé | **CRITIQUE** | Confirmé | Démarrer PostgreSQL/Docker |
| R2 | PostgreSQL 9.3.4 EOL — compatibilité Npgsql | ÉLEVÉE | Moyenne | Valider avec schema discovery |
| R3 | Colonnes non documentées manquantes dans le mapping | ÉLEVÉE | Élevée | Schema discovery + ajustement |
| R4 | parametre — 42 colonnes non mappées | MOYENNE | Élevée | Schema discovery + ajout |
| R5 | Defaults réels vs documentés divergents | MOYENNE | Moyenne | Schema discovery |
| R6 | Types exacts (varchar width, numeric precision) divergents | ÉLEVÉE | Moyenne | Schema discovery |

---

## 13. Plan détaillé de la sous-phase 4.4

### 4.4.1 — Schema Discovery (PostgreSQL réel)
1. **Prérequis** : Démarrer PostgreSQL (Docker Desktop ou installation native)
2. Exécuter `BMPharma.ChifaSchemaDiscovery` pour obtenir le schéma réel
3. Comparer chaque colonne avec nos entités EF Core
4. Documenter les divergences colonne par colonne
5. Ajuster les entités EF Core si nécessaire

### 4.4.2 — DbContext Read-Only Validation
1. Tester `ChifaPostgreSqlContext` contre PostgreSQL réel en mode ReadOnly
2. Valider que SELECT fonctionne sur les 4 tables
3. Vérifier que les JOINs facture→bordereau fonctionnent
4. Tester les requêtes de lecture typiques (dernière facture, parametre)

### 4.4.3 — Service Wiring Validation
1. Tester `ChifaPostgresInvoiceService` en mode ReadOnly (lecture seule)
2. Tester `ChifaPostgresBordereauService` en mode ReadOnly
3. Valider que les stubs sont bien utilisés en mode ReadOnly
4. Valider que les services Postgres sont bien activés en mode Test

### 4.4.4 — Write Guard Integration Test
1. Tenter une écriture via `ChifaWriteDbContext` en mode ReadOnly → doit échouer
2. Tenter une écriture en mode Test → doit fonctionner (InMemory)
3. Valider le circuit complet DI→Guard→Service

### 4.4.5 — End-to-End Read Test
1. Créer une facture BM dans SQLite
2. Mapper via `ChifaInvoiceMapper`
3. Lire via `ChifaPostgreSqlContext` (simulated)
4. Valider la cohérence des données

### 4.4.6 — Documentation et rapport 4.4
1. Rapport complet avec résultats de chaque test
2. Mise à jour du DATABASE_CONTRACT si divergences trouvées
3. Recommandation GO / GO WITH RESTRICTIONS / STOP pour 4.5

---

## 14. Recommandation

### **GO WITH RESTRICTIONS**

**Raison** :
- ✅ L'architecture est solide et bien testée (208/208 tests)
- ✅ Les entités EF Core sont correctement configurées (tests InMemory passent)
- ✅ Le mode ReadOnly fonctionne avec les stubs
- ✅ Le schema discovery tool est prêt
- ⚠️ **RESTRICTION CRITIQUE** : PostgreSQL n'est pas accessible sur cette machine
- ⚠️ Le mapping réel n'a pas été validé contre le schéma PostgreSQL live

**Prérequis pour continuer** :
1. **Démarrer PostgreSQL** (Docker Desktop ou installation native)
2. **Exécuter le schema discovery tool** pour valider le schéma
3. **Ajuster les entités** si des divergences sont trouvées
4. **Uniquement après validation du schéma** → passer à 4.4

**Aucune écriture réelle CHIFA ne doit être effectuée tant que le schéma n'est pas validé.**
