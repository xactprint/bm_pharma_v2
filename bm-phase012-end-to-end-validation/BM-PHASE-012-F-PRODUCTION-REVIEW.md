# BM-PHASE-012-F — Production Readiness Review

## Risk Assessment

| Risk | Severity | Likelihood | Mitigation | Residual |
|---|---|---|---|---|
| **Bordereau visibility** (TST003 root cause unknown) | **CRITICAL** | High | Option D fully defers to CHIFA-OFFICINE | Medium |
| **PostgreSQL 9.3** (EOL 2018, 32-bit) | **HIGH** | Medium | Npgsql EF Core works, but no support guarantees | High |
| **Superuser credentials** | **HIGH** | Low | Trust auth on local network | Medium |
| **CHIFA-OFFICINE dependency** | **HIGH** | Medium | BM cannot create/sign/close/transmit without it | High |
| **Token hardware (Identiv uTrust 3512)** | **HIGH** | Low | Managed by CHIFA-OFFICINE, not BM | Medium |
| **CNAS FTP transmission** | **HIGH** | Low | Managed by CHIFA-OFFICINE, external dependency | Medium |
| **DataTable detail_bord** | **HIGH** | High | BM never creates — CHIFA-OFFICINE handles | Low |
| **Memory leak (metrics unbounded)** | **LOW** | Medium | Diagnostic only, capped by recommendation | Low |
| **Audit log unbounded** | **LOW** | Medium | Only diagnostic sessions | Low |
| **No automated deployment** | **MEDIUM** | High | Manual deployment only | High |
| **No CI/CD pipeline** | **MEDIUM** | High | Manual testing only | High |
| **MainViewModel DI bypass** | **LOW** | High | ModeProvider null if resolved via parameterless | Low |

## External Dependencies

| Dependency | Version | Status |
|---|---|---|
| PostgreSQL | 9.3.4 32-bit | ⚠️ EOL 2018, limited to 4GB shared buffers |
| CHIFA-OFFICINE | Proprietary Delphi app | Not modifiable, Option D compliant |
| Identiv uTrust 3512 | PKCS#11 token | Not managed by BM Pharma |
| CNAS FTP Server | 41.111.149.250:21 | External, availability not guaranteed |
| .NET | 8.0 | Supported until 2026 |
| EF Core Npgsql | 8.0.4 | Latest stable |

## Known Limitations

### TST003 — Root Cause NOT DETERMINED
Six hypotheses remain. Bordereau invisibility in CHIFA UI is the key limitation. Option D fully mitigates by deferring all bordereau operations to CHIFA-OFFICINE.

### PostgreSQL 32-bit Impact
- Maximum shared_buffers: ~4GB
- Maximum connections limited
- No 64-bit performance features
- Testing on 9.3.4 revealed 3 bugs (DateTime Kind, FK ordering, column type mappings) — all fixed

### No Rollback Functionality
`cloturerbord()` PG function executed by CHIFA-OFFICINE. BM Pharma has no rollback capability for closed bordereaux.

## Production Recommendations

| Priority | Recommendation | Impact |
|---|---|---|
| P0 | Upgrade PostgreSQL to 64-bit (v15+) | Performance, reliability, security |
| P0 | Create restricted PG role for BM Pharma | Security |
| P1 | Add automated deployment script | DevOps |
| P1 | Replace trust auth with md5/scram | Security |
| P2 | Add CI/CD pipeline | Quality assurance |
| P2 | Cap ChifaMetricsService memory | Stability |
| P3 | Fix MainViewModel DI bypass | Correctness |
| P3 | Consolidate duplicate type definitions | Maintainability |
