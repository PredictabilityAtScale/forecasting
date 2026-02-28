/**
 * Team Dashboard – calculation engine
 *
 * Produces the same derived metrics as the "Team Dashboard" spreadsheet:
 *   • Weekly throughput  (run chart + histogram + percentiles)
 *   • Cycle-time scatter (per-item + weekly min/avg/max/percentiles + histogram)
 *   • Work-in-progress   (daily WIP + age buckets, weekly summary)
 *   • Defect / unplanned rate  (per week)
 *   • Cumulative flow     (started vs completed)
 */

/* ── Types ───────────────────────────────────────────────────────────────── */

export interface WorkItem {
  id: string
  completedDate: string          // ISO yyyy-MM-dd  (required for completed)
  startDate: string              // ISO yyyy-MM-dd
  type: 'Planned' | 'Unplanned' | 'Defect'
}

export interface WeekBucket {
  weekStart: string              // ISO Monday date
  weekEnd: string
  label: string                  // e.g. "2025-W07"
  throughput: number
  stories: number                // Planned + Unplanned
  defects: number
  defectRate: number             // defects / throughput
  unplannedRate: number          // (defects + unplanned) / throughput
}

export interface CycleTimeItem {
  id: string
  startDate: string
  completedDate: string
  cycleTime: number              // calendar days (0.5 if same-day)
  type: WorkItem['type']
  weekLabel: string
}

export interface CycleTimeWeekly {
  weekStart: string
  weekEnd: string
  label: string
  min: number
  avg: number
  max: number
  p25: number
  median: number
  p75: number
}

export interface WipDay {
  date: string
  wip: number
  ageLE1: number
  ageLE7: number
  ageLE14: number
  ageGT14: number
}

export interface WipWeekly {
  weekStart: string
  weekEnd: string
  label: string
  minWip: number
  avgWip: number
  maxWip: number
  started: number
  completed: number
  delta: number                  // completed – started
  cumStarted: number
  cumCompleted: number
}

export interface Percentiles {
  min: number
  p5: number
  p25: number
  p50: number
  p75: number
  p95: number
  max: number
}

export interface HistogramBin {
  bin: number
  frequency: number
  stories: number
  defects: number
}

export interface DashboardResult {
  /* Throughput */
  weeks: WeekBucket[]
  throughputPercentiles: Percentiles
  throughputHistogram: HistogramBin[]
  avgDefectRate: number
  avgUnplannedRate: number

  /* Cycle time */
  cycleTimeItems: CycleTimeItem[]
  cycleTimeWeekly: CycleTimeWeekly[]
  cycleTimePercentiles: Percentiles
  storyCycleTimeP85: number
  defectCycleTimeP85: number
  cycleTimeHistogram: HistogramBin[]

  /* WIP */
  wipDaily: WipDay[]
  wipWeekly: WipWeekly[]

  /* Filter metadata */
  dateRange: { from: string; to: string }
  totalItems: number
}

/* ── Settings ────────────────────────────────────────────────────────────── */

export interface DashboardSettings {
  sameDayCycleTime: number       // default 0.5
  ageThresholds: [number, number, number]  // [1, 7, 14]
  wipWarningPct: number          // default 0.3 (30%)
  ageWarningPct: number          // default 0.3 (30%)
  timespanMonths: number | null  // null = All
  typeFilter: 'All' | 'Planned' | 'Unplanned' | 'Defect'
}

export const DEFAULT_SETTINGS: DashboardSettings = {
  sameDayCycleTime: 0.5,
  ageThresholds: [1, 7, 14],
  wipWarningPct: 0.3,
  ageWarningPct: 0.3,
  timespanMonths: 3,
  typeFilter: 'All',
}

/* ── Helpers ─────────────────────────────────────────────────────────────── */

function toDate(s: string): Date {
  return new Date(s + 'T00:00:00')
}

function isoDate(d: Date): string {
  return d.toISOString().slice(0, 10)
}

/** Previous/same Monday (ISO week start) */
function mondayOf(d: Date): Date {
  const day = d.getUTCDay()           // 0=Sun … 6=Sat
  const diff = day === 0 ? 6 : day - 1
  const m = new Date(d)
  m.setUTCDate(m.getUTCDate() - diff)
  return m
}

/** Sunday of the same week (week ending) */
function sundayOf(d: Date): Date {
  const m = mondayOf(d)
  const s = new Date(m)
  s.setUTCDate(s.getUTCDate() + 6)
  return s
}

function weekLabel(d: Date): string {
  const mon = mondayOf(d)
  const year = mon.getUTCFullYear()
  // ISO week number
  const jan1 = new Date(Date.UTC(year, 0, 1))
  const dayOfYear = Math.floor((mon.getTime() - jan1.getTime()) / 86_400_000) + 1
  const wn = Math.ceil((dayOfYear + jan1.getUTCDay()) / 7)
  return `${year}-W${String(wn).padStart(2, '0')}`
}

