# 016-G — Failure Injection

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Simuler des pannes et valider le comportement résilient de l'application.

---

## Règles

| Règle | Description |
|-------|-------------|
| **R1** | Les tests sont effectués sur l'environnement pilote |
| **R2** | Une sauvegarde complète est faite avant chaque test |
| **R3** | L'activité métier est interrompue le temps du test uniquement |
| **R4** | Chaque test est documenté avec le comportement observé |

---

## Scénario 1 — PostgreSQL Indisponible

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 1.1 | Arrêter service PostgreSQL : `Stop-Service postgresql-9.3` | Service arrêté | ☐ OK / ☐ KO |
| 1.2 | Lancer BM Pharma | Message "Base de données inaccessible" | ☐ OK / ☐ KO |
| 1.3 | Vérifier Dashboard | Indicateur PostgreSQL rouge | ☐ OK / ☐ KO |
| 1.4 | Vérifier que l'application ne crashe pas | UI responsive (hors DB) | ☐ OK / ☐ KO |
| 1.5 | Vérifier logs | Erreur de connexion loggée | ☐ OK / ☐ KO |
| 1.6 | Vérifier retry | Tentatives de reconnexion loggées | ☐ OK / ☐ KO |
| 1.7 | Redémarrer PostgreSQL : `Start-Service postgresql-9.3` | Service démarré | ☐ OK / ☐ KO |
| 1.8 | Vérifier reconnexion automatique dans Dashboard | Indicateur redevient vert | ☐ OK / ☐ KO |

**Temps d'indisponibilité :** minutes  
**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Scénario 2 — CHIFA Fermé

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 2.1 | Fermer CHIFA-OFFICINE | Processus terminé | ☐ OK / ☐ KO |
| 2.2 | Lancer BM Pharma | Message "CHIFA inaccessible" | ☐ OK / ☐ KO |
| 2.3 | Vérifier Dashboard | Indicateur CHIFA rouge | ☐at OK / ☐ KO |
| 2.4 | Vérifier que l'application ne crashe pas | UI responsive | ☐ OK / ☐ KO |
| 2.5 | Vérifier circuit breaker | S'ouvre après 3 échecs | ☐ OK / ☐ KO |
| 2.6 | Vérifier logs | "Circuit breaker state: Open" | ☐ OK / ☐ KO |
| 2.7 | Rouvrir CHIFA-OFFICINE | CHIFA disponible | ☐ OK / ☐ KO |
| 2.8 | Vérifier reconnexion automatique | Indicateur redevient vert | ☐ OK / ☐ KO |

**Temps d'indisponibilité :** minutes  
**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Scénario 3 — Réseau Coupé

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 3.1 | Déconnecter le câble réseau / désactiver Wi-Fi | Réseau coupé | ☐ OK / ☐ KO |
| 3.2 | Lancer BM Pharma | Démarre en mode dégradé | ☐ OK / ☐ KO |
| 3.3 | Vérifier Dashboard | PostgreSQL et CHIFA rouges | ☐ OK / ☐ KO |
| 3.4 | Vérifier logs | Erreurs réseau loggées | ☐ OK / ☐ KO |
| 3.5 | Vérifier que l'application ne crashe pas | UI responsive, pas de freeze | ☐ OK / ☐ KO |
| 3.6 | Reconnecter le réseau | Réseau rétabli | ☐ OK / ☐ KO |
| 3.7 | Vérifier reconnexion automatique | Connexions rétablies | ☐ OK / ☐ KO |

**Temps d'indisponibilité :** minutes  
**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Scénario 4 — SAM Absent

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 4.1 | Vérifier que SAM est actuellement accessible | SAM OK (état initial) | ☐ OK / ☐ KO |
| 4.2 | Désactiver ou déconnecter SAM | SAM inaccessible | ☐ OK / ☐ KO |
| 4.3 | Vérifier Diagnostic Center T10 | "SAM inaccessible" | ☐ OK / ☐ KO |
| 4.4 | Vérifier que le reste de l'application fonctionne | Fonctionnalités non-SAM OK | ☐ OK / ☐ KO |
| 4.5 | Rétablir SAM | SAM de nouveau accessible | ☐ OK / ☐ KO |

**Temps d'indisponibilité :** minutes  
**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Scénario 5 — Accès Refusé

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 5.1 | Lancer BM Pharma sans droits admin | Démarrage normal | ☐ OK / ☐ KO |
| 5.2 | Vérifier permissions `%ProgramData%\BMPharma\logs\` | Lecture/Écriture OK | ☐ OK / ☐ KO |
| 5.3 | Vérifier permissions `%ProgramData%\BMPharma\backups\` | Lecture/Écriture OK | ☐ OK / ☐ KO |
| 5.4 | Vérifier connexion PostgreSQL avec utilisateur `pharm` | OK | ☐ OK / ☐ KO |

**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Scénario 6 — Disque Plein

| # | Action | Comportement Attendu | Résultat |
|---|--------|----------------------|----------|
| 6.1 | Remplir l'espace disque (fichier temporaire) | Disque quasi-plein | ☐ OK / ☐ KO |
| 6.2 | Vérifier Diagnostic Center T14 | Alerte espace disque | ☐ OK / ☐ KO |
| 6.3 | Vérifier logs | Log plein | ☐ OK / ☐ KO |
| 6.4 | Vérifier que l'application ne crashe pas | UI fonctionnelle | ☐ OK / ☐ KO |
| 6.5 | Libérer l'espace disque | Espace rétabli | ☐ OK / ☐ KO |
| 6.6 | Vérifier reprise des logs | Logs à nouveau écrits | ☐ OK / ☐ KO |

**Temps d'indisponibilité :** minutes  
**Comportement anormal :** ☐ Oui / ☐ Non  

---

## Résumé des Tests

| Scénario | Résultat | Temps d'indisponibilité | Anomalie |
|----------|----------|------------------------|----------|
| 1. PostgreSQL indisponible | ☐ OK / ☐ KO | min | ☐ Oui / ☐ Non |
| 2. CHIFA fermé | ☐ OK / ☐ KO | min | ☐ Oui / ☐ Non |
| 3. Réseau coupé | ☐ OK / ☐ KO | min | ☐ Oui / ☐ Non |
| 4. SAM absent | ☐ OK / ☐ KO | min | ☐ Oui / ☐ Non |
| 5. Accès refusé | ☐ OK / ☐ KO | — | ☐ Oui / ☐ Non |
| 6. Disque plein | ☐ OK / ☐ KO | min | ☐ Oui / ☐ Non |

---

## Anomalies

| # | Scénario | Description | Impact | Criticité |
|---|----------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| L'application ne crashe pas en cas de panne | ☐ OK / ☐ KO |
| Messages d'erreur clairs pour l'utilisateur | ☐ OK / ☐ KO |
| Reconnexion automatique après rétablissement | ☐ OK / ☐ KO |
| Circuit breaker fonctionnel | ☐ OK / ☐ KO |
| Dashboard reflète l'état réel | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
