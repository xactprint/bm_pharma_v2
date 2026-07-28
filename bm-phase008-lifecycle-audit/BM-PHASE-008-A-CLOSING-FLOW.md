# BM-PHASE-008-A — FLUX DE CLÔTURE

**Document:** 008-A-CLOSING-FLOW
**Date:** 2026-07-28

---

## Résumé

La clôture est exécutée par une **fonction PostgreSQL** `cloturerbord()`
qui lit les factures signées d'un bordereau et génère des fichiers `.P7M`.
**BM Pharma ne peut PAS exécuter `cloturerbord` sans signature valide.**

---

## La fonction `cloturerbord` (version complète)

```plpgsql
CREATE OR REPLACE FUNCTION public.cloturerbord(
    numbord character varying,
    chemin_cnas_signed text)
RETURNS integer
LANGUAGE plpgsql
AS $$
DECLARE
    factures CURSOR FOR
        SELECT num_fact, code_centre, num_bord, num_assure, signature, fact_xml
        FROM facture
        WHERE num_bord = '' || numbord || '';
    facture RECORD;
    num integer DEFAULT 0;
    result TEXT DEFAULT '';
    signatures TEXT DEFAULT '';
    codeps VARCHAR(10);
    versionn VARCHAR(12);
    dateln VARCHAR(10);
    datemedic VARCHAR(10);
    datenote VARCHAR(10);
    codecentre VARCHAR(5);
    bord_entete VARCHAR(255);
    command VARCHAR(100);
BEGIN
    DELETE FROM temp;

    -- Lire les paramètres du centre
    SELECT code_ps, version,
           to_char(date_medicament, 'dd/mm/yyyy'),
           to_char(date_note, 'dd/mm/yyyy'),
           to_char(date_liste_noire, 'dd/mm/yyyy')
        INTO codeps, versionn, datemedic, datenote, dateln
    FROM parametre;

    SELECT code_centre INTO codecentre
    FROM bordereau WHERE num_bord = '' || numbord || '';

    -- Créer le répertoire de sortie
    EXECUTE format(
        'COPY temp(data) FROM program %L',
        'PG_Program CREATE_DIR "' || $2 || '\' || codecentre || '\'
        || codecentre || '_' || codeps || '_' || numbord || '\"'
    );
    SELECT data INTO result FROM temp;

    IF result = '1' THEN
        OPEN factures;
        LOOP
            FETCH factures INTO facture;
            EXIT WHEN NOT FOUND;

            -- Écrire le fichier .P7M pour chaque facture signée
            IF facture.signature::text <> '' THEN
                EXECUTE format(
                    'COPY temp(data) FROM program %L',
                    'PG_Program WRITE_FILE "' || $2 || '\' || codecentre || '\'
                    || codecentre || '_' || codeps || '_' || numbord || '\'
                    || codecentre || '_' || codeps || '_' || numbord || '_'
                    || facture.num_fact || '_' || facture.num_assure
                    || '.P7M" "' || facture.signature || '"'
                );
                num = num + 1;
            END IF;
        END LOOP;
        CLOSE factures;
    ELSE
        num = -100;  -- Échec création répertoire
    END IF;

    -- Appeler PG_Program CLOTURER
    IF (num > 0) THEN
        EXECUTE format(
            'COPY temp(data) FROM program %L',
            'PG_Program CLOTURER "' || $2 || '\' || codecentre || '\'
            || codecentre || '_' || codeps || '_' || numbord || '\" "'
            || codecentre || '_' || codeps || '_' || numbord || '" "'
            || bord_entete || '"'
        );
    END IF;

    RETURN num;
END;
$$;
```

---

## Algorithme de la clôture

```
1. DELETE FROM temp            (vide la table temporaire)
2. SELECT paramètres centre    (code_ps, version, dates)
3. SELECT code_centre          (depuis bordereau)
4. PG_Program CREATE_DIR       (crée dossier cnas_signed/centrea/...)
5. Pour chaque facture du bordereau :
   SI signature ≠ '' :
     PG_Program WRITE_FILE     (écrit .P7M)
     num++
6. SI num > 0 :
     PG_Program CLOTURER       (finalise)
7. RETOURNE num                (nombre de fichiers signés)
```

---

## Conditions de succès

| Condition | Requis | Statut BM Pharma |
|-----------|--------|-----------------|
| Bordereau existe | `bordereau.num_bord = X` | ✅ Possible |
| Centre valide | `code_centre` non NULL | ✅ "11600" |
| **Au moins 1 facture signée** | `facture.signature <> ''` | 🔴 IMPOSSIBLE |
| PG_Program.exe accessible | Dans `data/` | ✅ Existe |
| Dossier cnas_signed accessible | Chemin local | ✅ Dépend du poste |

### Codes de retour

| Valeur | Signification |
|--------|--------------|
| `> 0` | Succès — N fichiers .P7M écrits |
| `0` | **Aucune facture signée** — échec silencieux |
| `-100` | Échec création répertoire |
| Exception | Erreur PG_Program |

---

## Overload 2 : avec XML bordereau

La deuxième signature de `cloturerbord` prend un `xml_bord TEXT` supplémentaire.

Elle génère en plus :
- Un en-tête XML du bordereau complet
- Des fichiers `.xml` pour chaque facture (en plus des `.P7M`)
- Le DTD utilisé dépend du code_centre :
  - `9xxxxx` → `DOCP7.dtd` (CASNOS)
  - Autres → `FACTP7.dtd` (CNAS)

---

## PG_Program.exe

Emplacement : `CHIFA_OFFICINE_DB/data/PG_Program.exe` (12,800 bytes)

Commandes supportées :
| Commande | Action |
|----------|--------|
| `CREATE_DIR <path>` | Crée un répertoire (et les parents) |
| `WRITE_FILE <path> <content>` | Écrit un fichier avec le contenu donné |
| `CLOTURER <path> <name> <header>` | Finalise la clôture |

Appelé depuis PostgreSQL via :
```sql
COPY temp(data) FROM PROGRAM 'PG_Program CREATE_DIR "...'
```

---

## Noms des fichiers générés

```
<cnas_signed>/
  <code_centre>/
    <code_centre>_<code_ps>_<numbord>/
      <code_centre>_<code_ps>_<numbord>_<num_fact>_<num_assure>.P7M
      <code_centre>_<code_ps>_<numbord>_<num_fact>_<num_assure>.xml  (overload 2)
```

Exemple avec valeurs réelles (TST002) :
```
cnas_signed/
  11600/
    11600_1234567890_TST002/
      11600_1234567890_TST002_TST002_TST99999.P7M
```

---

## Impact BM Pharma

### Bloquant
- `cloturerbord` retourne 0 si signature est vide
- BM Pharma ne peut pas générer de signature valide
- La clôture ne peut PAS être testée sans passer par CHIFA-OFFICINE

### Alternative pour test
- Insérer une signature factice NON NULL dans `facture.signature`
- `cloturerbord` écrirait un fichier .P7M (avec contenu invalide)
- `num > 0` → retourne N au lieu de 0
- **⚠️ Ne pas faire sans approbation — pourrait interférer avec CHIFA**

### Workflow recommandé
```
BM Pharma écrit facture → CHIFA signe → BM Pharma détecte signature
→ BM Pharma appelle cloturerbord → fichiers prêts → CNAS transmission
```
