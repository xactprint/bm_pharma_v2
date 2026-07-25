# BM-PHASE-003 — ERROR MATRIX

## Error Categories

### 1. Validation Errors

| Error Code | Field | Scenario | User Message | Rollback | Audit |
|-----------|-------|----------|-------------|----------|-------|
| NUM_FACT_TOO_LONG | num_fact | > 8 chars | "Numéro de facture trop long (max 8)" | N/A | Yes |
| NUM_FACT_EMPTY | num_fact | Empty | "Numéro de facture requis" | N/A | Yes |
| NUM_ASSURE_EMPTY | num_assure | Empty | "Numéro d'assuré requis" | N/A | Yes |
| NUM_ASSURE_TOO_LONG | num_assure | > 12 chars | "Numéro d'assuré trop long (max 12)" | N/A | Yes |
| CODE_CENTRE_TOO_LONG | code_centre | > 5 digits | "Code centre trop long" | N/A | Yes |
| NO_LINES | lines | Empty list | "La facture doit contenir au moins une ligne" | N/A | Yes |
| NUM_ENR_TOO_LONG | num_enr | > 5 chars | "Numéro d'enregistrement trop long" | N/A | Yes |
| QTE_INVALID | qte | ≤ 0 | "Quantité invalide" | N/A | Yes |
| QTE_TOO_HIGH | qte | > 999 | "Quantité maximale dépassée (999)" | N/A | Yes |
| PPA_INVALID | ppa | ≤ 0 | "Prix unitaire invalide" | N/A | Yes |
| NUM_BORD_EMPTY | num_bord | Empty | "Numéro de bordereau requis" | N/A | Yes |
| NUM_BORD_TOO_LONG | num_bord | > 6 chars | "Numéro de bordereau trop long" | N/A | Yes |
| CODE_CENTRE_EMPTY | code_centre | Empty | "Code centre requis" | N/A | Yes |
| NO_INVOICES | invoices | Empty list | "Le bordereau doit contenir au moins une facture" | N/A | Yes |

### 2. Integration Errors

| Error | Scenario | User Message | Rollback | Audit |
|-------|----------|-------------|----------|-------|
| WRITE_BLOCKED | ReadOnly mode | "Mode ReadOnly — changement requis" | N/A | Yes |
| CHIFA_OFFLINE | PostgreSQL unavailable | "CHIFA indisponible" | Yes | Yes |
| INVOICE_DUPLICATE | num_fact exists | "Facture déjà existante dans CHIFA" | N/A | Yes |
| BORDEREAU_DUPLICATE | num_bord exists | "Bordereau déjà existant" | N/A | Yes |
| INVOICE_NOT_FOUND | Bordereau references missing invoice | "Facture non trouvée dans CHIFA" | Yes | Yes |
| SIGNING_BLOCKED | No token | "Carte professionnelle requise — CHIFA-OFFICINE" | N/A | Yes |
| CLOSURE_BLOCKED | Not signed | "Bordereau doit être signé d'abord" | N/A | Yes |

### 3. System Errors

| Error | Scenario | User Message | Rollback | Audit |
|-------|----------|-------------|----------|-------|
| SQLITE_UNAVAILABLE | SQLite down | "Base locale indisponible" | Yes | Yes |
| POSTGRESQL_UNAVAILABLE | PostgreSQL down | "Base CHIFA indisponible" | Yes | Yes |
| TRANSACTION_FAILED | DB transaction error | "Erreur de transaction — annulation" | Yes | Yes |

### Recovery Actions

| Error Type | Automatic Recovery | User Action Required |
|-----------|-------------------|---------------------|
| Validation | None | Fix data and retry |
| WRITE_BLOCKED | None | Change integration mode |
| CHIFA_OFFLINE | Retry | Wait for CHIFA availability |
| INVOICE_DUPLICATE | None | Check existing invoices |
| SIGNING_BLOCKED | None | Switch to CHIFA-OFFICINE |
| TRANSACTION_FAILED | Rollback | Retry operation |
