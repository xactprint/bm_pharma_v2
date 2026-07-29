# BM PHARMA - CHIFA BORDEREAU CONTRACT

**Version**: 1.0
**Date**: 2026-07-25

## Purpose

Defines how BM Pharma creates and manages bordereaux in CHIFA PostgreSQL.

## Bordereau Lifecycle in BM Pharma

### Step 1: Prepare
- Collect invoices to include
- Validate each invoice exists in CHIFA PG
- Calculate totals

### Step 2: Create
- Read next_num_bord from parametre (FOR UPDATE)
- INSERT bordereau row
- UPDATE facture.num_bord for each invoice
- Increment parametre.next_num_bord

### Step 3: Verify Visibility
- SELECT bordereau to confirm it exists
- Note: CHIFA-visible status requires CHIFA-OFFICINE refresh

### Step 4: Ready for Signing
- All invoices linked
- Bordereau in CHIFA PG
- User proceeds to CHIFA-OFFICINE for signing

### Step 5: Signing (CHIFA-OFFICINE)
- BM Pharma does NOT participate
- CHIFA handles PKCS#11 signing

### Step 6: Closure (CHIFA-OFFICINE)
- BM Pharma does NOT participate
- CHIFA handles bordereau closure

## Visibility Limitation

A bordereau created via PostgreSQL INSERT may NOT appear in CHIFA-OFFICINE's "Visualiser Bordereau" interface. This is because CHIFA uses a proprietary DataSet with cached SQL that excludes externally-created bordereaux.

**Mitigation:**
- The bordereau EXISTS in the database (verifiable via SELECT)
- CHIFA-OFFICINE may need a restart or cache refresh
- BM Pharma should track this distinction explicitly

## Counter Management

| Counter | Table | Column | Format | Current |
|---------|-------|--------|--------|---------|
| Invoice | parametre | next_num_fact | integer | 1 |
| Bordereau | parametre | next_num_bord | smallint | 215 |

Both counters are managed atomically using SELECT FOR UPDATE.

## Collision Prevention

- `FOR UPDATE` acquires row-level lock on parametre
- Only one transaction can read+increment at a time
- Second transaction waits for lock release
- After lock, reads the NEW value (already incremented)

## Bordereau Number Format
- varchar(6)
- Zero-padded: '000215', '000216', etc.
- Must fit in 6 chars: max 999999

## Atomic Counter Protocol

```sql
BEGIN;
  -- Lock the parametre row
  SELECT next_num_bord FROM parametre 
  WHERE code_ps = :code_ps FOR UPDATE;
  
  -- Use current value
  SET @num_bord = :next_num_bord;
  
  -- Insert bordereau with zero-padded number
  INSERT INTO bordereau (num_bord, code_centre, date_ouverture, etat)
  VALUES (LPAD(@num_bord::text, 6, '0'), :code_centre, NOW(), NULL);
  
  -- Increment counter
  UPDATE parametre SET next_num_bord = next_num_bord + 1 
  WHERE code_ps = :code_ps;
COMMIT;
```
