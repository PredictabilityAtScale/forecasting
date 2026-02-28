    /**
 * Wrong Order-O-Meter
 *
 * Measures how far the actual delivery order diverged from the planned order.
 * Accounts for:
 *   - Planned items delivered out of order  → penalty = |delivered position − planned position|
 *   - Planned items never delivered         → penalty = plannedCount + 1  (max penalty)
 *   - Unplanned items that were delivered   → penalty = plannedCount + 1  (max penalty)
 *
 * Two scores are produced:
 *   - "With Unplanned" — includes unplanned‑item penalties
 *   - "Planned Only"   — ignores the per‑delivery penalty for unplanned items
 *                         (undelivered planned items still penalised)
 */

export interface WrongOrderInput {
  /** The intended delivery order (feature / item names). */
  planned: string[]
  /** The actual delivery order (may include names not in planned). */
  delivered: string[]
}

export interface DeliveredItemDetail {
  name: string
  kind: 'planned' | 'unplanned'
  /** 1‑based position in the delivered list */
  deliveredPosition: number
  /** 1‑based position in the planned list, or null if unplanned */
  plannedPosition: number | null
  /** Displacement penalty for this delivered item */
  penalty: number
}

export interface UndeliveredItemDetail {
  name: string
  /** 1-based position in the planned list */
  plannedPosition: number
  /** Penalty for not being delivered */
  penalty: number
}

export interface WrongOrderResult {
  /** Score including unplanned‑item penalties. */
  scoreWithUnplanned: number
  /** Score ignoring unplanned‑delivery penalties (still counts undelivered). */
  scorePlannedOnly: number
  /** Minimum possible score (always 0 — perfect ordering). */
  minScore: number
  /** Maximum possible score (worst case — everything maximally wrong). */
  maxScore: number
  /** Breakdown per delivered item. */
  deliveredDetails: DeliveredItemDetail[]
  /** Planned items that were never delivered. */
  undeliveredDetails: UndeliveredItemDetail[]
  /** Count of planned items that appeared in delivered. */
  plannedDeliveredCount: number
  /** Count of unplanned items in delivered. */
  unplannedDeliveredCount: number
  /** Duplicate names found in planned list. */
  plannedDuplicates: string[]
  /** Duplicate names found in delivered list. */
  deliveredDuplicates: string[]
}

export function computeWrongOrder(input: WrongOrderInput): WrongOrderResult {
  const planned = input.planned.filter((s) => s.trim() !== '')
  const delivered = input.delivered.filter((s) => s.trim() !== '')
  const plannedCount = planned.length

  // Detect duplicates
  const plannedDuplicates = findDuplicates(planned)
  const deliveredDuplicates = findDuplicates(delivered)

  // Build lookup: planned name → 1‑based position (first occurrence)
  const plannedIndex = new Map<string, number>()
  for (let i = 0; i < planned.length; i++) {
    const key = planned[i].trim().toLowerCase()
    if (!plannedIndex.has(key)) {
      plannedIndex.set(key, i + 1) // 1‑based
    }
  }

  // Build lookup: delivered name → exists
  const deliveredSet = new Set<string>()
  for (const d of delivered) {
    deliveredSet.add(d.trim().toLowerCase())
  }

  const maxPenalty = plannedCount + 1

  // ── Per‑delivered‑item penalties ──────────────────────────────────────
  const deliveredDetails: DeliveredItemDetail[] = []
  let sumPenaltyAll = 0
  let sumPenaltyPlannedOnly = 0

  for (let i = 0; i < delivered.length; i++) {
    const name = delivered[i]
    const key = name.trim().toLowerCase()
    const pos = i + 1 // 1‑based delivered position
    const plannedPos = plannedIndex.get(key) ?? null

    if (plannedPos !== null) {
      // Planned item: penalty = displacement
      const penalty = Math.abs(pos - plannedPos)
      deliveredDetails.push({
        name,
        kind: 'planned',
        deliveredPosition: pos,
        plannedPosition: plannedPos,
        penalty,
      })
      sumPenaltyAll += penalty
      sumPenaltyPlannedOnly += penalty
    } else {
      // Unplanned item: max penalty
      deliveredDetails.push({
        name,
        kind: 'unplanned',
        deliveredPosition: pos,
        plannedPosition: null,
        penalty: maxPenalty,
      })
      sumPenaltyAll += maxPenalty
      // Not counted in planned‑only score
    }
  }

  // ── Undelivered planned items ────────────────────────────────────────
  const undeliveredDetails: UndeliveredItemDetail[] = []
  let sumUndeliveredPenalty = 0

  for (let i = 0; i < planned.length; i++) {
    const key = planned[i].trim().toLowerCase()
    if (!deliveredSet.has(key)) {
      const penalty = maxPenalty
      undeliveredDetails.push({
        name: planned[i],
        plannedPosition: i + 1,
        penalty,
      })
      sumUndeliveredPenalty += penalty
    }
  }

  // ── Scores ───────────────────────────────────────────────────────────
  const scoreWithUnplanned = sumPenaltyAll + sumUndeliveredPenalty
  const scorePlannedOnly = sumPenaltyPlannedOnly + sumUndeliveredPenalty

  // Count of items contributing penalties (for max score calculation)
  const penalisedItemCount =
    deliveredDetails.filter((d) => d.penalty >= 0).length +
    undeliveredDetails.length
  const maxScore = maxPenalty * penalisedItemCount

  const plannedDeliveredCount = deliveredDetails.filter(
    (d) => d.kind === 'planned',
  ).length
  const unplannedDeliveredCount = deliveredDetails.filter(
    (d) => d.kind === 'unplanned',
  ).length

  return {
    scoreWithUnplanned,
    scorePlannedOnly,
    minScore: 0,
    maxScore: maxScore || 1, // avoid div‑by‑zero for gauge
    deliveredDetails,
    undeliveredDetails,
    plannedDeliveredCount,
    unplannedDeliveredCount,
    plannedDuplicates,
    deliveredDuplicates,
  }
}

function findDuplicates(items: string[]): string[] {
  const seen = new Set<string>()
  const dupes = new Set<string>()
  for (const item of items) {
    const key = item.trim().toLowerCase()
    if (key === '') continue
    if (seen.has(key)) dupes.add(item)
    seen.add(key)
  }
  return Array.from(dupes)
}
