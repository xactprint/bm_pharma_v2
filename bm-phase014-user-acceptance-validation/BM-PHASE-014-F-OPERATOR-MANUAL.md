# 014-F — Operator Manual

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Audience:** Pharmacien utilisateur  
**Version logiciel:** BM Pharma v2.0  

---

## 1. Installation

### Configuration minimale requise
- **OS :** Windows 10 ou 11 (64-bit)
- **RAM :** 4 Go minimum
- **Espace disque :** 500 Mo
- **Réseau :** Connexion au serveur PostgreSQL CHIFA_OFFICINE
- **Lecteur de carte :** PKCS#11 pour token professionnel (optionnel)

### Procédure d'installation
1. Exécuter le programme d'installation `BMPharma-Setup.exe`
2. Suivre les instructions à l'écran
3. L'application se lance automatiquement après installation

---

## 2. Configuration Initiale

### Fichier `appsettings.json`

L'application crée automatiquement un fichier de configuration au premier démarrage dans le dossier d'installation :

```json
{
  "CHIFA": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=***",
    "Mode": "ReadOnly",
    "Schema": "public",
    "ApplicationPath": "C:\\Program Files\\CHIFA-OFFICINE\\"
  }
}
```

### Paramètres à configurer avant utilisation

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `ConnectionString` | Chaîne de connexion PostgreSQL CHIFA | À configurer |
| `Mode` | Mode d'intégration (ReadOnly/Test/Production) | `ReadOnly` |
| `ApplicationPath` | Chemin d'installation de CHIFA-OFFICINE | À configurer |

> **⚠ Important :** En mode `ReadOnly` (par défaut), aucune donnée n'est écrite dans CHIFA. C'est le mode recommandé pour la découverte.

---

## 3. Premier Démarrage

1. Lancer BM Pharma depuis le menu Démarrer ou le raccourci bureau
2. La fenêtre principale s'affiche avec :
   - **Barre de titre :** "BM Pharma v2"
   - **Header vert :** Logo + tagline
   - **Sidebar gauche :** Navigation
   - **Status bar :** "Prêt" + "CHIFA: Déconnecté" (initialement)
3. Ouvrir le **Tableau de bord CHIFA** depuis la sidebar
4. Vérifier que le statut PostgreSQL est "Connecté" (carte verte)
5. Si CHIFA-OFFICINE est en cours d'exécution, le statut passe "En ligne"

---

## 4. Workflow Quotidien

### 4.1 Vérification Matinale

1. Ouvrir le **Tableau de bord CHIFA**
2. Vérifier les 6 cartes de statut (vert = OK)
3. Vérifier l'état du Circuit Breaker (doit être "Closed" = vert)
4. Vérifier les métriques de monitoring
5. Si tout est vert, le système est prêt

### 4.2 Préparation d'une Facture CHIFA

1. Dans la sidebar, cliquer sur **Préparation Factures**
2. Remplir le formulaire :
   - **N° facture :** Identifiant unique (max 8 caractères)
   - **N° assurance :** Numéro d'assuré social
   - **Code centre :** 11600 (par défaut, modifiable)
   - **Date soin :** Date des soins
   - **Patient :** Nom du patient (informationnel)
3. Ajouter des lignes de facture avec le bouton **+ Ajouter ligne**
4. Pour chaque ligne, saisir :
   - N° enregistrement
   - Produit (nom)
   - Code CIP
   - Quantité
   - Prix unitaire
5. Vérifier les montants calculés automatiquement
6. Cliquer sur **Aperçu** pour vérifier avant soumission
7. Cliquer sur **Valider** pour valider la structure
8. Cliquer sur **Préparer et soumettre** pour exécuter le workflow complet

> **Note :** En mode ReadOnly, la validation et la soumission sont simulées. Un message orange vous informe du mode.

### 4.3 Suivi des Bordereaux