function daysBetween(a: Date, b: Date): number {
  return Math.round((b.getTime() - a.getTime()) / 86_400_000)
}

function addDays(d: Date, n: number): Date {
  const r = new Date(d)
  r.setUTCDate(r.getUTCDate() + n)
  return r
}

function percentile(sorted: number[], p: number): number {
  if (sorted.length === 0) return 0
  const idx = p * (sorted.length - 1)
  const lo = Math.floor(idx)
  const hi = Math.ceil(idx)
  if (lo === hi) return sorted[lo]
  return sorted[lo] + (sorted[hi] - sorted[lo]) * (idx - lo)
}

function computePercentiles(values: number[]): Percentiles {
  const s = [...values].sort((a, b) => a - b)
  return {
    min: s[0] ?? 0,
    p5:  percentile(s, 0.05),
    p25: percentile(s, 0.25),
    p50: percentile(s, 0.50),
    p75: percentile(s, 0.75),
    p95: percentile(s, 0.95),
    max: s[s.length - 1] ?? 0,
  }
}

function buildHistogram(values: number[], binSize = 1): HistogramBin[] {
  if (values.length === 0) return []
  const min = Math.floor(Math.min(...values))
  const max = Math.ceil(Math.max(...values))
  const bins: HistogramBin[] = []
  for (let b = min; b <= max; b += binSize) {
    bins.push({ bin: b, frequency: 0, stories: 0, defects: 0 })
  }
  return bins
}

/* ── Main computation ────────────────────────────────────────────────────── */

