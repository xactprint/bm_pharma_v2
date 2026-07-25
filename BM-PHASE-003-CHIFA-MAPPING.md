# BM-PHASE-003 — CHIFA MAPPING

## BM Pharma → CHIFA Field Mapping

### Invoice Header (facture)

| CHIFA Field | BM Pharma Source | Conversion | Nullable | Default | Validation |
|-------------|------------------|------------|----------|---------|------------|
| num_fact | Invoice.InvoiceNumber | PadLeft(8,'0'), strip dashes | No | — | Max 8 chars, required |
| num_assure | Customer.InsuranceNumber | Direct | No | — | Max 12 chars, required |
| code_centre | Invoice.ChifaCodeCentre | Direct | No | 11600 | Max 5 digits |
| date_soin | Invoice.InvoiceDate | Direct | No | DateTime.Today | — |
| date_fin_mut | Invoice.InvoiceDate + 1 year | Computed | No | DateSoin + 1yr | — |
| mont_off | Product.PriceDA × Quantity | Sum of lines | No | 0 | Must be > 0 |
| mont_as | mont_off × reimbursement_rate | 70% default | No | 0 | — |
| mont_mut | mont_off - mont_as | Computed | No | 0 | — |
| mont_fact | Sum(Quantity × UnitPriceDA) | Computed | No | 0 | Must be > 0 |
| mont_maj_fae | Same as mont_maj | Computed | No | — | **NEVER NULL** |
| mont_maj | Same as mont_fact | Computed | No | — | **NEVER NULL** |

### Invoice Line (detail_fact)

| CHIFA Field | BM Pharma Source | Conversion | Nullable | Default | Validation |
|-------------|------------------|------------|----------|---------|------------|
| num_enr | Product.Code | PadLeft(5,'0'), strip dashes | No | "00001" | Max 5 chars |
| medic_code | Product.CIPCode | int.TryParse or last 9 digits | No | 1 | Must be positive |
| ppa | InvoiceLine.UnitPriceDA | Direct | No | — | Must be > 0 |
| qte | InvoiceLine.Quantity | Direct | No | — | 1–999 |
| inf_tr | — | Hardcoded | No | 1 | — |
| applic_tr | — | Hardcoded | No | 1 | — |
| medic | — | Hardcoded | No | 1 | — |
| ts | — | Hardcoded | No | 4 | — |
| duree_trait | — | Hardcoded | No | 5 | — |

### Bordereau

| CHIFA Field | BM Pharma Source | Conversion | Nullable | Default | Validation |
|-------------|------------------|------------|----------|---------|------------|
| num_bord | Bordereau.BordereauNumber | PadLeft(6,'0'), strip dashes | No | — | Max 6 chars, required |
| type_bord | Bordereau.CnasType | Direct | No | "BORD_CNAS" | — |
| date_bord | Bordereau.BordereauDate | Direct | No | DateTime.UtcNow | — |
| invoice_numbers | Invoice.ChifaNumFact list | Collected from assigned invoices | No | — | At least 1 required |

### Mapping Classes

| Class | File | Purpose |
|-------|------|---------|
| ChifaInvoiceMapper | Services/ChifaInvoiceMapper.cs | Invoice + Lines → ChifaInvoiceRequest |
| ChifaBordereauMapper | Services/ChifaBordereauMapper.cs | Bordereau → ChifaBordereauRequest |

### Error Handling

| Scenario | Behavior |
|----------|----------|
| num_fact > 8 chars | Truncated to last 8 chars |
| num_assure > 12 chars | Truncated to last 12 chars |
| num_bord > 6 chars | Truncated to last 6 chars |
| num_enr > 5 chars | Truncated to last 5 chars |
| CIP code non-numeric | Falls back to last 9 chars or 1 |
| Product.Code null | Defaults to "00001" |
| Customer.InsuranceNumber null | Empty string → validation fails |
