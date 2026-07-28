# BM-PHASE-008-A — FLUX DE SIGNATURE

**Document:** 008-A-SIGNING-FLOW
**Date:** 2026-07-28

---

## Résumé

La signature dans CHIFA-OFFICINE est un processus **hardware-bound** qui nécessite
un token professionnel physique (carte à puce + lecteur Identiv uTrust 3512).

**BM Pharma ne peut PAS signer. Cette étape est déléguée à CHIFA-OFFICINE.**

---

## Architecture de la signature

```
┌─────────────────────────────────────────────────────┐
│ CHIFA_OFFICINE.exe  (.NET 4.0, ConfuserEx-obfusqué)   │
├─────────────────────────────────────────────────────┤
│                                                       │
│  CGAPXUDN.dll  (v0.17.0.0, C++/CLI bridge)          │
│    ↓                                                  │
│  cgapxutl.dll  (v0.17.0.0, native Mühlbauer API)    │
│    ↓                                                  │
│  cnassam.dll   (v1.3.6.0, SAM token API)            │
│  cnaschifa.dll (v1.5.8.0, CHIFA card API)           │
│  cfchifa.dll   (v1.3.9.0, Cryptoflex card API)      │
│    ↓                                                  │
│  p7sign.dll    (v1.2.1.0, PKCS#7 signing engine)    │
│    ↓                                                  │
│  pkcs11.dll    (v0.4.13.0, Mühlbauer PKCS#11)       │
│    ↓                                                  │
│  opensc-pkcs11.dll  (v0.23.0.0, OpenSC PKCS#11)     │
│    ↓                                                  │
│  pcsc.dll      (v1.0.0.0, PC/SC wrapper)            │
│    ↓                                                  │
│  winscard.dll  (Windows PC/SC subsystem)             │
│    ↓                                                  │
│  USB → Identiv uTrust 3512 SAM slot Token            │
└─────────────────────────────────────────────────────┘
```

---

## Matériel requis

| Composant | Rôle |
|-----------|------|
| Identiv uTrust 3512 SAM slot | Lecteur de carte à puce |
| Carte Professionnelle (SAM) | Contient le certificat + clé privée |
| Lecteur pinpad intégré | Saisie du PIN sans interception logicielle |

---

## Étapes de la signature

### 1. Ouverture de session SAM
```
SAM_OpenSession("Identiv uTrust 3512 SAM slot Token 0")
→ samSession prête
```

### 2. Authentification
```
FDialogToken.Show()  // Boîte de dialogue PIN
→ DepresenterPINPS() // Validation PIN par le token hardware
→ GetCardInfo()       // Lecture certificat + métadonnées
```

### 3. Construction des données à signer
La facture est convertie en structure native C :
```c
struct CNAS_PARAMS_FACTURE {
    int32 CodeActe;
    int32 RisqueActe;
    int32 QuantiteActe;
    float64 Montant;
    void* Numero;     // ANSI string
    void* Date;       // ANSI string
};
```

### 4. Signature PKCS#7
```
SignerEtHistoriserFacture(CNAS_PARAMS_FACTURE)
→ Hash SHA-256 du contenu
→ Signature RSA avec clé privée SAM (2048-bit)
→ Encapsulation PKCS#7
→ byte[] signedData
```

### 5. Récupération du document signé
```
RecupererDocumentSigne()
→ byte[] → XML structuré :
   <InfoSignature>
     <CodeAD>...</CodeAD>       // Code AD (pharmacien)
     <Acte>...</Acte>           // Acte signé
     <Sensibilite>...</Sensibilite>
     <FACM>...</FACM>           // Facture mise en forme
     <FACX>...</FACX>           // Facture XML
     <SignatureAS>...</SignatureAS>
     <CondensatFACX>...</CondensatFACX>
   </InfoSignature>
```

### 6. Stockage dans PostgreSQL
```
UPDATE facture SET
  signature = '<InfoSignature>...</InfoSignature>',
  fact_xml  = '<facture_xml_data>'
WHERE num_fact = '<N>';
```

Le champ `signature` est de type **XML** en PostgreSQL.
Le champ `fact_xml` contient la facture formatée selon DTD CNAS.

---

## Configuration du token

Fichier de configuration (probablement `chifa.ini` ou équivalent) :
```ini
[ProfessionalToken]
CAD=Identiv uTrust 3512 SAM slot Token 0
CODE=E096955F5305D66031A88B3A610A97E8   # Encrypted

[PKCS7]
DYNAMIC_PATH=.\\pkcs11.dll
MODULE_PATH=.\\opensc-pkcs11.dll
LOG=0
```

---

## Tables liées à la signature

```sql
CREATE TABLE certificat_token (
    num_serie      BIGINT PRIMARY KEY,
    date_certificat DATE
);

CREATE TABLE token (
    ip           VARCHAR(15),
    code_affect  INTEGER
);
```

---

## Ce que BM Pharma peut faire

### Impossible
- Signer une facture (pas de token physique)
- Produire `facture.signature` valide (PKCS#7 avec certificat SAM)
- Produire `facture.fact_xml` valide (format XML CNAS)

### Possible
- Mettre `type_signature = "0"` (non signé)
- Laisser `signature = NULL`
- Laisser `fact_xml = NULL`
- Ouvrir CHIFA-OFFICINE pour que l'utilisateur signe manuellement
- Détecter l'état signé via `SELECT ... WHERE signature IS NOT NULL`

---

## Impact sur le workflow

```
┌──────────────────────────────────────────────────────┐
│  BM Pharma écrit facture dans PostgreSQL              │
│  → facture.signature = NULL                          │
│  → facture.type_signature = "0"                      │
│  ↓                                                   │
│  Pharmacien ouvre CHIFA-OFFICINE                     │
│  → Consultation Facture → sélectionne TST002         │
│  → Insère sa carte professionnelle                   │
│  → Saisit son PIN                                    │
│  → CHIFA signe → UPDATE facture SET signature = X    │
│  → BM Pharma détecte signature ≠ NULL                │
│  → Workflow peut continuer                           │
└──────────────────────────────────────────────────────┘
```
