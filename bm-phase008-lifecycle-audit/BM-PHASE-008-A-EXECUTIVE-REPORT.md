# BM-PHASE-008-A — RAPPORT EXÉCUTIF

**Date:** 2026-07-28
**Phase:** 008-A — Audit du cycle de vie CHIFA et préparation du test de visibilité

---

## Résumé

Phase 008-A a audité le code BM Pharma **et** le vrai CHIFA-OFFICINE pour reconstruire le workflow
complet d'une facture CHIFA : de la vente à la transmission CNAS.

---

## Découvertes majeures

### 1. Écriture EF Core — ✅ PROUVÉE

La chaîne `BM Pharma → EF Core → Npgsql → PostgreSQL 9.3.4` fonctionne. TST002 a été
créée et vérifiée dans Phase 007-G.

### 2. Workflow CHIFA — 3 états seulement sont prouvés

| État | Preuve |
|------|--------|
| Draft → WrittenToChifa | ✅ Phase 007-G — facture créée dans CHIFA_OFFICINE |
| VisibleInChifa | 🟡 DÉDUIT — Phase 005-E consultation a montré TST001 |
| BordereauAssigned | ❓ INCONNU — jamais testé avec un vrai bordereau |

Tous les autres états (Signed, Closed, Transmitted) sont **simulés** ou **délégués** à CHIFA-OFFICINE.

### 3. Visibilité des bordereaux — CAUSE PROBABLE IDENTIFIÉE

**Problème :** Un bordereau créé directement en PostgreSQL n'apparaît PAS dans
« Visualiser Bordereau ».

**Cause probable :**
- `FBordereau` charge les bordereaux avec une `DataTable` nommée `detail_bord`
- Cette `DataTable` fait une **jointure SQL complexe** entre `facture` et `detail_fact`
- Le DataSet ne contient que les bordereaux qui ont **au moins une facture liée**
- Mais une condition WHERE supplémentaire existe : `detail_fact` doit avoir `qte > 0`
  ou un autre filtre non trivial

**Théorie (à confirmer) :**
1. Un bordereau vide (0 factures liées) n'affiche aucune ligne → invisible
2. `detail_bord` n'est pas une table — c'est un **DataTable en mémoire**
3. La requête sous-jacente est probablement :
   ```sql
   SELECT f.*, d.* FROM facture f
   JOIN detail_fact d ON f.num_fact = d.num_fact
   WHERE f.num_bord = '<numbord>'
   ```
4. Si `facture.num_bord` est NULL ou le bordereau n'existe pas, la jointure
   retourne 0 lignes → invisible

### 4. Signature — 🔴 IMPOSSIBLE SANS CHIFA-OFFICINE

La signature nécessite :
- Token physique : **Identiv uTrust 3512 SAM slot**
- Carte professionnelle insérée
- PIN hardware (pinpad)
- DLL native : `p7sign.dll` + `opensc-pkcs11.dll`
- Stack complète : `CHIFA.exe → CGAPXUDN.dll → cgapxutl.dll → p7sign.dll → pkcs11.dll → winscard.dll`

BM Pharma **ne peut pas** signer sans passer par CHIFA-OFFICINE.

### 5. Clôture (`cloturerbord`) — 🔴 IMPOSSIBLE SANS SIGNATURE

`cloturerbord()` écrit les fichiers `.P7M` uniquement pour les factures qui ont
`facture.signature <> ''`. Sans signature, `num` reste à 0 et la fonction
retourne 0 → **aucun fichier généré, cloture échoue**.

### 6. Transmission CNAS — 🔴 IMPOSSIBLE SANS CLÔTURE

Même avec clôture, la transmission nécessite :
- FTP vers `41.111.149.250` (serveur CNAS)
- Credentials chiffrés dans `APICNAS.config.xxx`
- Fichiers `.P7M` et `.xml` formatés selon les DTDs CNAS/CASNOS

---

## Matrice de preuves

| Étape | État | Preuve |
|-------|------|--------|
| Création facture | ✅ PROUVÉE | Phase 007-G |
| Visibilité Consultation Facture | 🟡 DÉDUITE | Phase 005-E |
| Création bordereau | ❓ INCONNUE | Jamais testée |
| Association facture ↔ bordereau | ❓ INCONNUE | Jamais testée |
| Visibilité Visualiser Bordereau | ❓ INCONNUE | Cause probable identifiée |
| Signature token | 🔴 NON TESTABLE | Nécessite CHIFA-OFFICINE |
| Clôture | 🔴 NON TESTABLE | Nécessite signature |
| Transmission CNAS | 🔴 NON TESTABLE | Nécessite clôture |

---

## Recommandations

### Priorité haute — Phase 008-B

1. Tester la création d'un bordereau + association d'une facture
2. Vérifier la visibilité dans « Visualiser Bordereau »
3. Valider ou infirmer la théorie de la jointure `detail_bord`

### Priorité basse

4. `Ts` = `false` toujours — corriger le bug
5. `MontMut` = `0` au lieu de NULL — aligner avec CHIFA
6. `nat_remb` / `date_synchro` — valeurs attendues

### Bloqué — Dépend de CHIFA-OFFICINE

7. Signature, clôture, transmission CNAS

---

## ⛔ STOP — Phase 008-A terminée. En attente d'approbation pour 008-B.
