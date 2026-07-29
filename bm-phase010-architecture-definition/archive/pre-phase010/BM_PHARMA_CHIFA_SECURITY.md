# BM PHARMA - CHIFA SECURITY

**Version**: 1.0
**Date**: 2026-07-25

## Integration Modes

| Mode | SELECT | INSERT | UPDATE | DELETE | Use Case |
|------|--------|--------|--------|--------|----------|
| ReadOnly | Yes | No | No | No | Default. Safe. |
| Test | Yes | Yes | Yes | Yes | Testing only. Synthetic data. |
| Production | Yes | Yes | Yes | Yes | Protected. Audit required. |

## Default: ReadOnly

BM Pharma ships in ReadOnly mode. All CHIFA writes are blocked by default.

To enable writes, user must:
1. Open Settings → CHIFA Integration
2. Change mode from ReadOnly to Test or Production
3. Enter admin password
4. Acknowledge warning dialog

## Safety Guards

### Guard 1: Write Blocking
In ReadOnly mode, every write method checks mode before executing.
If ReadOnly: return error "CHIFA integration is in ReadOnly mode"

### Guard 2: Confirmation Dialog
In Test/Production mode, before every write:
- Show what will be written
- Require explicit user confirmation
- Log the confirmation

### Guard 3: Audit Trail
Every CHIFA operation is logged:
- Timestamp
- User
- Operation type
- Table affected
- Row key
- Result (success/failure)
- Duration
- Correlation ID

### Guard 4: No Private Key Access
BM Pharma NEVER:
- Accesses the private key
- Reads the PIN
- Signs documents
- Replicates the token functionality

## What BM Pharma MUST NOT Do

1. Modify CHIFA-OFFICINE binary
2. Patch CHIFA DLLs
3. Bypass token authentication
4. Create fake signatures
5. Automate signing operations
6. Send directly to CNAS without CHIFA
7. Delete CHIFA data
8. Modify existing CHIFA records (except counter increment + invoice creation)
