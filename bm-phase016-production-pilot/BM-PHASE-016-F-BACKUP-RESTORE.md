# 016-F — Backup & Restore Validation

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Valider les procédures de sauvegarde, restauration, rollback et reprise après incident.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Auteur test** | |
| **Date** | |
| **Base PostgreSQL** | CHIFA_OFFICINE |
| **Base SQLite** | BMPharma.db |

---

## 1. Sauvegarde PostgreSQL

### 1.1 Sauvegarde Automatique

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 1.1.1 | Vérifier planification Task Scheduler | Tâche `BMPharma-Backup-PG` présente, planifiée à 02:00 | ☐ OK / ☐ KO |
| 1.1.2 | Exécuter la tâche manuellement | `backup-pg.ps1` s'exécute sans erreur | ☐ OK / ☐ KO |
| 1.1.3 | Vérifier fichier backup créé | `CHIFA_OFFICINE-{date}.backup` présent | ☐ OK / ☐ KO |
| 1.1.4 | Vérifier taille du backup | > 1KB | ☐ OK / ☐ KO |
| 1.1.5 | Vérifier log de backup | `CHIFA_OFFICINE-{date}.backup.log` présent, pas d'erreur | ☐ OK / ☐ KO |

### 1.2 Sauvegarde Manuelle

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 1.2.1 | Lancer `pg_dump -h localhost -p 5432 -U pharm -F c -b -f test.backup CHIFA_OFFICINE` | Backup créé | ☐ OK / ☐ KO |
| 1.2.2 | Vérifier intégrité | `pg_restore -l test.backup` liste les objets | ☐ OK / ☐ KO |

---

## 2. Restauration PostgreSQL

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 2.1 | Créer base de test `CHIFA_RESTORE_TEST` | Base créée | ☐ OK / ☐ KO |
| 2.2 | Restaurer backup : `pg_restore -h localhost -p 5432 -U pharm -d CHIFA_RESTORE_TEST backup.backup` | Restauration réussie | ☐ OK / ☐ KO |
| 2.3 | Vérifier nombre de tables | Même nombre qu'original | ☐ OK / ☐ KO |
| 2.4 | Vérifier nombre d'enregistrements | Même nombre qu'original | ☐ OK / ☐ KO |
| 2.5 | Vérifier une donnée spécifique | Donnée présente et correcte | ☐ OK / ☐ KO |
| 2.6 | Supprimer base de test | `DROP DATABASE CHIFA_RESTORE_TEST` | ☐ OK / ☐ KO |

---

## 3. Sauvegarde SQLite

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 3.1 | Identifier fichier SQLite | `BMPharma.db` dans `%ProgramData%\BMPharma\data\` | ☐ OK / ☐ KO |
| 3.2 | Copie manuelle vers backup | Copie réussie | ☐ OK / ☐ KO |
| 3.3 | Vérifier intégrité : `sqlite3 BMPharma.db "PRAGMA integrity_check;"` | `ok` | ☐ OK / ☐ KO |

---

## 4. Restauration SQLite

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 4.1 | Copier backup vers `BMPharma.db.restored` | Fichier copié | ☐ OK / ☐ KO |
| 4.2 | Lancer BM Pharma avec base restaurée | Application démarre | ☐ OK / ☐ KO |
| 4.3 | Vérifier données restaurées | Données visibles dans l'UI | ☐ OK / ☐ KO |

---

## 5. Rollback de Version

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 5.1 | Installer version précédente (MSI) | Installation réussie | ☐ OK / ☐ KO |
| 5.2 | Vérifier que la base PostgreSQL est compatible | Connexion OK | ☐at OK / ☐ KO |
| 5.3 | Restaurer backup PostgreSQL de la version précédente | Restauration réussie | ☐ OK / ☐ KO |
| 5.4 | Vérifier que l'application fonctionne | Lancement + données OK | ☐ OK / ☐ KO |

---

## 6. Reprise Après Incident

### Scénario : Crash BM Pharma

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 6.1 | Fermer brutalement BM Pharma (End Task) | Processus terminé | ☐ OK / ☐ KO |
| 6.2 | Relancer BM Pharma | Redémarrage sans corruption | ☐ OK / ☐ KO |
| 6.3 | Vérifier l'intégrité de la base SQLite | `PRAGMA integrity_check` = ok | ☐ OK / ☐ KO |
| 6.4 | Vérifier l'intégrité de PostgreSQL | Requêtes fonctionnent | ☐ OK / ☐ KO |

### Scénario : Panne PostgreSQL

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 6.5 | Arrêter service PostgreSQL | Service arrêté | ☐ OK / ☐ KO |
| 6.6 | Relancer BM Pharma | Message "Base de données inaccessible" | ☐ OK / ☐ KO |
| 6.7 | Redémarrer PostgreSQL | `Start-Service postgresql-9.3` | ☐ OK / ☐ KO |
| 6.8 | Relancer BM Pharma | Application fonctionne | ☐ OK / ☐ KO |
| 6.9 | Restaurer dernier backup si nécessaire | Données restaurées | ☐ OK / ☐ KO |

---

## 7. Résumé

| Test | Résultat |
|------|----------|
| Sauvegarde PostgreSQL automatique | ☐ OK / ☐ KO |
| Sauvegarde PostgreSQL manuelle | ☐ OK / ☐ KO |
| Restauration PostgreSQL | ☐ OK / ☐ KO |
| Sauvegarde SQLite | ☐ OK / ☐ KO |
| Restauration SQLite | ☐ OK / ☐ KO |
| Rollback version | ☐ OK / ☐ KO |
| Reprise après crash BM Pharma | ☐ OK / ☐ KO |
| Reprise après panne PostgreSQL | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |

---

## Anomalies

| # | Scénario | Description | Impact | Criticité |
|---|----------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| Backup fonctionnel | ☐ OK / ☐ KO |
| Restauration fonctionnelle | ☐ OK / ☐ KO |
| Rollback possible | ☐ OK / ☐ KO |
| Reprise après incident | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
