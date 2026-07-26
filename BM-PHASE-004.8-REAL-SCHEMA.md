# BM-PHASE-004.8 — REAL SCHEMA

## Statut : 🟡 **BLOCKED** (PostgreSQL réel inaccessible)

**Date** : 2026-07-26  
**Sous-phase** : BM-PHASE-004.8 — First CHIFA Real Schema Discovery

---

## Résumé

Le schéma PostgreSQL RÉEL de CHIFA-OFFICINE n'a pas pu être découvert car le serveur PostgreSQL sur le port 5432 n'est pas accessible.

Le schéma Docker test (port 5433) a été découvert avec succès et documenté ci-dessous.

---

## 1. PostgreSQL RÉEL — BLOCKED

| Paramètre | Valeur |
|-----------|--------|
| Host | localhost |
| Port | 5432 |
| Database | CHIFA_OFFICINE |
| User | pharm |
| Résultat | ❌ CONNECTION FAILED |
| Erreur | Failed to connect to 127.0.0.1:5432 |
| Date/heure | 2026-07-26 18:21:41 UTC |

**Vérifications effectuées** :
- `netstat -an | findstr ":5432"` → Aucun résultat (port non ouvert)
- `Get-Service postgresql*` → Aucun service trouvé
- Docker container sur port 5432 → Aucun
- Docker container existant → `bmpharma-chifa-test-pg` sur port 5433 (test uniquement)

---

## 2. Docker Test PG — Schéma Complet Découvert

### 2.1 Version et Settings

| Paramètre | Valeur |
|-----------|--------|
| Server Version | PostgreSQL 16.14 |
| Version Num | 160014 |
| Encoding | UTF8 |
| Max Identifier Length | 63 |
| ProcessID | 13833 |

### 2.2 Tables

| Table | Colonnes | Rows |
|-------|----------|------|
| facture | 53 | 0 |
| detail_fact | 20 | 0 |
| bordereau | 11 | 0 |
| parametre | 15 | 1 |

### 2.3 Colonnes par Table

#### facture (53 colonnes)

```
num_fact              varchar(8)          NOT NULL  PK
date_fact             timestamp           NULL
etat                  char(1)             NULL
num_bord              varchar(6)          NULL
mont_off              numeric(10,2)       NULL
mont_as               numeric(10,2)       NULL
mont_fact             numeric(11,2)       NULL
num_assure            varchar(12)         NULL
code_centre           varchar(5)          NULL
date_soin             date                NULL
date_fin_droit        date                NULL
date_fin_droit_benef  date                NULL
date_envoi_sms        timestamp           NULL
date_synchro          timestamp           NULL
type_maj              integer             NOT NULL  DEFAULT 0
mont_maj_fae          numeric(4,2)        NOT NULL  DEFAULT 0
mont_maj              numeric(11,2)       NOT NULL  DEFAULT 0
nat_remb              varchar(1)          NULL      DEFAULT '0'
mont_mut              numeric(10,2)       NULL      DEFAULT 0
date_fin_mut          date                NULL      DEFAULT '1900-01-01'
version               varchar(10)         NULL      DEFAULT '2.0.0'
num_serie_ps          bigint              NULL      DEFAULT -1
version_carte         integer             NULL      DEFAULT 1
echifa                boolean             NULL      DEFAULT false
id_fact_echifa        bigint              NULL      DEFAULT -1
e_ord                 boolean             NULL      DEFAULT false
id_e_ord              bigint              NULL      DEFAULT -1
taux                  numeric(5,2)        NULL
nom_assure            varchar(50)         NULL
prenom_assure         varchar(50)         NULL
nom_benef             varchar(50)         NULL
prenom_benef          varchar(50)         NULL
lieu_naissance        varchar(50)         NULL
date_naissance        date                NULL
wilaya                varchar(50)         NULL
commune               varchar(50)         NULL
adresse               varchar(100)        NULL
code_postal           varchar(10)         NULL
tel                   varchar(20)         NULL
num_dossier           varchar(20)         NULL
motif_rejet           varchar(200)        NULL
date_rejet            timestamp           NULL
date_paiement         timestamp           NULL
mont_paiement         numeric(10,2)       NULL
num_cheque            varchar(20)         NULL
date_controle         timestamp           NULL
id_utilisateur        integer             NULL
date_creation         timestamp           NULL
date_modification     timestamp           NULL
centre_gestion        varchar(10)         NULL
code_acte             varchar(10)         NULL
beneficiaire          varchar(100)        NULL
matricule             varchar(20)         NULL
```

#### detail_fact (20 colonnes)

