# BM-PHASE-008-A — MATRICE DE PREUVES

**Document:** 008-A-EVIDENCE-MATRIX
**Date:** 2026-07-28

---

## Légende

| Symbole | Signification |
|---------|---------------|
| ✅ PROUVÉE | Testé réellement, résultats vérifiés |
| 🟡 DÉDUIT | Forte probabilité, preuve indirecte |
| ❓ INCONNU | Aucune information disponible |
| 🔃 À TESTER | Prochaine phase (008-B) |
| 🔴 BLOQUÉ | Impossible sans dépendance externe |

---

## Preuves BM Pharma → CHIFA-OFFICINE

### Niveau 1 : Écriture base

| Preuve | Résultat | Source |
|--------|----------|--------|
| Chaîne EF Core → Npgsql → PostgreSQL ✓ | ✅ PROUVÉE | Phase 007-G |
| `INSERT INTO facture` via EF Core | ✅ PROUVÉE | Phase 007-G |
| `INSERT INTO detail_fact` via EF Core | ✅ PROUVÉE | Phase 007-G |
| Transaction ReadCommitted + Commit | ✅ PROUVÉE | Phase 007-G |
| Rollback transactionnel | ✅ PROUVÉE | Phase 005-F |
| Connexion TCP 127.0.0.1:5432 | ✅ PROUVÉE | Phase 005-C |
| `signature` type XML → EF Core | ✅ PROUVÉE | Phase 007-G (fix) |
| `fact_xml` type XML → EF Core | ✅ PROUVÉE | Phase 007-G (fix) |
| `date_fact` timestamp without tz | ✅ PROUVÉE | Phase 007-G (fix) |
| `date_soin` type date | ✅ PROUVÉE | Phase 007-G (fix) |

### Niveau 2 : Compteurs

| Preuve | Résultat | Source |
|--------|----------|--------|
| `parametre` table accessible | ✅ PROUVÉE | Phase 005-C |
| `next_num_fact` lu | ✅ PROUVÉE | Phase 005-C |
| `next_num_bord` lu | ✅ PROUVÉE | Phase 005-C |
| Atomicité UPDATE...RETURNING | 🟡 DÉDUIT | Code analysis |
| `GetNextInvoiceNumberAsync()` fonctionnel | 🟡 DÉDUIT | Code + base |
| `GetNextBordereauNumberAsync()` fonctionnel | 🟡 DÉDUIT | Code + base |

### Niveau 3 : Validation

| Preuve | Résultat | Source |
|--------|----------|--------|
| `ChifaInvoiceValidator` 9 règles | ✅ PROUVÉE | Phase 007-E, 007-G |
| `ChifaBordereauValidator` 4 règles | 🟡 DÉDUIT | Code + InMemory |
| `ChifaWriteGuard` mode Test | ✅ PROUVÉE | Phase 007-G |
| `ChifaWriteGuard` mode ReadOnly | ✅ PROUVÉE | Tests unitaires |

---

## Preuves CHIFA-OFFICINE

### Niveau 4 : Visibilité

| Preuve | Résultat | Source |
|--------|----------|--------|
| Consultation Facture affiche TST001 | 🟡 DÉDUIT | Phase 005-E (log visuel) |
| Consultation Facture affiche TST002 | ❓ Pas testé | Phase 007-G n'a pas ouvert CHIFA |
| Visualiser Bordereau affiche bordereau | ❓ INCONNU | Théorie `detail_bord` non confirmée |
| `DataSet1.detail_bord` jointure | 🟡 DÉDUIT | Reverse engineering CHIFA |
| `FBordereau` DataSource | 🟡 DÉDUIT | Reverse engineering CHIFA |

### Niveau 5 : Signature

| Preuve | Résultat | Source |
|--------|----------|--------|
| Architecture signature | ✅ CONNU | DLL discovery |
| p7sign.dll charge opensc-pkcs11 | ✅ CONNU | Dependency analysis |
| Token Identiv uTrust 3512 | ✅ CONNU | Config scan |
| PIN pinpad hardware | ✅ CONNU | RE IL code |
| SAM session sign | ✅ CONNU | RE cnas/chifa |
| BM Pharma peut signer | 🔴 IMPOSSIBLE | Aucun token physique |

### Niveau 6 : Clôture

| Preuve | Résultat | Source |
|--------|----------|--------|
| `cloturerbord()` fonction PostgreSQL | ✅ CONNU | Lecture PL/pgSQL |
| PG_Program.exe CREATE_DIR | ✅ CONNU | Lecture PL/pgSQL |
| PG_Program.exe WRITE_FILE | ✅ CONNU | Lecture PL/pgSQL |
| PG_Program.exe CLOTURER | ✅ CONNU | Lecture PL/pgSQL |
| Format nom fichier .P7M | ✅ CONNU | Lecture PL/pgSQL |
| BM Pharma peut exécuter cloturerbord | ❓ Potentiel | Impossible sans signature |

### Niveau 7 : Transmission CNAS

| Preuve | Résultat | Source |
|--------|----------|--------|
| Serveur FTP 41.111.149.250 | ✅ CONNU | Log d'erreur FTP |
| Mode passif | ✅ CONNU | Log d'erreur FTP |
| Auth chiffrée APICNAS.config | ✅ CONNU | File discovery |
| BM Pharma peut transmettre | 🔴 IMPOSSIBLE | Sans cloture ni credentials |

### Niveau 8 : Import

| Preuve | Résultat | Source |
|--------|----------|--------|
| `importdata()` fonction | ✅ CONNU | Lecture PL/pgSQL |
| `importtable()` fonction | ✅ CONNU | Lecture PL/pgSQL |
| Tables staging facture2/detail_fact2 | ✅ CONNU | DDL discovery |
| Normalisation post-import | ✅ CONNU | Lecture PL/pgSQL |
| Format CSV semicolon WIN1252 | ✅ CONNU | Lecture PL/pgSQL |

---

## Résumé des gaps

| Gap | Sévérité | Solution |
|-----|----------|----------|
| `nat_remb` = NULL | Haute | Fix BM Pharma → `'0'` ou `'2'` selon taux |
| `mont_mut` = NULL | Haute | Fix BM Pharma → `0` |
| `date_fin_mut` = NULL | Haute | Fix BM Pharma → `'1900-01-01'` |
| `date_synchro` = NULL | Haute | Fix BM Pharma → `'1900-01-01'` |
| `Ts` = false (bug) | Moyenne | Fix BM Pharma → mappage `bool` vs `int` |
| Visibilité bordereau non testée | Haute | À tester Phase 008-B |
| Signature impossible | Bloquant | Dépend de CHIFA-OFFICINE |
| Clôture impossible | Bloquant | Dépend de signature |
| Transmission impossible | Bloquant | Dépend de clôture |

---

## Preuves manquantes à acquérir en Phase 008-B

| # | Preuve | Méthode | Risque |
|---|--------|---------|--------|
| 1 | Création bordereau → visible dans Visualiser Bordereau | INSERT bordereau + UPDATE facture SET num_bord | Aucun (rollbackable) |
| 2 | Facture avec `num_bord` lié | Créer facture avec `num_bord != NULL` | Aucun (rollbackable) |
| 3 | `detail_bord` jointure requiert `detail_fact` | Facture sans detail_fact → invisible ? | Aucun (rollbackable) |
| 4 | `nat_remb` / `mont_mut` / `date_synchro` obligatoires | INSERT avec NULL vs DEFAULT | Aucun (rollbackable) |