export function computeDashboard(
  rawItems: WorkItem[],
  settings: DashboardSettings = DEFAULT_SETTINGS,
): DashboardResult {
  /* ---------- Filter by type ---------- */
  let items = settings.typeFilter === 'All'
    ? rawItems
    : rawItems.filter(i => i.type === settings.typeFilter)

  /* ---------- Filter by timespan ---------- */
  let dateFrom: Date
  let dateTo: Date

  const completedDates = items
    .filter(i => i.completedDate)
    .map(i => toDate(i.completedDate))

  if (completedDates.length === 0) {
    return emptyResult()
  }

  dateTo = new Date(Math.max(...completedDates.map(d => d.getTime())))

  if (settings.timespanMonths != null) {
    dateFrom = new Date(dateTo)
    dateFrom.setUTCMonth(dateFrom.getUTCMonth() - settings.timespanMonths)
  } else {
    dateFrom = new Date(Math.min(...completedDates.map(d => d.getTime())))
  }

  // Apply date filter
  items = items.filter(i => {
    if (!i.completedDate) return false
    const d = toDate(i.completedDate)
    return d >= dateFrom && d <= dateTo
  })

  if (items.length === 0) return emptyResult()

  /* ===================================================================== */
  /*   THROUGHPUT – aggregate by week (completed-date basis)               */
  /* ===================================================================== */

  const weekMap = new Map<string, { start: Date; items: WorkItem[] }>()
  for (const item of items) {
    const d = toDate(item.completedDate)
    const wl = weekLabel(d)
    if (!weekMap.has(wl)) weekMap.set(wl, { start: mondayOf(d), items: [] })
    weekMap.get(wl)!.items.push(item)
  }

  // Build contiguous weeks (fill gaps with 0)
  const allWeekStarts = [...weekMap.values()].map(v => v.start.getTime())
  const firstWeekMon = new Date(Math.min(...allWeekStarts))
  const lastWeekMon = new Date(Math.max(...allWeekStarts))

  const weeks: WeekBucket[] = []
  let cur = new Date(firstWeekMon)
  while (cur <= lastWeekMon) {
    const wl = weekLabel(cur)
    const wItems = weekMap.get(wl)?.items ?? []
    const defects = wItems.filter(i => i.type === 'Defect').length
    const unplanned = wItems.filter(i => i.type === 'Unplanned').length
    const tp = wItems.length
    weeks.push({
      weekStart: isoDate(cur),
      weekEnd: isoDate(sundayOf(cur)),
      label: wl,
      throughput: tp,
      stories: tp - defects,
      defects,
      defectRate: tp > 0 ? defects / tp : 0,
      unplannedRate: tp > 0 ? (defects + unplanned) / tp : 0,
    })
    cur = addDays(cur, 7)
  }

  const tpValues = weeks.map(w => w.throughput)
  const throughputPercentiles = computePercentiles(tpValues)

  // Throughput histogram
  const tpHisto = buildHistogram(tpValues)
  for (const v of tpValues) {
    const idx = tpHisto.findIndex(b => v >= b.bin && v < b.bin + 1)
    if (idx >= 0) tpHisto[idx].frequency++
    else if (tpHisto.length > 0) tpHisto[tpHisto.length - 1].frequency++
  }

  const avgDefectRate = weeks.length > 0
    ? weeks.reduce((s, w) => s + w.defectRate, 0) / weeks.length
    : 0
  const avgUnplannedRate = weeks.length > 0
    ? weeks.reduce((s, w) => s + w.unplannedRate, 0) / weeks.length
    : 0

  /* ===================================================================== */
  /*   CYCLE TIME                                                          */
  /* ===================================================================== */

  const ctItems: CycleTimeItem[] = items
    .filter(i => i.startDate && i.completedDate)
    .map(i => {
      const s = toDate(i.startDate)
      const c = toDate(i.completedDate)
      let ct = daysBetween(s, c)
      if (ct === 0) ct = settings.sameDayCycleTime
      if (ct < 0) ct = 0
      return {
        id: i.id,
        startDate: i.startDate,
        completedDate: i.completedDate,
        cycleTime: ct,
        type: i.type,
        weekLabel: weekLabel(c),
      }
    })

  const ctValues = ctItems.map(i => i.cycleTime)
  const cycleTimePercentiles = computePercentiles(ctValues)

  const storyCtValues = ctItems.filter(i => i.type !== 'Defect').map(i => i.cycleTime).sort((a, b) => a - b)
  const defectCtValues = ctItems.filter(i => i.type === 'Defect').map(i => i.cycleTime).sort((a, b) => a - b)
  const storyCycleTimeP85 = percentile(storyCtValues, 0.85)
  const defectCycleTimeP85 = percentile(defectCtValues, 0.85)

  // Weekly aggregation of cycle time
  const ctWeekMap = new Map<string, { start: Date; values: number[] }>()
  for (const ci of ctItems) {
    const d = toDate(ci.completedDate)
    const wl = ci.weekLabel
    if (!ctWeekMap.has(wl)) ctWeekMap.set(wl, { start: mondayOf(d), values: [] })
    ctWeekMap.get(wl)!.values.push(ci.cycleTime)
  }

  const cycleTimeWeekly: CycleTimeWeekly[] = []
  for (const w of weeks) {
    const entry = ctWeekMap.get(w.label)
    if (!entry || entry.values.length === 0) continue
    const sorted = [...entry.values].sort((a, b) => a - b)
    cycleTimeWeekly.push({
      weekStart: w.weekStart,
      weekEnd: w.weekEnd,
      label: w.label,
      min: sorted[0],
      avg: sorted.reduce((a, b) => a + b, 0) / sorted.length,
      max: sorted[sorted.length - 1],
      p25: percentile(sorted, 0.25),
      median: percentile(sorted, 0.5),
      p75: percentile(sorted, 0.75),
    })
  }

  // Cycle time histogram
  const ctHisto = buildHistogram(ctValues)
  for (const ci of ctItems) {
    const idx = ctHisto.findIndex(b => ci.cycleTime >= b.bin && ci.cycleTime < b.bin + 1)
    if (idx >= 0) {
      ctHisto[idx].frequency++
      if (ci.type === 'Defect') ctHisto[idx].defects++
      else ctHisto[idx].stories++
    } else if (ctHisto.length > 0) {
      ctHisto[ctHisto.length - 1].frequency++
      if (ci.type === 'Defect') ctHisto[ctHisto.length - 1].defects++
      else ctHisto[ctHisto.length - 1].stories++
    }
  }

  /* ===================================================================== */
  /*   WORK IN PROGRESS – day by day                                       */
  /* ===================================================================== */

  // We need ALL items (incl. those without completed date = still in progress)
  const allItemsForWip = (settings.typeFilter === 'All' ? rawItems : rawItems.filter(i => i.type === settings.typeFilter))
    .filter(i => i.startDate)

  const wipDaily: WipDay[] = []
  const [ageLo, ageMid, ageHi] = settings.ageThresholds

  // Build day range
  const allStarts = allItemsForWip.filter(i => i.startDate).map(i => toDate(i.startDate).getTime())
  const wipStart = allStarts.length > 0 ? new Date(Math.min(...allStarts)) : dateFrom
  // Use dateTo for the end  
  const wipEnd = dateTo

  // Apply timespan filter to WIP
  const wipDateFrom = settings.timespanMonths != null
    ? (() => { const d = new Date(wipEnd); d.setUTCMonth(d.getUTCMonth() - settings.timespanMonths!); return d })()
    : wipStart

  let day = new Date(Math.max(wipDateFrom.getTime(), wipStart.getTime()))
  while (day <= wipEnd) {
    let wip = 0, le1 = 0, le7 = 0, le14 = 0, gt14 = 0
    for (const item of allItemsForWip) {
      const s = toDate(item.startDate)
      const c = item.completedDate ? toDate(item.completedDate) : null
      // Item is in progress on this day if started <= day AND (not completed OR completed >= day)
      if (s <= day && (c === null || c >= day)) {
        wip++
        const age = daysBetween(s, day)
        if (age <= ageLo) le1++
        else if (age <= ageMid) le7++
        else if (age <= ageHi) le14++
        else gt14++
      }
    }
    wipDaily.push({
      date: isoDate(day),
      wip,
      ageLE1: le1,
      ageLE7: le7,
      ageLE14: le14,
      ageGT14: gt14,
    })
    day = addDays(day, 1)
  }

  // Weekly WIP summary
  const wipWeekMap = new Map<string, WipDay[]>()
  for (const wd of wipDaily) {
    const wl = weekLabel(toDate(wd.date))
    if (!wipWeekMap.has(wl)) wipWeekMap.set(wl, [])
    wipWeekMap.get(wl)!.push(wd)
  }

  const wipWeekly: WipWeekly[] = []
  let cumStarted = 0
  let cumCompleted = 0

  // Build week entries from the throughput weeks range
  cur = new Date(Math.max(wipDateFrom.getTime(), firstWeekMon.getTime()))
  const wipWeekEnd = lastWeekMon
  while (cur <= wipWeekEnd) {
    const wl = weekLabel(cur)
    const days = wipWeekMap.get(wl) ?? []
    const wips = days.map(d => d.wip)
    const minW = wips.length > 0 ? Math.min(...wips) : 0
    const maxW = wips.length > 0 ? Math.max(...wips) : 0
    const avgW = wips.length > 0 ? wips.reduce((a, b) => a + b, 0) / wips.length : 0

    // Count items started/completed this week
    const s = cur
    const e = sundayOf(cur)
    const startedThisWeek = allItemsForWip.filter(i => {
      const sd = toDate(i.startDate)
      return sd >= s && sd <= e
    }).length
    const completedThisWeek = items.filter(i => {
      const cd = toDate(i.completedDate)
      return cd >= s && cd <= e
    }).length

    cumStarted += startedThisWeek
    cumCompleted += completedThisWeek

    wipWeekly.push({
      weekStart: isoDate(cur),
      weekEnd: isoDate(sundayOf(cur)),
      label: wl,
      minWip: minW,
      avgWip: Math.round(avgW * 10) / 10,
      maxWip: maxW,
      started: startedThisWeek,
      completed: completedThisWeek,
      delta: completedThisWeek - startedThisWeek,
      cumStarted,
      cumCompleted,
    })
    cur = addDays(cur, 7)
  }

  return {
    weeks,
    throughputPercentiles,
    throughputHistogram: tpHisto,
    avgDefectRate,
    avgUnplannedRate,
    cycleTimeItems: ctItems,
    cycleTimeWeekly,
    cycleTimePercentiles,
    storyCycleTimeP85,
    defectCycleTimeP85,
    cycleTimeHistogram: ctHisto,
    wipDaily,
    wipWeekly,
    dateRange: { from: isoDate(dateFrom), to: isoDate(dateTo) },
    totalItems: items.length,
  }
}

