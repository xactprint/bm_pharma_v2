# BM PHARMA - CHIFA TEST PLAN

**Version**: 1.0
**Date**: 2026-07-25

## Test Categories

### Unit Tests (no DB required)

1. **CH001**: Valid invoice passes validation
2. **CH002**: Invoice num_fact > 8 chars → REJECTED
3. **CH003**: mont_maj_fae = NULL → auto-set to 0
4. **CH004**: mont_maj = NULL → auto-set to 0
5. **CH005**: Detail without matching facture → REJECTED
6. **CH006**: Amount mismatch (qte*ppa != mont) → REJECTED
7. **CH007**: Quantity > 999 → REJECTED
8. **CH008**: Valid bordereau number generation
9. **CH009**: Bordereau number > 6 chars → REJECTED
10. **CH010**: ReadOnly mode blocks INSERT
11. **CH011**: ReadOnly mode allows SELECT
12. **CH012**: Test mode accepts synthetic data
13. **CH013**: Production mode requires confirmation

### Integration Tests (requires InMemory/SQLite)

14. **CH014**: Invoice creation with detail lines
15. **CH015**: Invoice rollback on detail failure
16. **CH016**: Bordereau creation with counter increment
17. **CH017**: Bordereau rollback preserves counter
18. **CH018**: Concurrent counter increment (thread safety)

### PostgreSQL Integration Tests (requires real CHIFA DB — MANUAL)

19. **CH019**: Invoice visible in CHIFA after write
20. **CH020**: Bordereau visible in bordereau table

## Coverage Target

- Unit tests: 100% of validation logic
- Integration tests: All transaction scenarios
- Architecture tests: All layer boundaries
