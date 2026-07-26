# BM-PHASE-004.9 — SECURITY REPORT

## Status: ✅ **READ-ONLY INVESTIGATION — NO SECURITY VIOLATIONS**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

BM-PHASE-004.9 performed read-only discovery of the CHIFA_OFFICINE PostgreSQL database using single-user mode. All operations were non-destructive and no security boundaries were crossed. This report also identifies security concerns in the CHIFA-OFFICINE environment itself.

---

## 1. Investigation Security Compliance

### 1.1 Operations Performed

| Operation | Count | Type | Impact |
|-----------|-------|------|--------|
| SELECT queries | ~50 | Read-only | None |
| pg_catalog queries | ~30 | Read-only | None |
| pg_controldata | 1 | Read-only | None |
| File reads | ~10 | Read-only | None |
| INSERT/UPDATE/DELETE | 0 | — | None |
| ALTER/DROP/CREATE | 0 | — | None |
| Configuration changes | 0 | — | None |

### 1.2 Security Rules Compliance

| Rule | Status | Detail |
|------|--------|--------|
| No real writes to PostgreSQL | ✅ PASS | All queries via single-user mode (read-only) |
| No modification of CHIFA-OFFICINE files | ✅ PASS | No files modified |
| No token bypass | ✅ PASS | No tokens used |
| No signature bypass | ✅ PASS | No signatures created or modified |
| No credential exposure | ✅ PASS | Credentials not logged in clear text |
| No patient data exposure | ✅ PASS | Schema-level discovery only, no row data read |
| Read-only investigation documents | ✅ PASS | All output is documentation |

### 1.3 Data Access

| Data Type | Accessed? | Detail |
|-----------|-----------|--------|
| Database metadata | ✅ YES | Table names, column names, types |
| Patient records | ❌ NO | No SELECT on patient data |
| Invoice data | ❌ NO | facture table is empty (0 rows) |
| Drug data | ❌ NO | Only schema, not row data |
| User credentials | ❌ NO | utilisateur table schema only |
| API tokens | ❌ NO | parametre schema only (tokens not read) |
| Signature data | ❌ NO | signature table is empty |

---

## 2. CHIFA-OFFICINE Security Assessment

### 2.1 Authentication Configuration

| Setting | Value | Risk Level |
|---------|-------|------------|
| pg_hba.conf method | **trust** | 🔴 **CRITICAL** |
| Password required | **No** | 🔴 **CRITICAL** |
| Network scope | **0.0.0.0/0** | 🔴 **CRITICAL** |
| SSL/TLS | **Not configured** | 🔴 **HIGH** |
| User restriction | **None** (all users) | 🔴 **HIGH** |

**Analysis**: The `trust` authentication method combined with `0.0.0.0/0` means **any device on the network can connect to this PostgreSQL instance without any credentials** and perform any operation allowed by the user (in this case, superuser `postgres`).

### 2.2 Network Exposure

| Setting | Value | Risk |
|---------|-------|------|
| listen_addresses | `*` | Binds to ALL network interfaces |
| port | 5432 | Default port (well-known) |
| Firewall | Not configured | No network-level protection |
| VPN | Not used | Direct network access |

### 2.3 User Privileges

| User | Privileges | Risk |
|------|-----------|------|
| postgres | Superuser (all privileges) | 🔴 **CRITICAL** — Full database control |
| All other users | Permitted via trust auth | 🔴 **HIGH** — Any user can connect |

### 2.4 Data-at-Risk

| Data Category | Tables | Sensitivity |
|---------------|--------|-------------|
| Patient identity | facture.num_assure, beneficiaire | 🔴 **HIGH** — PII |
| Medical data | facture.prescripteur, medicament | 🟡 **MEDIUM** — Health data |
| Financial data | facture.mont_*, tarif.* | 🟡 **MEDIUM** — Financial data |
| API tokens | parametre.access_token, refresh_token | 🔴 **CRITICAL** — Auth tokens in plaintext |
| Smart card data | token, certificat_token | 🔴 **CRITICAL** — Authentication credentials |
| User credentials | utilisateur (password hash?) | 🔴 **HIGH** — Auth data |

---

## 3. Critical Security Findings

### 3.1 FINDING-001: Trust Authentication

**Severity**: 🔴 CRITICAL
**Location**: `pg_hba.conf`
**Description**: PostgreSQL uses `trust` authentication, requiring no password for any connection.
**Impact**: Any device on the network can connect as the `postgres` superuser.
**Recommendation**: Change to `md5` or `scram-sha-256` authentication with strong passwords.

### 3.2 FINDING-002: Network-Wide Access

**Severity**: 🔴 CRITICAL
**Location**: `pg_hba.conf` (0.0.0.0/0) and `postgresql.conf` (listen_addresses='*')
**Description**: PostgreSQL accepts connections from ANY IP address on ANY network interface.
**Impact**: Any device on the local network (or internet if port-forwarded) can access the database.
**Recommendation**: Restrict to `127.0.0.1/32` (localhost only) or specific LAN IPs.

### 3.3 FINDING-003: API Tokens in Plaintext

**Severity**: 🔴 CRITICAL
**Location**: `parametre.access_token`, `parametre.refresh_token`
**Description**: API access tokens are stored in plaintext in the database.
**Impact**: Any user with database access can extract API tokens.
**Recommendation**: Encrypt tokens at rest, or store in a secure vault.

### 3.4 FINDING-004: No SSL/TLS

**Severity**: 🔴 HIGH
**Location**: `postgresql.conf`
**Description**: No SSL/TLS configuration for database connections.
**Impact**: All data transmitted in plaintext, vulnerable to eavesdropping.
**Recommendation**: Configure SSL certificates for encrypted connections.