/* ── Empty fallback ──────────────────────────────────────────────────────── */

function emptyResult(): DashboardResult {
  const z: Percentiles = { min: 0, p5: 0, p25: 0, p50: 0, p75: 0, p95: 0, max: 0 }
  return {
    weeks: [],
    throughputPercentiles: z,
    throughputHistogram: [],
    avgDefectRate: 0,
    avgUnplannedRate: 0,
    cycleTimeItems: [],
    cycleTimeWeekly: [],
    cycleTimePercentiles: z,
    storyCycleTimeP85: 0,
    defectCycleTimeP85: 0,
    cycleTimeHistogram: [],
    wipDaily: [],
    wipWeekly: [],
    dateRange: { from: '', to: '' },
    totalItems: 0,
  }
}

/* ── CSV parser utility ──────────────────────────────────────────────────── */

export function parseWorkItemsFromCSV(csv: string): WorkItem[] {
  const lines = csv.trim().split('\n')
  if (lines.length < 2) return []
  return lines.slice(1).map((line, idx) => {
    const parts = line.split(',').map(s => s.trim())
    return {
      id: String(idx + 1),
      completedDate: parts[0] || '',
      startDate: parts[1] || '',
      type: (['Planned', 'Unplanned', 'Defect'].includes(parts[2])
        ? parts[2]
        : 'Planned') as WorkItem['type'],
    }
  })
}
