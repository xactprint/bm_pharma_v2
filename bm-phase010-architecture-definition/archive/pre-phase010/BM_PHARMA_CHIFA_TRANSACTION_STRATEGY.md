# BM PHARMA - CHIFA TRANSACTION STRATEGY

**Version**: 1.0
**Date**: 2026-07-25

## Principle

Every multi-table CHIFA operation is transactional. On ANY failure: FULL ROLLBACK.

## Scenario 1: Create Invoice

```
BEGIN;
  -- 1. Read and lock counter
  SELECT next_num_fact FROM parametre WHERE code_ps = :ps FOR UPDATE;
  
  -- 2. Insert facture
  INSERT INTO facture (num_fact, date_fact, etat, mont_maj_fae, mont_maj, type_maj, ...)
  VALUES (:num_fact, NOW(), 'B', 0, 0, 0, ...);
  
  -- 3. Insert detail lines
  INSERT INTO detail_fact (num_fact, num_enr, qte, ppa, mont, ...)
  VALUES (:num_fact, '00001', :qte, :ppa, :mont, ...);
  
  -- 4. Increment counter
  UPDATE parametre SET next_num_fact = next_num_fact + 1 WHERE code_ps = :ps;
COMMIT;
```

## Scenario 2: Create Bordereau with Invoices

```
BEGIN;
  -- 1. Lock bordereau counter
  SELECT next_num_bord FROM parametre WHERE code_ps = :ps FOR UPDATE;
  
  -- 2. Create bordereau
  INSERT INTO bordereau (num_bord, code_centre, date_ouverture, ...)
  VALUES (:num_bord, :centre, NOW(), ...);
  
  -- 3. Link existing invoices
  UPDATE facture SET num_bord = :num_bord WHERE num_fact IN (:facts);
  
  -- 4. Increment counter
  UPDATE parametre SET next_num_bord = next_num_bord + 1 WHERE code_ps = :ps;
COMMIT;
```

## Scenario 3: Full Invoice + Bordereau (Atomic)

```
BEGIN;
  -- 1. Lock both counters
  SELECT next_num_fact, next_num_bord FROM parametre WHERE code_ps = :ps FOR UPDATE;
  
  -- 2. Create bordereau
  INSERT INTO bordereau (...) VALUES (...);
  
  -- 3. Create facture
  INSERT INTO facture (...) VALUES (...);
  
  -- 4. Create detail_fact
  INSERT INTO detail_fact (...) VALUES (...);
  
  -- 5. Increment both counters
  UPDATE parametre SET next_num_fact = next_num_fact + 1, next_num_bord = next_num_bord + 1 WHERE code_ps = :ps;
COMMIT;
```

## Rollback Strategy

If ANY step fails:
- PostgreSQL transaction is automatically rolled back
- BM Pharma SQLite data remains unchanged (separate DB)
- No orphaned records
- Counter not incremented
- User notified of specific failure

## Error Handling

| Error | Action |
|-------|--------|
| Connection failure | Return error, no write attempted |
| Constraint violation | Return error with details |
| Counter lock timeout | Retry up to 3 times, then fail |
| Any INSERT failure | Full rollback |
| Any UPDATE failure | Full rollback |