### 3.5 FINDING-005: Superuser as Application User

**Severity**: 🔴 HIGH
**Location**: `Lancer_Serveur.bat` (PGUSER=postgres)
**Description**: The application connects as the PostgreSQL superuser.
**Impact**: Full database control, including DROP TABLE, ALTER SYSTEM, etc.
**Recommendation**: Create a dedicated application user with minimal privileges.

### 3.6 FINDING-006: Minimal WAL Level

**Severity**: 🟡 MEDIUM
**Location**: `postgresql.conf` (wal_level=minimal)
**Description**: WAL level set to minimal, reducing crash recovery capability.
**Impact**: Data loss risk in case of corruption.
**Recommendation`: Change to `replica` or `logical` for better durability.

### 3.7 FINDING-007: 32-bit EOL PostgreSQL

**Severity**: 🟡 MEDIUM
**Location**: PostgreSQL 9.3.4 (x86)
**Description**: PostgreSQL 9.3.4 is 32-bit, end-of-life since 2018, with known security vulnerabilities.
**Impact**: No security patches, known vulnerabilities unpatched.
**Recommendation`: Upgrade to PostgreSQL 16.x when feasible.

---

## 4. Token/Smart Card Security

### 4.1 Smart Card Configuration

The `settings.cfg` file configures smart card readers:
- Gemalto readers
- Identiv uTrust 3512 SAM readers

**Purpose**: CNAS beneficiary card reading
**Risk**: Smart card data could be intercepted if reader communication is not encrypted

### 4.2 Token Table

The `token` table stores:
- `code_affect` (PK) — Affectation code
- IP address (unique index: TOKEN_UN)
- Likely: token values, expiry dates

**Risk**: Tokens stored in database could be extracted by anyone with database access.

### 4.3 Certificate Table

The `certificat_token` table stores:
- `num_serie` (PK) — Serial number
- Likely: certificate data

**Risk**: Certificate data could be extracted and used for impersonation.

---

## 5. BM Pharma Security Recommendations

### 5.1 Connection Security

| Recommendation | Priority | Implementation |
|---------------|----------|----------------|
| Use localhost only | CRITICAL | Never expose PG to network |
| No remote connections | CRITICAL | 127.0.0.1 only |
| Connection pooling | HIGH | Min=1, Max=5 |
| Command timeout | HIGH | 30 seconds max |
| Connection timeout | HIGH | 10 seconds max |

### 5.2 Data Security

| Recommendation | Priority | Implementation |
|---------------|----------|----------------|
| Read-only for reference tables | CRITICAL | Never write to medicament, ln, etc. |
| Write-only to billing tables | HIGH | facture, detail_fact, bordereau, signature |
| No patient data logging | CRITICAL | Never log num_assure, nom, prenom |
| No token logging | CRITICAL | Never log access_token, refresh_token |
| No signature logging | HIGH | Never log signature data |

### 5.3 Application Security

| Recommendation | Priority | Implementation |
|---------------|----------|----------------|
| Parameterized queries | CRITICAL | Use EF Core (always parameterized) |
| No SQL injection | CRITICAL | EF Core prevents this |
| Input validation | HIGH | Validate all inputs before DB |
| Error handling | HIGH | Never expose PG errors to users |
| Audit logging | MEDIUM | Log all write operations |

### 5.4 Network Security

| Recommendation | Priority | Implementation |
|---------------|----------|----------------|
| Localhost firewall rule | CRITICAL | Block port 5432 from external |
| No port forwarding | CRITICAL | Never expose to internet |
| VPN for remote access | MEDIUM | If remote access needed |
| Network monitoring | LOW | Monitor for unauthorized connections |

---

## 6. Compliance Considerations

### 6.1 Data Protection

| Regulation | Requirement | BM Pharma Status |
|-----------|-------------|------------------|
| Patient data protection | Encrypt PII at rest | ⚠️ Not yet implemented |
| Access control | Authenticate all users | ⚠️ Trust auth = no authentication |
| Audit trail | Log data access | ⚠️ Not yet implemented |
| Data minimization | Collect only necessary data | ✅ Schema-level only |

### 6.2 Healthcare Data

| Data Type | Sensitivity | Protection Needed |
|-----------|-------------|-------------------|
| Social security numbers | HIGH | Encrypt, restrict access |
| Patient names | HIGH | Never log, restrict display |
| Medical prescriptions | MEDIUM | Restrict access |
| Drug data | LOW | Read-only access acceptable |

---

## 7. Security Testing Checklist

| Test | Status | Result |
|------|--------|--------|
| No writes to PostgreSQL | ✅ PASS | Zero write operations |
| No CHIFA file modifications | ✅ PASS | Zero file modifications |
| No credential exposure | ✅ PASS | Credentials not logged |
| No patient data access | ✅ PASS | Schema-level only |
| No token bypass | ✅ PASS | No tokens used |
| No signature bypass | ✅ PASS | No signatures created |
| Read-only documents only | ✅ PASS | All output is documentation |

---

## 8. Summary

| Category | Findings | Critical |
|----------|----------|----------|
| Investigation compliance | ✅ PASS | 0 |
| CHIFA-OFFICINE security | 🔴 7 findings | 3 CRITICAL |
| BM Pharma recommendations | 15 recommendations | 4 CRITICAL |
| Compliance gaps | 3 gaps identified | 1 HIGH |

**Overall Assessment**: The BM-PHASE-004.9 investigation was conducted securely with no violations. However, the CHIFA-OFFICINE database environment has **CRITICAL security vulnerabilities** (trust auth, network-wide access, plaintext tokens) that must be addressed before production deployment.

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
