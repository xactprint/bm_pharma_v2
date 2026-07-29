# BM-PHASE-010 — INVOICE CONTRACT

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 007 (real write TST002), Phase 004.10 (entity correction)
**Previous Version:** BM_PHARMA_CHIFA_INVOICE_CONTRACT.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial contract based on BM-SPEC-028-029 |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with real PG write (TST002, Phase 007) |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| Write order | detail_fact then facture | facture first, then detail_fact (FK constraint) | Phase 007-G |
| DateTime Kind | DateTime.Now | DateTime.SpecifyKind(..., Unspecified) | Phase 007-G |
| Column types | varchar for xml columns | xml columns need HasColumnType("xml") | Phase 007-G |
| date_fact type | timestamp | timestamp without time zone | Phase 007-G |
| date_soin type | timestamp | date | Phase 007-G |
| Compteurs | FOR UPDATE protocol | UPDATE...RETURNING (atomic) | Phase 006 |

---

## Purpose

Defines the validation rules BM Pharma applies BEFORE writing a facture to CHIFA PostgreSQL, reconciled with real write validation performed in Phase 007.

---

## Pre-Write Validation Rules

All rules confirmed working in Phase 007-G (TST002):

### Rule 1: Invoice Number Length
- **Column:** facture.num_fact
- **Type:** varchar(8) NOT NULL PK
- **Max:** 8 characters
- **Action:** REJECT if exceeded
- **Status:** CONFIRMED

### Rule 2: mont_maj_fae NOT NULL
- **Column:** facture.mont_maj_fae
- **Type:** numeric(4,2) NOT NULL DEFAULT 0
- **Required:** Must be explicitly set to 0
- **Action:** Auto-set to 0 if NULL
- **Status:** CONFIRMED

### Rule 3: mont_maj NOT NULL
- **Column:** facture.mont_maj
- **Type:** numeric(11,2) NOT NULL DEFAULT 0
- **Required:** Must be explicitly set to 0
- **Action:** Auto-set to 0 if NULL
- **Status:** CONFIRMED

### Rule 4: type_maj NOT NULL
- **Column:** facture.type_maj
- **Type:** integer NOT NULL DEFAULT 0
- **Required:** Must be 0
- **Action:** Auto-set to 0
- **Status:** CONFIRMED

### Rule 5: Detail Invoice FK
- **Column:** detail_fact.num_fact
- **Required:** Must match an existing facture.num_fact
- **Action:** INSERT facture FIRST, then detail_fact (two SaveChangesAsync calls)
- **Bypass:** FK constraint enforced by PostgreSQL (error 23503)
- **Status:** CONFIRMED

### Rule 6: Detail Sequential Numbering
- **Column:** detail_fact.num_enr
- **Type:** varchar(5) NOT NULL (PK2)
- **Format:** Zero-padded sequential ('00001', '00002', ...)
- **Status:** CONFIRMED

### Rule 7: Amount Coherence
- detail_fact.mont = qte * ppa
- facture.mont_fact = SUM(detail_fact.mont)
- facture.mont_off = SUM(detail_fact.ppa * qte)
- facture.mont_as = mont_off * taux (70%)
- **Action:** REJECT if mismatch
- **Status:** CONFIRMED

### Rule 8: Quantity Limit
- **Column:** detail_fact.qte
- **Type:** numeric(3,0) NOT NULL
- **Max:** 999
- **Action:** REJECT if exceeded
- **Status:** CONFIRMED

### Rule 9: DateTime Kind
- **Column:** facture.date_fact (timestamp without time zone)
- **Requirement:** DateTimeKind MUST be Unspecified (not Local, not Utc)
- **Action:** `DateTime.SpecifyKind(value, DateTimeKind.Unspecified)`
- **Bug fixed:** Phase 007-G
- **Status:** CONFIRMED

