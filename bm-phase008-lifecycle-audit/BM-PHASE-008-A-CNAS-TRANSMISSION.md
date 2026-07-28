# BM-PHASE-008-A — TRANSMISSION CNAS

**Document:** 008-A-CNAS-TRANSMISSION
**Date:** 2026-07-28

---

## Résumé

La transmission CNAS est le **dernier état du workflow**. Elle consiste à uploader
les fichiers `.P7M` et `.xml` générés par `cloturerbord()` vers un serveur FTP CNAS.

**BM Pharma ne peut PAS transmettre sans signature et clôture préalables.**

---

## Serveur CNAS

| Paramètre | Valeur |
|-----------|--------|
| Hôte | `41.111.149.250` |
| Port | `21` (FTP standard) |
| Mode | Passif (PASV) |
| Intervalle de scrutation | ~15 minutes |
| Authentification | Credentials chiffrés dans `APICNAS.config.xxx` |
| Taille du fichier config | 57,448 bytes |

## Flux de transmission

```mermaid
flowchart TD
    CLOTURE[cloturerbord() OK] --> FILES[Fichiers .P7M + .xml<br/>dans cnas_signed/]
    FILES --> CONNECT[Connexion FTP<br/>41.111.149.250:21]
    CONNECT --> AUTH[Authentification<br/>APICNAS.config.xxx]
    AUTH --> UPLOAD[Upload fichiers<br/>via Controles.FTP.upload()]
    UPLOAD --> DEPOT[Dépôt CNAS]
    DEPOT --> UPDATE[UPDATE bordereau<br/>SET date_depot_ftp = NOW()]
    UPDATE --> RECU[Impression Accusé<br/>Depot_Bord.rdlc]
```

---

## Types de fichiers transmis

| Extension | Contenu | Généré par |
|-----------|---------|-----------|
| `.P7M` | Signature PKCS#7 encapsulée | `cloturerbord()` |
| `.xml` | Facture formatée XML CNAS | `cloturerbord()` (overload 2) |

### Format des noms de fichiers

```
{code_centre}_{code_ps}_{num_bord}_{num_fact}_{num_assure}.P7M
{code_centre}_{code_ps}_{num_bord}_{num_fact}_{num_assure}.xml
```

### DTDs utilisées

| Centre | DTD | Caisse |
|--------|-----|--------|
| Code `9xxxxx` | `DOCP7.dtd` | CASNOS |
| Code autre | `FACTP7.dtd` | CNAS |

Code centre de la pharmacie : **11600** → DTD **FACTP7.dtd** (CNAS).

---

## Services web additionnels

CHIFA-OFFICINE utilise Axis2/C sur le port **6060** pour des services SOAP :

| Service | URL probable | Fonction |
|---------|-------------|----------|
| `ServiceHistConsom` | `http://41.111.149.250:6060/axis2/services/ServiceHistConsom` | Historique de consommation |
| `ServiceCMPD` | `http://41.111.149.250:6060/axis2/services/ServiceCMPD` | CMPD |

Ces services sont utilisés **en plus** du FTP pour des données non-facturières
(listes noires, tarifs, médicaments).

---

## Ce que BM Pharma peut faire

### Impossible
- Transmettre sans clôture préalable
- Se connecter au FTP CNAS (credentials chiffrés, inconnus)
- Produire les fichiers `.P7M` valides (nécessite signature SAM)

### Possible
- Détecter `bordereau.date_depot_ftp != '1900-01-01'` → transmis
- Vérifier présence des fichiers dans `cnas_signed/`
- Marquer l'état `Transmitted` après détection CNAS

---

## Configuration FTP

```csharp
// Extraite de Controles.FTP (obfusqué, inféré)
class FTP {
    static string getFileDate(string path);
    static string[] getFileList(string path);
    static bool upload(string localPath, string remotePath);
}

class TrustAllCertificatesPolicy { ... }
```

**Détail :** La classe `TrustAllCertificatesPolicy` indique que CHIFA-OFFICINE
**ignore les erreurs SSL/TLS** pour la connexion FTP. Cela suggère que les
certificats CNAS ne sont pas valides ou auto-signés.

---

## Téléchargements depuis le FTP CNAS

L'application scrute le FTP CNAS toutes les ~15 minutes pour :

| Fichier | Contenu | Destination |
|---------|---------|-------------|
| `Medicaments.txt` | Liste des 6,529 médicaments | `medicament` table |
| `Liste_noire.txt` | Liste noire des assurés | `liste_noire` table |
| `Tarifs.txt` | Mise à jour tarifs | `tarif` table |
| `Version.txt` | Mise à jour version | `parametre.version` |
| `CM_*.txt` | Décisions CM | `cm` table |
| `Mutuelle_*.txt` | Données mutuelles | `mutualiste_radie` table |

---

## Checks de sécurité pour BM Pharma

```
Transmission CNAS requiert :
  ✅  Bordereau existe dans la base
  ✅  Facture(s) liée(s) au bordereau (num_bord)
  🔴 Signature valide (PKCS#7 SAM) → CHIFA-OFFICINE
  🔴 Clôture OK (cloturerbord retourne > 0) → CHIFA-OFFICINE
  🔴 Fichiers .P7M générés dans cnas_signed/ → CHIFA-OFFICINE
  🔴 Credentials FTP CNAS (APICNAS.config) → CHIFA-OFFICINE
  🔴 Upload FTP vers 41.111.149.250 → CHIFA-OFFICINE
```

**La transmission CNAS est entièrement déléguée à CHIFA-OFFICINE.**
BM Pharma s'arrête au stade "Bordereau prêt pour clôture".
