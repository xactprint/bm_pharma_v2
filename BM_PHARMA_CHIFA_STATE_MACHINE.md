# BM PHARMA - CHIFA STATE MACHINE

## Invoice States

```
[BM Pharma Local]
    → Draft (SQLite)
    → Completed (SQLite)
    
[CHIFA Integration]
    → Validating (checking constraints)
    → DatabaseCreated (written to CHIFA PG)
    → CHIFAVisible (confirmed visible in CHIFA UI)
    
[CHIFA-OFFICINE Controlled]
    → SigningRequired (in bordereau, awaiting sign)
    → SigningInProgress (token detected, PIN entered)
    → Signed (P7M signature applied)
    
[Post-Signing]
    → Closed (bordereau closed)
    → Transmitted (sent to CNAS)
    → Rejected (CNAS rejected)
```

## Bordereau States

```
Draft → CreatedInDatabase → ReadyForSigning → Signed → Closed → Transmitted
                                                        ↓
                                                   Rejected
```

### State Definitions

| State | DB exists | CHIFA visible | Signed | Transmitted |
|-------|-----------|--------------|--------|-------------|
| Draft | No | No | No | No |
| CreatedInDatabase | Yes | Maybe | No | No |
| ReadyForSigning | Yes | Yes | No | No |
| Signed | Yes | Yes | Yes | No |
| Closed | Yes | Yes | Yes | No (pending FTP) |
| Transmitted | Yes | Yes | Yes | Yes |
| Rejected | Yes | Yes | Yes | Failed |

## State Transitions

| From | To | Actor | Action |
|------|-----|-------|--------|
| Draft | CreatedInDatabase | BM Pharma | INSERT to CHIFA PG |
| CreatedInDatabase | ReadyForSigning | BM Pharma | Confirm CHIFA visibility |
| ReadyForSigning | Signed | CHIFA | PKCS#11 signing |
| Signed | Closed | CHIFA | cloturerbord() |
| Closed | Transmitted | CHIFA | FTP to CNAS |
| Any | Rejected | CNAS | Rejection response |

## Critical Rule

`CreatedInDatabase` DOES NOT EQUAL `CHIFAVisible`

BM Pharma must never assume visibility. It must verify.
