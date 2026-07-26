# BM-PHASE-004.8 — COMPATIBILITY MATRIX

## Statut : 🟡 **PARTIELLEMENT VALIDÉ** (Docker OK, CHIFA réel BLOCKED)

**Date** : 2026-07-26  
**Sous-phase** : BM-PHASE-004.8 — First CHIFA Real Schema Discovery

---

## Matrice de Compatibilité

| # | Élément | Docker Test (16.14) | CHIFA Réel (9.3) | Statut Final | Preuve | Risque |
|---|---------|---------------------|------------------|--------------|--------|--------|
| 1 | facture — 53 colonnes | ✅ 53/53 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SchemaDiscovery | PG réel inconnu |
| 2 | detail_fact — 20 colonnes | ✅ 20/20 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SchemaDiscovery | PG réel inconnu |
| 3 | bordereau — 11 colonnes | ✅ 11/11 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SchemaDiscovery | PG réel inconnu |
| 4 | parametre — 15/57 colonnes | ✅ 15/15 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SchemaDiscovery | 42 colonnes manquantes |
| 5 | EF Core mapping | ✅ 88/88 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SchemaDiscovery | PG réel inconnu |
| 6 | BM-SPEC-028 (num_fact varchar(8)) | ✅ varchar(8) | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | 0 rows pour valider max |
| 7 | BM-SPEC-029 (NOT NULL) | ✅ PASS | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 8 | BM-SPEC-030 (code_centre varchar(5)) | ✅ varchar(5) | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 9 | BM-SPEC-031 (counters) | ✅ integer/smallint | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 10 | BM-SPEC-032 (bordereau auto-inc) | ✅ nextval(seq) | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 11 | FK detail_fact→facture | ✅ fk_detail_fact_facture | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 12 | FK facture→bordereau | ✅ fk_facture_bordereau | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 13 | Index facture | ✅ 4 index | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | pg_indexes | PG réel inconnu |
| 14 | Index detail_fact | ✅ 2 index | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | pg_indexes | PG réel inconnu |
| 15 | Index bordereau | ✅ 2 index | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | pg_indexes | PG réel inconnu |
| 16 | Sequences | ✅ 1 seq | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 17 | Compteurs (next_num_*) | ✅ 1/216 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SELECT direct | PG réel inconnu |
| 18 | NULL/Defaults | ✅ Valider | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 19 | CHECK constraints | ✅ Aucune | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | pg_constraint | PG réel inconnu |
| 20 | Triggers | ✅ Aucun | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 21 | Views | ✅ Aucune | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | information_schema | PG réel inconnu |
| 22 | PG version | ✅ 16.14 | ❌ BLOCKED | BLOCKED | SELECT version() | 9.3 vs 16.14 |
| 23 | Encoding | ✅ UTF8 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SELECT | PG réel inconnu |
| 24 | Max identifier length | ✅ 63 | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SELECT | PG réel inconnu |
| 25 | READ-ONLY verification | ✅ 4/4 tables | ❌ BLOCKED | PARTIELLEMENT VALIDÉ | SELECT count(*) | PG réel inconnu |

---

## Légende

| Statut | Signification |
|--------|---------------|
| ✅ VALIDÉ | Schéma réel vérifié, compatible |
| ❌ BLOCKED | PostgreSQL réel inaccessible, validation impossible |
| ⚠️ PARTIELLEMENT VALIDÉ | Docker test validé, CHIFA réel non vérifié |
| 🔴 DIVERGENCE | Différence détectée entre contrat et réalité |

---

## Résumé par Catégorie

### Architecture & Mapping
| Élément | Docker | Réel | Statut |
|---------|--------|------|--------|
| Tables (4) | ✅ | ❌ | PARTIEL |
| Colonnes facture (53) | ✅ | ❌ | PARTIEL |
| Colonnes detail_fact (20) | ✅ | ❌ | PARTIEL |
| Colonnes bordereau (11) | ✅ | ❌ | PARTIEL |
| Colonnes parametre (15) | ✅ | ❌ | PARTIEL |
| EF Core mapping (88) | ✅ | ❌ | PARTIEL |
| Primary keys | ✅ | ❌ | PARTIEL |
| Foreign keys | ✅ | ❌ | PARTIEL |
| Index | ✅ | ❌ | PARTIEL |
| Sequences | ✅ | ❌ | PARTIEL |

### BM-SPEC Validation
| Spécification | Docker | Réel | Statut |
|--------------|--------|------|--------|
| BM-SPEC-028 (num_fact varchar(8)) | ✅ | ❌ | PARTIEL |
| BM-SPEC-029 (NOT NULL) | ✅ | ❌ | PARTIEL |
| BM-SPEC-030 (code_centre varchar(5)) | ✅ | ❌ | PARTIEL |
| BM-SPEC-031 (counters) | ✅ | ❌ | PARTIEL |
| BM-SPEC-032 (bordereau auto-inc) | ✅ | ❌ | PARTIEL |

### Contraintes & Intégrité
| Élément | Docker | Réel | Statut |
|---------|--------|------|--------|
| CHECK constraints | ✅ Aucune | ❌ | PARTIEL |
| Triggers | ✅ Aucun | ❌ | PARTIEL |
| Views | ✅ Aucune | ❌ | PARTIEL |
| NULL/Defaults | ✅ Valider | ❌ | PARTIEL |
| FK relations | ✅ 2 FK | ❌ | PARTIEL |

### Environnement
| Élément | Docker | Réel | Statut |
|---------|--------|------|--------|
| PG version | ✅ 16.14 | ❌ BLOCKED | BLOCKED |
| Encoding | ✅ UTF8 | ❌ | PARTIEL |
| Read-only verification | ✅ 4/4 | ❌ | PARTIEL |
| Compteurs | ✅ 1/216 | ❌ | PARTIEL |

---

## Statistiques Finales

| Catégorie | Total | Validé | Blocked | Partiel |
|-----------|-------|--------|---------|---------|
| Éléments testés | 25 | 0 | 1 | 24 |
| % Validation | 100% | 0% | 4% | 96% |

**Conclusion** : 0% réellement validé, 96% partiellement validé (Docker OK), 4% bloqué (PG version).

---

## Recommandation

Pour transformer les 24 éléments "PARTIELLEMENT VALIDÉ" en "VALIDÉ", il faut :

1. **URGENT** : Rendre PostgreSQL CHIFA-OFFICINE accessible (port 5432)
2. **IMPORTANT** : Exécuter `dotnet run -- real`
3. **IMPORTANT** : Comparer Docker vs Réel
4. **IMPORTANT** : Mettre à jour cette matrice
