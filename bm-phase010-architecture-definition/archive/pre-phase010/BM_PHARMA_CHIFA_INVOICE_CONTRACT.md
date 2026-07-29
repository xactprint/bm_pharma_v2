# BM PHARMA - CHIFA INVOICE CONTRACT

**Version**: 1.0
**Date**: 2026-07-25

## Purpose

Defines the validation rules BM Pharma applies BEFORE writing a facture to CHIFA PostgreSQL.

## Pre-Write Validation Rules

### Rule 1: Invoice Number Length
- Column: facture.num_fact
- Max: 8 characters
- Action: REJECT with error if exceeded

### Rule 2: mont_maj_fae NOT NULL
- Column: facture.mont_maj_fae
- Required: Must be explicitly set to 0
- Action: AUTO-SET to 0 if NULL

### Rule 3: mont_maj NOT NULL
- Column: facture.mont_maj
- Required: Must be explicitly set to 0
- Action: AUTO-SET to 0 if NULL

### Rule 4: type_maj NOT NULL
- Column: facture.type_maj
- Required: Must be 0
- Action: AUTO-SET to 0

### Rule 5: Detail Invoice Link
- Column: detail_fact.num_fact
- Required: Must match an existing facture.num_fact
- Action: CREATE in same transaction

### Rule 6: Detail Numbering
- Column: detail_fact.num_enr
- Format: varchar(5), zero-padded sequential
- Example: '00001', '00002', '00003'

### Rule 7: Amount Coherence
- detail_fact.mont = qte * ppa
- facture.mont_fact = SUM(detail_fact.mont)
- Action: REJECT if mismatch

### Rule 8: Quantity Limit
- Column: detail_fact.qte
- Max: 999 (numeric(3,0))
- Action: REJECT if exceeded

## Validation Pipeline

```
Request → ValidateNumFactLength → ValidateNullSafety → 
ValidateAmounts → ValidateDetails → WriteTransaction → ReturnResult
```

## Error Response

```json
{
  "Success": false,
  "Errors": [
    {"Code": "NUM_FACT_TOO_LONG", "Field": "num_fact", "Message": "Max 8 characters"},
    {"Code": "MONT_MAJ_FAE_NULL", "Field": "mont_maj_fae", "Message": "Must not be NULL"}
  ]
}
```