### Rule 10: Column Type Mapping
| Entity Property | PG Type | EF Core Configuration | Status |
|----------------|---------|----------------------|--------|
| string? Signature | xml | HasColumnType("xml") | CONFIRMED |
| string? FactXml | xml | HasColumnType("xml") | CONFIRMED |
| DateTime? DateFact | timestamp without time zone | HasColumnType("timestamp without time zone") | CONFIRMED |
| DateTime? DateSoin | date | HasColumnType("date") | CONFIRMED |

---

## Write Protocol

### Correct Write Order (Phase 007-G confirmed)

```
1. BeginTransactionAsync(ReadCommitted)
2. context.Factures.Add(facture)          → facture first
3. context.SaveChangesAsync()             → INSERT facture
4. context.DetailFacts.Add(detail)        → detail_fact second
5. context.SaveChangesAsync()             → INSERT detail_fact
6. transaction.CommitAsync()
```

**Why two SaveChangesAsync calls:** EF Core must insert the parent facture first because detail_fact has a FK constraint referencing facture.num_fact. A single SaveChangesAsync may insert detail_fact first (alphabetical order), causing FK violation.

### Write Data (TST002 — Proven Working)

**facture:**
```csharp
new ChifaFacture
{
    NumFact = "TST002",
    DateFact = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
    Etat = "0",
    MontFact = 120.00m,
    MontAs = 84.00m,
    MontOff = 120.00m,
    NumAssure = "TST99999",
    CodeCentre = "11600",
    DateSoin = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified),
    TypeMaj = 0,
    MontMajFae = 0m,
    MontMaj = 0m,
    Taux = 3m,
    RangAd = 1,
    Tp = 1,
    CodeAffect = "01",
    Conv = 1,
    TypeConsult = "01",
    Prescripteur = "BM PHARMA",
    Risque = 0,
    StatutFact = 1,
    Verifcms = 0,
    TypeSignature = 0,
    VerifFact = 0,
    Version = "2.0.0"
}
```

**detail_fact:**
```csharp
new ChifaDetailFact
{
    NumFact = "TST002",
    NumEnr = "00010",
    Ppa = 60.00m,
    Qte = 2,
    Mont = 120.00m,
    MontAs = 84.00m,
    MontPharm = 36.00m,
    NumEnrPrescrit = "00010",
    NumLot = "LOT002",
    DureeTrait = 30,
    TarifRef = 60.00m,
    Posologie = "1/day"
}
```

---

## Reimbursement Calculation

| Element | Rate | Formula | Verified |
|---------|------|---------|----------|
| Montant officine (mont_off) | 100% | qte * ppa | CONFIRMED |
| Part assurance (mont_as) | 70% | mont_off * 0.70 | CONFIRMED |
| Reste pharmacie (mont_pharm) | 30% | mont - mont_as | CONFIRMED |
| Taux | 3 (70%) | — | CONFIRMED |

---

## Error Response

```json
{
  "Success": false,
  "Errors": [
    {"Code": "NUM_FACT_TOO_LONG", "Field": "num_fact", "Message": "Max 8 characters"},
    {"Code": "MONT_MAJ_FAE_NULL", "Field": "mont_maj_fae", "Message": "Must not be NULL"},
    {"Code": "FK_VIOLATION", "Field": "num_fact", "Message": "FK constraint error 23503"}
  ]
}
```

---

## Validation Pipeline (Proven)

```
Request → ValidateNumFactLength → ValidateNullSafety →
ValidateAmounts → ValidateDetails → WriteTransaction (facture first, then detail_fact) → ReturnResult
```

## Audit Trail

Every invoice write is logged with:

| Field | Detail | Status |
|-------|--------|--------|
| Operation | CREATE_INVOICE | CONFIRMED |
| Entity | facture | CONFIRMED |
| Key | NumFact value | CONFIRMED |
| Lines | Number of detail_fact rows | CONFIRMED |
| Total | mont_fact value | CONFIRMED |
| Duration | ms | CONFIRMED |
| CorrelationId | GUID | CONFIRMED |
| User | system (RBAC pending) | PARTIAL |