```
num_fact          varchar(8)          NOT NULL  PK1, FK
num_enr           varchar(5)          NOT NULL  PK2
ppa               numeric(10,2)       NOT NULL  PK3
qte               numeric(3,0)        NOT NULL
mont              numeric(10,2)       NOT NULL
mont_as           numeric(10,2)       NULL
mont_pharm        numeric(10,2)       NULL
num_enr_prescrit  varchar(5)          NOT NULL
num_lot           varchar(6)          NULL
maj_local         numeric(10,2)       NULL      DEFAULT 0
maj_sub           numeric(3,0)        NULL      DEFAULT 0
duree_trait       numeric(3,0)        NULL      DEFAULT 5
tarif_ref         numeric(10,2)       NULL
posologie         varchar(50)         NULL
remboursable      boolean             NULL
local             boolean             NULL
inf_tr            boolean             NULL      DEFAULT true
applic_tr         boolean             NULL      DEFAULT true
medic             boolean             NULL      DEFAULT true
ts                boolean             NULL      DEFAULT true
```

#### bordereau (11 colonnes)

```
id_bord          bigint              NOT NULL  PK (auto-inc via sequence)
num_bord         varchar(6)          NOT NULL  UNIQUE
code_centre      varchar(5)          NOT NULL
etat             char(1)             NULL
id_user_cloture  integer             NULL
poste_cloture    varchar(100)        NULL
mont_vir         numeric(10,2)       NULL      DEFAULT 0
duplicata        boolean             NULL      DEFAULT false
date_cloture     timestamp           NULL      DEFAULT '1900-01-01 00:00:00'
date_ouverture   timestamp           NULL
date_depot_ftp   timestamp           NULL      DEFAULT '1900-01-01 00:00:00'
```

#### parametre (15 colonnes)

```
code_ps            varchar(10)         NULL
code_centre        varchar(5)          NULL
nom_pharmacie      varchar(50)         NULL
next_num_fact      integer             NULL
next_num_bord      smallint            NULL
adresse            varchar(100)        NULL
tel                varchar(20)         NULL
fax                varchar(20)         NULL
email              varchar(50)         NULL
wilaya             varchar(50)         NULL
commune            varchar(50)         NULL
code_postal        varchar(10)         NULL
date_creation      timestamp           NULL
date_modification  timestamp           NULL
version            integer             NULL
```

### 2.4 Primary Keys

| Table | Contrainte | Colonne(s) |
|-------|-----------|------------|
| facture | facture_pkey | num_fact |
| detail_fact | detail_fact_pkey | num_fact + num_enr + ppa |
| bordereau | bordereau_pkey | id_bord |
| parametre | (keyless) | — |

### 2.5 Foreign Keys

| Source | Colonne | Cible | Colonne | Contrainte |
|--------|---------|-------|---------|-----------|
| detail_fact | num_fact | facture | num_fact | fk_detail_fact_facture |
| facture | num_bord | bordereau | num_bord | fk_facture_bordereau |

### 2.6 Index

| Table | Index | Type | Colonne(s) |
|-------|-------|------|------------|
| facture | facture_pkey | UNIQUE | num_fact |
| facture | idx_facture_date_fact | INDEX | date_fact |
| facture | idx_facture_num_assure | INDEX | num_assure |
| facture | idx_facture_num_bord | INDEX | num_bord |
| detail_fact | detail_fact_pkey | UNIQUE | num_fact + num_enr + ppa |
| detail_fact | idx_detail_fact_num_fact | INDEX | num_fact |
| bordereau | bordereau_pkey | UNIQUE | id_bord |
| bordereau | idx_bordereau_num_bord | UNIQUE | num_bord |

### 2.7 Sequences

| Séquence | Table | Colonne | Auto-Inc |
|----------|-------|---------|----------|
| bordereau_id_bord_seq | bordereau | id_bord | OUI |

### 2.8 CHECK Constraints

**Aucune**

### 2.9 Triggers

**Aucun**

### 2.10 Views

**Aucune**

### 2.11 Compteur Values

| Champ | Type | Valeur Actuelle |
|-------|------|-----------------|
| code_ps | varchar(10) | PHARM01 |
| code_centre | varchar(5) | 11600 |
| nom_pharmacie | varchar(50) | PHARMACIE TEST |
| next_num_fact | integer | 1 |
| next_num_bord | smallint | 216 |

---

## 3. Conclusions

1. **Docker test PG** : Schéma complet et validé
2. **PostgreSQL réel** : BLOCKED — Aucun accès possible
3. **Comparaison Docker vs Contrat** : Conforme
4. **Comparaison EF Core vs Docker** : 100% compatible
5. **Aucune écriture effectuée** ✅

---

## 4. Prochaines Étapes

1. Rendre PostgreSQL CHIFA-OFFICINE accessible (port 5432)
2. Exécuter `dotnet run -- real` pour découverte schéma réel
3. Comparer Docker vs Réel
4. Mettre à jour les matrices de compatibilité