1. Dans la sidebar, cliquer sur **Statut Bordereau**
2. La liste des bordereaux s'affiche avec leur état
3. Cliquer sur un bordereau pour voir le détail
4. Les actions disponibles (selon l'état) :
   - **Valider** : Valider la structure du bordereau
   - **Signer** : Signer le bordereau (requiert token PKCS#11)
   - **Clôturer** : Clôturer le bordereau
   - **Transmettre** : Transmettre à la CNAS

### 4.4 Fin de Journée

1. Vérifier que toutes les factures du jour sont préparées
2. Vérifier les bordereaux en attente
3. Consulter le journal d'audit pour tracer les opérations
4. Le dashboard s'actualise automatiquement toutes les 30 secondes

---

## 5. Résolution de Problèmes

### Problème : PostgreSQL non connecté

**Symptôme :** Carte PostgreSQL rouge "Déconnecté"

**Causes possibles :**
- Serveur PostgreSQL arrêté
- Configuration réseau incorrecte
- Firewall bloquant le port 5432

**Solution :**
1. Vérifier que le service PostgreSQL est démarré
2. Vérifier la connectivité réseau : `ping <serveur_pg>`
3. Vérifier le fichier `appsettings.json` (ConnectionString)
4. Consulter les logs dans `logs/bmpharma-.log`

---

### Problème : CHIFA-OFFICINE non détecté

**Symptôme :** Carte CHIFA rouge "Hors ligne"

**Causes possibles :**
- CHIFA-OFFICINE n'est pas démarré
- SAM (carte) absente
- Chemin d'application incorrect

**Solution :**
1. Démarrer CHIFA-OFFICINE
2. Vérifier que la SAM est insérée
3. Vérifier le paramètre `ApplicationPath` dans la configuration

---

### Problème : Token PKCS#11 non détecté

**Symptôme :** Dashboard affiche "ACTION REQUISE" — Token absent

**Solution :**
1. Insérer le token professionnel dans le lecteur
2. Attendre le prochain rafraîchissement automatique (30s max)
3. Ou cliquer sur **Actualiser** dans le dashboard

---

### Problème : Erreur de validation facture

**Symptôme :** Panneau rouge "Échec de validation" avec liste d'erreurs

**Solutions par code d'erreur :**

| Code | Problème | Solution |
|------|----------|----------|
| V001 | N° facture manquant | Saisir un numéro de facture |
| V002 | N° assurance manquant | Saisir le numéro d'assurance |
| V005 | Aucune ligne | Ajouter au moins un produit |
| V006 | Quantité invalide | Vérifier que la quantité est > 0 |
| V007 | Prix invalide | Vérifier que le prix unitaire est > 0 |

---

### Problème : L'application freeze ou ralentit

**Symptôme :** Interface non réactive

**Solutions :**
1. Attendre la fin de l'opération en cours (les opérations réseau peuvent prendre quelques secondes)
2. Si le freeze persiste, vérifier les logs dans `logs/bmpharma-.log`
3. Redémarrer l'application

---

## 6. FAQ

| Question | Réponse |
|----------|---------|
| **Que faire si une facture est rejetée ?** | Consulter les erreurs de validation, corriger et soumettre à nouveau. |
| **Puis-je modifier une facture après soumission ?** | Non. Le workflow est séquentiel. Une fois soumise, la facture ne peut plus être modifiée. |
| **Comment savoir si une opération a réussi ?** | Le journal d'audit affiche le résultat de chaque opération avec un ID de corrélation. |
| **Que signifie "SIMULATION" ?** | L'application est en mode ReadOnly. Les opérations sont simulées sans écriture réelle. |
| **Combien de temps les logs sont-ils conservés ?** | Les logs sont conservés dans `logs/bmpharma-.log` avec rotation quotidienne. |
| **Puis-je utiliser BM Pharma sans CHIFA-OFFICINE ?** | Oui. En mode ReadOnly, toutes les fonctionnalités de préparation sont disponibles sans CHIFA. |
| **Que faire en cas d'erreur "Rollback requis" ?** | Contacter le support technique. Une intervention manuelle sur la base est nécessaire. |

---

## 7. Bonnes Pratiques

### Sécurité
- **Toujours** vérifier le mode d'intégration avant d'effectuer des opérations
- Ne jamais partager le mot de passe PostgreSQL `appsettings.json`
- Éjecter le token PKCS#11 en fin de journée

### Performance
- Préparer les factures par lots plutôt qu'une par une
- Utiliser le bouton **Actualiser** plutôt que d'attendre l'auto-refresh si un résultat est attendu
- Fermer les vues inutilisées pour libérer des ressources

### Traçabilité
- Consulter le journal d'audit quotidiennement
- Noter les ID de corrélation des opérations importantes
- Signaler toute anomalie au support technique avec l'ID de corrélation

### Sauvegarde
- La base PostgreSQL CHIFA_OFFICINE est sauvegardée par le DBA
- Les données locales BM Pharma sont sauvegardées automatiquement dans le dossier `%APPDATA%/BMPharma/`

---

## 8. Contacts Support

| Rôle | Contact |
|------|---------|
| Support technique BM Pharma | support@bmpharma.com |
| Administrateur système | admin@pharmacie.com |
| Éditeur CHIFA-OFFICINE | support@chifa.dz |
