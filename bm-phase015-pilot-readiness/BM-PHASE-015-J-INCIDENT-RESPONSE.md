# 015-J — Incident Response

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir la procédure de réponse aux incidents pendant le pilote.

---

## 1. Incident Severity Matrix

| Niveau | Label | Temps réponse | Temps résolution | Exemples |
|--------|-------|---------------|------------------|----------|
| **S1** | **CRITICAL** | < 15 min | < 2h | Base inaccessible, perte données, impossibilité de facturer |
| **S2** | **MAJOR** | < 30 min | < 4h | CHIFA déconnecté, synchronisation bloquée, erreur récurrente |
| **S3** | **MINOR** | < 2h | < 24h | Bug UI, performance dégradée, message d'erreur incorrect |
| **S4** | **LOW** | < 24h | < 72h | Suggestion, amélioration cosmétique, documentation |

---

## 2. Contact Tree

### Support Pilot

| Rôle | Nom / Contact | Disponibilité |
|------|--------------|---------------|
| **Support N1** | Pharmacie pilote (utilisateur) | Heures ouvrées |
| **Support N2** | Équipe support BM Pharma | 08:00–19:00 (J+7) |
| **Support N3** | Développeur (on-call) | 08:00–22:00 (J+7) |
| **Support N4** | DBA PostgreSQL | Si nécessaire |

### Escalade

```
S1 ── 15 min ──▶ N2 ── 30 min ──▶ N3 ── 1h ──▶ N4
S2 ── 30 min ──▶ N2 ── 1h ──────▶ N3
S3 ── 2h ──────▶ N2
S4 ── 24h ─────▶ N2 (ticket)
```

---

## 3. Incident Response Procedure

### Détection

| Source | Méthode |
|--------|---------|
| Pharmacien | Appel téléphonique, email, message |
| Dashboard monitoring | Auto-détection (alerte CRITICAL/ERROR) |
| Diagnostic Center | Tests programmés (quotidiens) |
| Logs | Analyse automatique (seuil d'erreurs dépassé) |

### Enregistrement

Tout incident doit être enregistré dans un ticket avec :

```
Incident #{id}
─────────────
Date/Horaire : 2026-07-29 08:30
Pharmacie    : [Nom pharmacie]
Sévérité     : S1/S2/S3/S4
Description  : [Description du problème]
Impact       : [Impact sur l'activité]
Workaround   : [Solution de contournement si existante]
Statut       : Ouvert / En cours / Résolu / Fermé
```

### Processus standard

```
DÉTECTION ──▶ ENREGISTREMENT ──▶ DIAGNOSTIC ──▶ RÉSOLUTION ──▶ VÉRIFICATION ──▶ FERMETURE
    │               │                │               │               │
    ▼               ▼                ▼               ▼               ▼
  Pharmacien     Ticket #       Analyser         Appliquer       Pharmacien
  ou alerte      Support        logs + bundle    correctif       confirme
```

---

## 4. Runbooks par Incident Type

### RUNBOOK S1-01 — Base de données inaccessible

| Étape | Action | Responsable |
|-------|--------|-------------|
| 1 | Vérifier état PostgreSQL : `pg_isready -h localhost -p 5432` | N2 |
| 2 | Vérifier service Windows : `Get-Service postgresql*` | N2 |
| 3 | Vérifier logs PostgreSQL : `%ProgramFiles%\PostgreSQL\9.3\data\pg_log\*.log` | N2 |
| 4 | Redémarrer service : `Restart-Service postgresql-9.3` | N2 |
| 5 | Vérifier connexion Dashboard | N2 |
| 6 | Si persiste, escalader N3 pour analyse | N2 |

### RUNBOOK S1-02 — CHIFA déconnecté

| Étape | Action | Responsable |
|-------|--------|-------------|
| 1 | Vérifier que CHIFA-OFFICINE est ouvert et connecté | N1 |
| 2 | Vérifier paramètres connexion (Diagnostic Center T05) | N2 |
| 3 | Vérifier réseau : `ping chifa-server` | N2 |
| 4 | Redémarrer CHIFA-OFFICINE | N1 |
| 5 | Redémarrer BM Pharma | N1 |
| 6 | Si persiste, escalader N3 | N2 |

### RUNBOOK S2-01 — Synchronisation bloquée

| Étape | Action | Responsable |
|-------|--------|-------------|
| 1 | Vérifier Dashboard : statut synchronisation | N2 |
| 2 | Vérifier connexion PostgreSQL et CHIFA | N2 |
| 3 | Déclencher synchronisation manuelle | N2 |
| 4 | Analyser logs synchronisation | N3 |
| 5 | Réinitialiser état synchronisation si nécessaire | N3 |

### RUNBOOK S2-02 — Erreurs répétées

| Étape | Action | Responsable |
|-------|--------|-------------|
| 1 | Générer support bundle | N2 |
| 2 | Analyser logs pour identifier le pattern | N2/N3 |
| 3 | Isoler le composant défaillant | N3 |
| 4 | Appliquer correctif ou workaround | N3 |

---

## 5. Communication Templates

### Notification Incident (email)

```
Objet : [S1/S2] Incident BM Pharma — [Pharmacie] — [Description courte]

Incident #{id} — [STATUT]

Pharmacie : [Nom]
Sévérité  : [S1/S2/S3/S4]
Début     : [Date/heure]
Problème  : [Description]
Impact    : [Impact]
Workaround: [Oui/Non]

Prochaine mise à jour : [Heure]
```

### Rapport Post-Mortem

| Champ | Description |
|-------|-------------|
| **Incident** | #ID, date, durée |
| **Résumé** | Quoi, quand, impact |
| **Cause racine** | Pourquoi c'est arrivé |
| **Résolution** | Comment c'a été résolu |
| **Leçons apprises** | Ce qu'on a appris |
| **Actions** | Correctifs, tests, documentation |
| **Responsable** | Qui suit les actions |

---

## 6. Post-Mortem Process

Déclenché pour tout incident S1 ou S2.

| Étape | Délai | Livrable |
|-------|-------|----------|
| Analyse immédiate | < 24h | Cause racine identifiée |
| Correctif | < 48h | Patch ou hotfix déployé |
| Rapport post-mortem | < 72h | Document partagé avec l'équipe |
| Validation pharmacien | < 7j | Pharmacien confirme résolution |

---

## 7. Outils de Diagnostic

| Outil | Usage | Documentation |
|-------|-------|---------------|
| Diagnostic Center (CLI) | Tests rapides/support bundle | 015-B, 015-F |
| Logs Serilog | Analyse détaillée | 015-E |
| Dashboard monitoring | État temps réel | 015-I |
| Support Bundle ZIP | Diagnostic complet | 015-F |

---

## 8. Période de Supervision Renforcée

Pendant les 7 premiers jours (J+0 à J+7), la supervision est renforcée :

| Mesure | Détail |
|--------|--------|
| Support N2 disponible | 08:00–19:00 |
| Support N3 on-call | 08:00–22:00 |
| Revue des logs | Quotidienne |
| Dashboard monitoring | 60s refresh |
| Contact pharmacien | Appel quotidien J+0 à J+3 |
