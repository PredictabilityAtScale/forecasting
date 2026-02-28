/**
 * Capability Matrix — skill assessment engine.
 *
 * Maps each person/team × skill to a numeric level (0-4) and computes
 * aggregate risk scores, level-bucket counts, and heatmap data.
 *
 * Skill levels (from spreadsheet Settings):
 *   0 = Know nothing
 *   1 = Can run and use the tools needed
 *   2 = Can tweak it or do easy bug fixes
 *   3 = Can start from nothing and create
 *   4 = Can teach others
 *
 * Risk score for a skill = sum of all person scores for that skill.
 * Lower score → higher risk (fewer capable people).
 */

/* ── Constants ─────────────────────────────────────────────────────────── */

export const SKILL_LEVELS = [
  { value: 0, label: 'Know nothing', short: 'None' },
  { value: 1, label: 'Can run and use the tools needed', short: 'Run' },
  { value: 2, label: 'Can tweak it or do easy bug fixes', short: 'Tweak' },
  { value: 3, label: 'Can start from nothing and create', short: 'Create' },
  { value: 4, label: 'Can teach others', short: 'Teach' },
] as const

export const FUTURE_NEED_LEVELS = [
  'Not needed',
  'Light need (occasional)',
  'Moderate need (often)',
  'Heavy need (always)',
  'Critical (immediate response needed)',
] as const

export type FutureNeed = (typeof FUTURE_NEED_LEVELS)[number]

/* ── Types ─────────────────────────────────────────────────────────────── */

export interface CapabilityEntry {
  /** personIndex × skillIndex → level (0-4) */
  level: number
}

export interface SkillMeta {
  name: string
  futureNeed: FutureNeed
  trainingLeadMonths: number
}

export interface SkillResult {
  name: string
  teachAndCreate: number // count of people with level >= 3
  doAndMaintain: number // count of people with level === 2
  novice: number // count of people with level === 1
  none: number // count of people with level === 0
  riskScore: number // sum of all levels
  futureNeed: FutureNeed
  trainingLeadMonths: number
  /** Per-person levels for heatmap */
  personLevels: number[]
}

export interface CapabilityResult {
  skills: SkillResult[]
  people: string[]
  /** For risk heatmap: min and max risk across skills */
  minRisk: number
  maxRisk: number
}

/* ── Compute ───────────────────────────────────────────────────────────── */

export function computeCapabilityMatrix(
  people: string[],
  skills: SkillMeta[],
  /** matrix[skillIndex][personIndex] = level (0-4) */
  matrix: number[][],
): CapabilityResult {
  const results: SkillResult[] = skills.map((skill, si) => {
    const row = matrix[si] ?? []
    let teachAndCreate = 0
    let doAndMaintain = 0
    let novice = 0
    let none = 0
    let riskScore = 0
    const personLevels: number[] = []

    for (let pi = 0; pi < people.length; pi++) {
      const level = row[pi] ?? 0
      personLevels.push(level)
      riskScore += level
      if (level >= 3) teachAndCreate++
      else if (level === 2) doAndMaintain++
      else if (level === 1) novice++
      else none++
    }

    return {
      name: skill.name,
      teachAndCreate,
      doAndMaintain,
      novice,
      none,
      riskScore,
      futureNeed: skill.futureNeed,
      trainingLeadMonths: skill.trainingLeadMonths,
      personLevels,
    }
  })

  const scores = results.map((r) => r.riskScore).filter((s) => s > 0)
  const minRisk = scores.length > 0 ? Math.min(...scores) : 0
  const maxRisk = scores.length > 0 ? Math.max(...scores) : 0

  return { skills: results, people, minRisk, maxRisk }
}

/* ── Helpers ───────────────────────────────────────────────────────────── */

/** Returns a color class for a risk score within [min, max] range.
 *  Lower risk = redder (worse). Higher = greener (better). */
export function riskColor(
  score: number,
  min: number,
  max: number,
): string {
  if (score === 0) return 'bg-gray-100 text-gray-400 dark:bg-gray-800 dark:text-gray-500'
  const range = max - min
  if (range === 0) return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300'
  const pct = (score - min) / range
  if (pct < 0.25)
    return 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300'
  if (pct < 0.5)
    return 'bg-orange-100 text-orange-800 dark:bg-orange-900/40 dark:text-orange-300'
  if (pct < 0.75)
    return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300'
  return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300'
}

/** Returns a color class for a skill level cell (0-4). */
export function levelColor(level: number): string {
  switch (level) {
    case 0:
      return 'bg-red-50 text-red-400 dark:bg-red-950/30 dark:text-red-400'
    case 1:
      return 'bg-orange-50 text-orange-600 dark:bg-orange-950/30 dark:text-orange-400'
    case 2:
      return 'bg-yellow-50 text-yellow-700 dark:bg-yellow-950/30 dark:text-yellow-400'
    case 3:
      return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400'
    case 4:
      return 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300'
    default:
      return ''
  }
}
