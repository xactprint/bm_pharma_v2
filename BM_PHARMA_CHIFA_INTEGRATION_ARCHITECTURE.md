# BM PHARMA - CHIFA INTEGRATION ARCHITECTURE

**Version**: 1.0
**Date**: 2026-07-25

## Architectural Principle

> BM Pharma automates everything that can be automated.
> CHIFA-OFFICINE remains the authority for proprietary operations requiring the professional token.

## Integration Model

```
BM Pharma (Domain + Application)
    ↓ validates & orchestrates
CHIFA Integration Layer (CHIFA project)
    ↓ writes via EF Core
CHIFA PostgreSQL Database
    ↓ reads & signs
CHIFA-OFFICINE (proprietary)
    ↓ signs & transmits
CNAS (national healthcare)
```

## Layer Responsibilities

| Layer | Responsibility | Read | Write |
|-------|---------------|------|-------|
| Domain | Business entities, rules | Own DB | Own DB |
| Application | Orchestration, validation | Own DB | Own DB |
| CHIFA | PostgreSQL bridge | CHIFA tables | CHIFA tables |
| Persistence.SQLite | BM Pharma local storage | SQLite | SQLite |
| Persistence.PostgreSQL | CHIFA PostgreSQL context | PG | PG |
| UI (WPF) | User interaction | All | Via services |

## Service Boundaries

### IChifaIntegrationService
- Health check: can we reach PostgreSQL?
- Mode check: ReadOnly / Test / Production

### IChifaInvoiceService
- Validate invoice before write
- Check column length limits
- Check null constraints (mont_maj_fae, mont_maj)
- Insert facture + detail_fact transactionally
- Return structured result

### IChifaBordereauService
- Get next bordereau number (atomic)
- Create bordereau in PostgreSQL
- Link invoices to bordereau
- Increment counter
- Full rollback on failure

### IChifaTokenService
- Detect if PKCS#11 token is present
- NEVER attempt signing from BM Pharma
- Guide user to CHIFA for signing

## Data Flow: Invoice Preparation

1. BM Pharma creates invoice in SQLite
2. User triggers "Prepare for CHIFA"
3. BM Pharma validates CHIFA constraints
4. BM Pharma writes to CHIFA PostgreSQL (facture + detail_fact)
5. Status: "DatabaseCreated"
6. User can then open CHIFA for signing
7. CHIFA signs → status: "Signed"
8. CHIFA closes bordereau → status: "Closed"
9. CHIFA transmits to CNAS → status: "Transmitted"

## State Distinction

BM Pharma MUST distinguish:
- **DatabaseCreated**: Row exists in CHIFA PostgreSQL
- **CHIFAVisible**: Visible in CHIFA-OFFICINE UI
- **ReadyForSigning**: Ready for PKCS#11 operation
- **Signed**: Digital signature applied
- **Closed**: Bordereau closed in CHIFA
- **Transmitted**: Sent to CNAS
