==========================================================
  BM-PHASE-008-B — ROLLBACK TST003
  Date: 2026-07-28T16:57:52.7750788Z
==========================================================
  Connected: 9.3.4 | pharm@CHIFA_OFFICINE

=== STEP 1 — PRE-ROLLBACK SNAPSHOT ===
  facture           = 1
  detail_fact       = 1
  bordereau         = 1
  signature         = 0
  next_num_fact     = 1
  next_num_bord     = 215
  TST003 facture    = 1
  TST003 detail_fact= 1
  bordereau 215     = 1

=== STEP 2 — DELETE TST003 DATA (dans l'ordre FK inverse) ===
  DELETE detail_fact WHERE num_fact='TST003'
  → 1 ligne(s) supprimée(s)

  DELETE facture WHERE num_fact='TST003'
  → 1 ligne(s) supprimée(s)

  DELETE bordereau WHERE num_bord='215'
  → 1 ligne(s) supprimée(s)

  ✅ TRANSACTION ROLLBACK COMMITTED

=== STEP 3 — VÉRIFICATION POST-ROLLBACK ===
  facture           = 0  (attendu: 0)
  detail_fact       = 0  (attendu: 0)
  bordereau         = 0  (attendu: 0)
  signature         = 0  (attendu: 0)
  next_num_fact     = 1  (attendu: 1)
  next_num_bord     = 215  (attendu: 215)
  TST003 facture    = 0  (attendu: 0)
  TST003 detail_fact= 0  (attendu: 0)
  bordereau 215     = 0  (attendu: 0)
  orphelins f→b     = 0  (attendu: 0)
  orphelins d→f     = 0  (attendu: 0)

  ➡ Statut: ✅ BASELINE RESTORED
