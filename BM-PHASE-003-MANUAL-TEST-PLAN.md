# BM-PHASE-003 — MANUAL TEST PROTOCOL

## Classification

### A. Tests sans risque (Local Only)
1. ✅ Créer une facture draft dans SQLite
2. ✅ Ajouter des lignes à la facture
3. ✅ Vérifier les calculs (montant total, part assurée, reste à charge)
4. ✅ Vérifier le mapping BM Pharma → CHIFA
5. ✅ Vérifier la machine d'état (transitions autorisées)
6. ✅ Vérifier les messages d'erreur

### B. Tests en environnement de simulation (FakeChifaIntegrationProvider)
7. ✅ Exécuter le workflow complet via FakeChifa
8. ✅ Vérifier la création facture simulée
9. ✅ Vérifier la création bordereau simulée
10. ✅ Vérifier que la signature est requise (token absent)
11. ✅ Simuler la signature avec token
12. ✅ Simuler la clôture après signature
13. ✅ Vérifier l'audit structuré
14. ✅ Vérifier le comportement offline

### C. Tests PostgreSQL CHIFA en READ-ONLY
15. 🔍 Vérifier la connexion à PostgreSQL CHIFA
16. 🔍 Lire les factures existantes
17. 🔍 Vérifier les compteurs
18. 🔍 Vérifier la cohérence des données

### D. Tests nécessitant approbation explicite
19. ⚠️ Écrire une facture dans PostgreSQL CHIFA (mode Test)
20. ⚠️ Écrire un bordereau dans PostgreSQL CHIFA (mode Test)
21. ⚠️ Vérifier le rollback en cas d'erreur
22. ⚠️ Tester le ChifaWriteGuard en mode Production

### E. Tests nécessitant CHIFA-OFFICINE
23. 🔒 Vérifier que CHIFA-OFFICINE détecte la facture
24. 🔒 Présenter la carte professionnelle pour signature
25. 🔒 Vérifier la signature dans CHIFA-OFFICINE
26. 🔒 Clôturer le bordereau dans CHIFA-OFFICINE

### F. Tests nécessitant le token professionnel
27. 🔒 Insérer le token professionnel
28. 🔒 Effectuer la signature cryptographique
29. 🔒 Vérifier la validité de la signature
30. 🔒 Transmettre le bordereau au CNAS

## Prérequis

| Environnement | Requis |
|--------------|--------|
| SQLite | Base bmpharma.db créée |
| PostgreSQL | Connexion configurée (appsettings) |
| CHIFA-OFFICINE | Installé et configuré |
| Token professionnel | Disponible pour tests E/F |

## Notes de sécurité

- Aucun test de catégorie E/F ne doit être exécuté sans supervision
- Toute écriture PostgreSQL nécessite validation préalable
- Le mode ReadOnly doit être rétabli après tout test en mode Test/Production
