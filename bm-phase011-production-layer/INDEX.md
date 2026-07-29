# Phase 011 — Production Integration Layer — INDEX

## Documents

| # | Document | Description |
|---|---|---|
| 01 | BM-PHASE-011-01-CONTRACT | Phase 011 scope, constraints, and architecture overview |
| 02 | BM-PHASE-011-02-DESIGN-FACADE | IChifaIntegrationFacade interface and implementation design |
| 03 | BM-PHASE-011-03-DESIGN-STATUS-ENGINE | Status Engine (Technical/Business/Visibility) |
| 04 | BM-PHASE-011-04-DESIGN-MONITORING | CircuitBreaker, Metrics, HealthCheck, CorrelationContext |
| 05 | BM-PHASE-011-05-DESIGN-ERRORS | Exception hierarchy and error handling patterns |
| 06 | BM-PHASE-011-06-REVISIONS | Document revision history |
| 07 | BM-PHASE-011-07-MIGRATION | Migration path from old to new architecture |
| 08 | BM-PHASE-011-08-TEST-REPORT | Test results and coverage summary |
| 09 | BM-PHASE-011-09-VALIDATION | Validation checklist and sign-off |
| 10 | BM-PHASE-011-10-ARCHIVE | Archived artifacts index |
| — | BM-PHASE-011-FINAL-REPORT | Final delivery summary |

## Source Code

| File | Location |
|---|---|
| IChifaIntegrationFacade.cs | `src/BMPharma.CHIFA/Interfaces/` |
| ChifaIntegrationFacade.cs | `src/BMPharma.CHIFA/Services/` |
| StatusEngine.cs + StatusEngineModels.cs | `src/BMPharma.CHIFA/Services/` |
| InvoiceSynchronizer.cs | `src/BMPharma.CHIFA/Services/` |
| BordereauSynchronizer.cs | `src/BMPharma.CHIFA/Services/` |
| StatusSynchronizer.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaCircuitBreaker.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaMetricsService.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaHealthCheckService.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaMonitoringService.cs | `src/BMPharma.CHIFA/Services/` |
| CorrelationContext.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaExceptions.cs | `src/BMPharma.CHIFA/Services/` |
| ChifaIntegrationFacadeTests.cs | `tests/BMPharma.CHIFA.Tests/` |
| StatusEngineTests.cs | `tests/BMPharma.CHIFA.Tests/` |
| MonitoringTests.cs | `tests/BMPharma.CHIFA.Tests/` |
| ChifaExceptionTests.cs | `tests/BMPharma.CHIFA.Tests/` |
| SynchronizerTests.cs | `tests/BMPharma.CHIFA.Tests/` |
| ChifaDashboardViewModel.cs (refactored) | `src/BMPharma.UI/ViewModels/` |
| ChifaInvoicePreparationViewModel.cs (refactored) | `src/BMPharma.UI/ViewModels/` |
| ChifaBordereauStatusViewModel.cs (refactored) | `src/BMPharma.UI/ViewModels/` |
| DependencyInjection.cs (updated) | `src/BMPharma.CHIFA/` |

## Archive

Previous-version artifacts: `bm-phase011-production-layer/archive/`
