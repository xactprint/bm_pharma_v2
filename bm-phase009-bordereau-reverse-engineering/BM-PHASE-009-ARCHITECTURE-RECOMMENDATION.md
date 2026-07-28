# BM-PHASE-009 — Architecture Recommendation

**Integration Strategy for BM Pharma v2 ↔ CHIFA-OFFICINE**

> **Date:** 2026-07-28
> **Status:** PROVISIONAL — requires additional investigation for final decision
> **Classification:** Confidential — BM Pharma Internal

---

## Integration Options

### Option A: BM Pharma manages facture + bordereau → CHIFA only signature/clôture/transmission

| Aspect | Description |
|--------|-------------|
| **BM Pharma writes** | `facture`, `detail_fact`, `bordereau` directly to PostgreSQL |
| **CHIFA handles** | Card reading, PKCS#11 signing, `cloturerbord()`, FTP transmission |
| **Risk** | Bordereaux created by BM Pharma may be invisible in CHIFA's Visualiser Bordereau — but CHIFA can still sign and transmit them via its native workflow if the pharmacist navigates to the correct invoice |
| **Proof** | TST003 confirmed SQL writes are valid; invisibility confirmed |

### Option B: BM Pharma prepares data → CHIFA processes bordereau entirely → BM Pharma monitors status

| Aspect | Description |
|--------|-------------|
| **BM Pharma writes** | Data to staging tables (`facture2`, `detail_fact2`) or to main tables |
| **CHIFA handles** | Full lifecycle: charge invoices, create bordereau, sign, close, transmit |
| **BM Pharma monitors** | Reads PostgreSQL status columns (`facture.etat`, `bordereau.etat`, `facture.signature`) |
| **Risk** | Requires pharmacist to manually process each invoice in CHIFA; BM Pharma has less control |

### Option C: BM Pharma replicates the detail_bord filling algorithm

| Aspect | Description |
|--------|-------------|
| **BM Pharma builds** | A custom .NET DataSet or equivalent that mirrors CHIFA's internal `detail_bord` logic |
| **CHIFA handles** | Only hardware operations (token signing, card reading) |
| **Risk** | **HIGH** — the exact SQL and business logic are unknown. Replicating incorrectly could cause data integrity issues or signature rejection by CNAS |

### Option D: Hybrid — BM Pharma creates, CHIFA validates in native UI

| Aspect | Description |
|--------|-------------|
| **BM Pharma writes** | `facture` + `detail_fact` (but NOT `bordereau`) to PostgreSQL |
| **CHIFA handles** | Pharmacist opens each invoice in "Vente Chifa" UI → validates → creates bordereau natively → signs → transmits |
| **BM Pharma monitors** | Reads status from PostgreSQL |
| **Risk** | Additional pharmacist effort to "re-open" each invoice in CHIFA; but this ensures CHIFA's full validation pipeline runs |

---

## Recommendation

**Current recommendation: Option D (Hybrid) — PROVISIONAL**

```
BM Pharma                        PostgreSQL                    CHIFA
─────────                        ──────────                    ─────
1. Create facture       ──►      facture
2. Create detail_fact   ──►      detail_fact
3. (skip bordereau)              ──►   Pharmacist opens Vente Chifa
                                           ↓
                                    Validates invoice natively
                                           ↓
                                    Creates bordereau in CHIFA
                                           ↓
                                    Signs with PKCS#11 token
                                           ↓
                                    Clôture + Transmit
4. Read status ◄──────────      facture.etat = 'S'
                                 bordereau.etat = 'C'
```

### Rationale

1. **Maximizes compatibility** — CHIFA handles all unknown business logic natively
2. **Minimizes risk** — BM Pharma does not attempt to replicate unknown WHERE clauses or business rules
3. **Preserves audit trail** — CHIFA's internal workflow validates each invoice before inclusion in a bordereau
4. **Gradual migration path** — Once `detail_bord` logic is fully understood, can switch to Option A or C
5. **No data loss** — BM Pharma data is already in PostgreSQL; CHIFA processes it from there

### Trade-offs

| Factor | Option D | Option A | Option B | Option C |
|--------|----------|----------|----------|----------|
| BM Pharma control | MEDIUM | HIGH | LOW | HIGH |
| CHIFA dependency | MEDIUM | LOW | HIGH | LOW |
| Risk of invisibility | NONE | HIGH | LOW | HIGH |
| Pharmacist effort | MEDIUM | LOW | HIGH | LOW |
| Implementation complexity | LOW | LOW | LOW | VERY HIGH |
| Data integrity risk | LOW | MEDIUM | LOW | HIGH |

---

## Decision

> **Décision architecturale provisoire — nécessite BM-PHASE-009-B ou une investigation complémentaire.**

The final architectural decision depends on resolving the following unknowns:

| Unknown | Impact on Decision |
|---------|-------------------|
| Exact WHERE clause of `detail_bord` SQL | If no significant filter exists, Option A becomes viable |
| Whether CHIFA validates invoices before bordereau inclusion | If not, Option A becomes viable |
| Whether `cloturerbord()` accepts externally-created bordereaux | If yes, Option A becomes viable |
| Whether CHIFA's signing process validates invoice data | If not, Option A becomes viable |

Until these are resolved, Option D provides the safest integration path with the least risk of data corruption or CNAS rejection.

---

## Recommended Next Steps for Integration

| Step | Action | Priority |
|------|--------|----------|
| 1 | Restart PostgreSQL logging to capture CHIFA SQL at runtime | HIGH |
| 2 | Test CHIFA workflow: create facture → open in Vente Chifa → observe behavior | HIGH |
| 3 | Test CHIFA workflow: create facture + bordereau → attempt clôture in CHIFA | MEDIUM |
| 4 | Investigate ConfuserEx deobfuscation options (if applicable) | MEDIUM |
| 5 | Decide on final architecture after steps 1-4 are complete | HIGH |
