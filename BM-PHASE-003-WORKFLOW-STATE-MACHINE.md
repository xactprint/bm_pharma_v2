# BM-PHASE-003 — WORKFLOW STATE MACHINE

## Overview

The CHIFA workflow state machine defines all valid transitions for an invoice from Draft to Transmitted.

## States

| State | Value | Description | Requires Human | Is Simulated |
|-------|-------|-------------|----------------|--------------|
| Draft | 0 | Invoice created locally | No | No |
| Validated | 1 | Passed local + CHIFA validation | No | No |
| PreparedForChifa | 2 | Data prepared for CHIFA write | No | No |
| WrittenToChifa | 3 | Written to PostgreSQL CHIFA | No | Yes |
| VisibleInChifa | 4 | Confirmed visible in CHIFA | **Yes** | Yes |
| Signed | 5 | Signed via CHIFA-OFFICINE token | No | Yes |
| BordereauAssigned | 6 | Assigned to bordereau | **Yes** | Yes |
| BordereauClosed | 7 | Bordereau closed | No | Yes |
| Transmitted | 8 | Transmitted to CNAS (terminal) | No | Yes |
| Failed | 9 | Error occurred | No | No |
| Rejected | 10 | Rejected by CHIFA | No | No |
| RollbackRequired | 11 | Rollback needed | No | No |
| Cancelled | 12 | User cancelled (terminal) | No | No |

## Allowed Transitions

```
Draft → Validated, Cancelled
Validated → PreparedForChifa, Failed, Cancelled
PreparedForChifa → WrittenToChifa, Failed, RollbackRequired
WrittenToChifa → VisibleInChifa, Failed, RollbackRequired
VisibleInChifa → Signed, Rejected, Failed  [HUMAN ACTION REQUIRED]
Signed → BordereauAssigned, Failed
BordereauAssigned → BordereauClosed, Rejected, Failed  [HUMAN ACTION REQUIRED]
BordereauClosed → Transmitted, Failed
Transmitted → (none — terminal)
Failed → Draft (retry)
Rejected → Draft (retry)
RollbackRequired → Draft (retry)
Cancelled → Draft (retry)
```

## IntegrationState Distinction

| State | Meaning |
|-------|---------|
| Confirmed | Real CHIFA operation confirmed by CHIFA-OFFICINE |
| Simulated | Operation simulated locally for testing |
| Unknown | State not determined |
| Pending | Operation initiated but not confirmed |
| NotApplicable | State does not apply |

## Human Action Points

1. **VisibleInChifa** → User must present professional card in CHIFA-OFFICINE
2. **BordereauAssigned** → User must sign bordereau through CHIFA-OFFICINE

## Key Invariant

BM Pharma NEVER claims signing, closure, or transmission as confirmed unless a real confirmation from CHIFA-OFFICINE exists. All post-VisibleInChifa states are marked as `Simulated` in IntegrationState.
