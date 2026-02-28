/**
 * Latent Defect Estimation — Lincoln-Petersen capture-recapture method.
 *
 * Two independent groups each review/test the same product and flag defects.
 * Defects found by both groups let us estimate how many remain undiscovered.
 *
 * Formula:  Estimated Total = ⌈(Total_A × Total_B) / Both⌉
 */

/* ── Types ─────────────────────────────────────────────────────────────── */

export interface Defect {
  name: string
  severity: string
  foundByA: boolean
  foundByB: boolean
}

export interface LatentDefectResult {
  totalA: number
  totalB: number
  onlyA: number
  onlyB: number
  both: number
  totalFound: number
  /** ⌈(totalA × totalB) / both⌉  — null when both === 0 */
  estimatedTotal: number | null
  /** estimatedTotal − totalFound  — null when both === 0 */
  estimatedUndiscovered: number | null
  /** Detail per defect */
  details: DefectDetail[]
}

export interface DefectDetail {
  name: string
  severity: string
  foundByA: boolean
  foundByB: boolean
  /** 'both' | 'onlyA' | 'onlyB' | 'neither' */
  bucket: 'both' | 'onlyA' | 'onlyB' | 'neither'
}

/* ── Compute ───────────────────────────────────────────────────────────── */

export function computeLatentDefects(defects: Defect[]): LatentDefectResult {
  let totalA = 0
  let totalB = 0
  let onlyA = 0
  let onlyB = 0
  let both = 0

  const details: DefectDetail[] = defects.map((d) => {
    const a = d.foundByA
    const b = d.foundByB

    if (a) totalA++
    if (b) totalB++

    let bucket: DefectDetail['bucket']
    if (a && b) {
      both++
      bucket = 'both'
    } else if (a) {
      onlyA++
      bucket = 'onlyA'
    } else if (b) {
      onlyB++
      bucket = 'onlyB'
    } else {
      bucket = 'neither'
    }

    return {
      name: d.name,
      severity: d.severity,
      foundByA: a,
      foundByB: b,
      bucket,
    }
  })

  const totalFound = onlyA + onlyB + both

  let estimatedTotal: number | null = null
  let estimatedUndiscovered: number | null = null

  if (both > 0) {
    estimatedTotal = Math.ceil((totalA * totalB) / both)
    estimatedUndiscovered = estimatedTotal - totalFound
  }

  return {
    totalA,
    totalB,
    onlyA,
    onlyB,
    both,
    totalFound,
    estimatedTotal,
    estimatedUndiscovered,
    details,
  }
}
