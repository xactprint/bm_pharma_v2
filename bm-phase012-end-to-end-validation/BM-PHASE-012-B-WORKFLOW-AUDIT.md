# BM-PHASE-012-B — Workflow Audit

## Complete End-to-End Workflow

```
BM Pharma (UI) → Facade → Validator → InvoiceService → EF Core → PostgreSQL → CHIFA-OFFICINE → Signing → CNAS
```

## Phase 1: Invoice Preparation

| Step | Service | Method | Precondition | Postcondition | Error |
|---|---|---|---|---|---|
| 1.1 Build Request | ViewModel | BuildChifaRequest() | Form filled | ChifaInvoiceRequest created | Validation errors as ChifaValidationError |
| 1.2 Validate | Facade → Workflow.ValidateOnlyAsync | ValidateInvoiceAsync | Request built | ChifaWorkflowResult.IsSuccess | ValidationFailed status |
| 1.3 Prepare | Facade → Workflow.PrepareInvoiceAsync | PrepareInvoiceAsync | Validation passed | ChifaWorkflowResult with Step=VALIDATED | ValidationFailed / WriteFailed |

## Phase 2: Invoice Persistence

| Step | Service | Method | Precondition | Postcondition | Error |
|---|---|---|---|---|---|
| 2.1 Check exists | Facade → InvoiceService | CreateInvoiceAsync | NumFact unique | Conflict if exists | Conflict status |
| 2.2 Write facture | ChifaPostgresInvoiceService | CreateInvoiceAsync | WriteGuard passes | ChifaFacture row inserted | WriteException, ConcurrencyException |
| 2.3 Write detail_fact | ChifaPostgresInvoiceService | (internal) | facture written | ChifaDetailFact rows inserted | WriteException, FK violation |
| 2.4 Audit log | ChifaAuditService | LogOperationAsync | Operation completed | Audit entry persisted | Logged as warning only |

## Phase 3: Visibility Check

| Step | Service | Method | Precondition | Postcondition | Error |
|---|---|---|---|---|---|
| 3.1 Read facture | StatusSynchronizer | LoadInvoiceAsync | Invoice written | ChifaInvoiceStatusSnapshot | NotFound status |
| 3.2 Determine visibility | StatusSynchronizer | (NumBord check) | Snapshot loaded | VisibleInFacture / NotVisible | VisibilityException |

## Phase 4: Synchronization

| Step | Service | Method | Precondition | Postcondition | Error |
|---|---|---|---|---|---|
| 4.1 Check availability | IntegrationService | IsChifaAvailableAsync | — | true/false | ConnectionException |
| 4.2 Sync invoices | InvoiceSynchronizer | SynchronizeAsync | CHIFA available | ChifaSyncSummary | SynchronizationException |
| 4.3 Sync bordereaux | BordereauSynchronizer | SynchronizeAsync | CHIFA available | ChifaSyncSummary | SynchronizationException |

## Phase 5: Bordereau (Option D — CHIFA-OFFICINE only)

| Step | Owner | Action | Precondition | Postcondition |
|---|---|---|---|---|
| 5.1 Create bordereau | CHIFA-OFFICINE | User creates in CHIFA UI | Invoices visible | Bordereau row created |
| 5.2 Assign invoices | CHIFA-OFFICINE | Select invoices for bordereau | Bordereau exists | Invoices linked to NumBord |
| 5.3 Sign | CHIFA-OFFICINE | PKCS#7 via Identiv uTrust 3512 | Token inserted | Bordereau signed |
| 5.4 Clôture | CHIFA-OFFICINE | cloturerbord() PG function | Bordereau signed | Bordereau closed |
| 5.5 Transmit CNAS | CHIFA-OFFICINE | FTP to 41.111.149.250:21 | Bordereau closed | CNAS transmitted |

**BM Pharma never creates/signs/closes/transmits bordereaux.** This is the Option D constraint.

## State Machine States

### Invoice States (ChifaWorkflowStateMachine — 13 states)
```
Draft → Validated → PreparedForChifa → WrittenToChifa → VisibleInChifa
                                                              ↓
                                                    Signed → BordereauAssigned
                                                                      ↓
                                                            BordereauClosed → Transmitted
                                                                      ↓
                                                           Failed / Rejected / RollbackRequired
                                                                      ↓
                                                              Cancelled
```

### Bordereau States (BordereauWorkflowStateMachine)
```
Draft → Created → Validated → Signed → Closed → Transmitted → Acknowledged
  ↓        ↓          ↓         ↓        ↓           ↓
Cancelled  Cancelled  Cancelled Cancelled Cancelled  Failed
```

## Error Transitions

| Fault | From State | To State | Action |
|---|---|---|---|
| Validation failed | Draft | Draft | Fix errors |
| Write failed | PreparedForChifa | Failed | Retry or rollback |
| Visibility timeout | WrittenToChifa | RollbackRequired | Manual intervention |
| Signing failed | BordereauAssigned | RollbackRequired | Manual intervention |
| CHIFA unavailable | Any | Failed | CircuitBreaker opens, retry after timeout |
| Concurrency conflict | WrittenToChifa | Failed | Retry |
| FK violation | PreparedForChifa | Failed | Check reference data |
