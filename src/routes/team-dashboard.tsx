import { createFileRoute } from '@tanstack/react-router'
import { useState, useMemo, useCallback, useRef, useEffect } from 'react'
import {
  computeDashboard,
  parseWorkItemsFromCSV,
  DEFAULT_SETTINGS,
  type WorkItem,
  type DashboardSettings,
  type DashboardResult,
  type WeekBucket,
  type CycleTimeItem,
  type CycleTimeWeekly,
  type HistogramBin,
  type WipWeekly,
  type WipDay,
  type Percentiles,
} from '#/lib/team-dashboard'

export const Route = createFileRoute('/team-dashboard')({
  component: TeamDashboardPage,
})

/* ── Sample data (from spreadsheet) ──────────────────────────────────────── */

const SAMPLE_CSV = `Completed Date,Start Date,Type
2015-01-21,2015-01-14,Planned
2015-01-26,2015-01-14,Planned
2015-01-26,2015-01-14,Defect
2015-01-26,2015-01-21,Planned
2015-01-26,2015-01-22,Planned
2015-01-29,2015-01-23,Planned
2015-02-02,2015-01-23,Planned
2015-02-02,2015-01-20,Defect
2015-02-02,2015-01-20,Defect
2015-02-04,2015-01-20,Planned
2015-02-04,2015-01-26,Planned
2015-02-04,2015-01-23,Planned
2015-02-04,2015-01-22,Planned
2015-02-04,2015-01-28,Planned
2015-02-09,2015-02-02,Defect
2015-02-09,2015-02-05,Planned
2015-02-10,2015-02-05,Planned
2015-02-11,2015-02-02,Defect
2015-02-11,2015-02-05,Planned
2015-02-12,2015-02-09,Planned
2015-02-13,2015-02-09,Planned
2015-02-16,2015-02-09,Planned
2015-02-16,2015-02-10,Planned
2015-02-16,2015-02-10,Defect
2015-02-16,2015-02-09,Planned
2015-02-16,2015-02-10,Planned
2015-02-17,2015-02-10,Planned
2015-02-17,2015-02-11,Defect
2015-02-17,2015-02-12,Planned
2015-02-17,2015-02-11,Defect
2015-02-17,2015-02-11,Planned
2015-02-18,2015-02-11,Planned
2015-02-24,2015-02-16,Defect
2015-02-24,2015-02-16,Planned
2015-02-24,2015-02-16,Planned
2015-02-25,2015-02-19,Planned
2015-02-25,2015-02-23,Planned
2015-02-25,2015-02-23,Defect
2015-02-25,2015-02-23,Defect
2015-02-26,2015-02-24,Planned
2015-02-26,2015-02-25,Defect
2015-02-26,2015-02-25,Planned
2015-02-26,2015-02-25,Planned
2015-02-27,2015-02-18,Planned
2015-02-27,2015-02-24,Planned
2015-03-02,2015-02-26,Planned
2015-03-02,2015-02-27,Planned
2015-03-02,2015-02-25,Planned
2015-03-02,2015-02-27,Defect
2015-03-03,2015-02-26,Planned
2015-03-03,2015-02-24,Planned
2015-03-04,2015-02-26,Planned
2015-03-04,2015-03-02,Defect
2015-03-05,2015-02-26,Planned
2015-03-05,2015-02-27,Defect
2015-03-06,2015-03-02,Planned
2015-03-09,2015-03-04,Planned
2015-03-09,2015-03-02,Defect
2015-03-10,2015-03-03,Planned
2015-03-10,2015-03-05,Planned
2015-03-11,2015-03-04,Planned
2015-03-11,2015-03-05,Planned
2015-03-11,2015-03-06,Planned
2015-03-12,2015-03-09,Planned
2015-03-12,2015-03-05,Planned
2015-03-12,2015-03-09,Planned
2015-03-13,2015-03-10,Planned
2015-03-13,2015-03-06,Defect
2015-03-16,2015-03-10,Planned
2015-03-16,2015-03-11,Planned
2015-03-16,2015-03-12,Planned
2015-03-17,2015-03-12,Planned
2015-03-17,2015-03-12,Planned
2015-03-18,2015-03-16,Planned
2015-03-18,2015-03-13,Defect
2015-03-18,2015-03-10,Planned
2015-03-19,2015-03-13,Planned
2015-03-20,2015-03-16,Planned
2015-03-20,2015-03-17,Planned
2015-03-23,2015-03-17,Planned
2015-03-24,2015-03-18,Planned
2015-03-25,2015-03-19,Planned
2015-03-25,2015-03-18,Defect
2015-03-26,2015-03-20,Planned
2015-03-27,2015-03-23,Planned
2015-03-27,2015-03-20,Planned
2015-03-30,2015-03-27,Defect
2015-03-31,2015-03-23,Planned
2015-04-01,2015-03-26,Planned
2015-04-02,2015-03-30,Planned
2015-04-02,2015-03-27,Planned
2015-04-02,2015-03-25,Defect
2015-04-03,2015-03-31,Planned
2015-04-06,2015-04-01,Planned
2015-04-06,2015-04-01,Planned
2015-04-07,2015-03-31,Planned
2015-04-07,2015-04-01,Planned
2015-04-07,2015-04-02,Planned
2015-04-08,2015-04-06,Planned
2015-04-08,2015-04-02,Planned
2015-04-09,2015-04-02,Planned
2015-04-09,2015-04-06,Planned
2015-04-10,2015-04-03,Planned
2015-04-10,2015-04-06,Defect
2015-04-13,2015-04-07,Planned
2015-04-13,2015-04-08,Planned
2015-04-14,2015-04-09,Planned
2015-04-14,2015-04-08,Planned
2015-04-15,2015-04-10,Planned
2015-04-15,2015-04-08,Defect
2015-04-20,2015-04-13,Planned
2015-04-20,2015-04-13,Planned
2015-04-21,2015-04-14,Planned
2015-04-21,2015-04-15,Planned
2015-04-22,2015-04-14,Planned
2015-04-22,2015-04-20,Planned
2015-04-22,2015-04-15,Defect
2015-04-23,2015-04-20,Planned
2015-04-28,2015-04-21,Planned
2015-04-28,2015-04-22,Planned
2015-04-29,2015-04-22,Planned
2015-04-29,2015-04-23,Planned
2015-04-29,2015-04-23,Defect
2015-04-30,2015-04-27,Planned
2015-04-30,2015-04-28,Planned
2015-05-01,2015-04-28,Planned
2015-05-04,2015-04-29,Planned
2015-05-04,2015-04-30,Planned
2015-05-05,2015-04-30,Planned
2015-05-05,2015-05-01,Planned
2015-05-05,2015-04-30,Defect
2015-05-06,2015-05-01,Planned
2015-05-06,2015-05-04,Planned
2015-05-07,2015-05-04,Planned
2015-05-07,2015-05-05,Planned
2015-05-11,2015-05-05,Planned
2015-05-11,2015-05-06,Planned
2015-05-12,2015-05-06,Planned
2015-05-12,2015-05-07,Planned
2015-05-12,2015-05-07,Defect
2015-05-13,2015-05-07,Planned
2015-05-13,2015-05-11,Planned
2015-05-14,2015-05-06,Planned
2015-05-14,2015-05-11,Planned
2015-05-14,2015-05-12,Planned`

/* ── Page component ──────────────────────────────────────────────────────── */

function TeamDashboardPage() {
  /* ── State ─────────────────────────────────────────────────────────── */
  const [items, setItems] = useState<WorkItem[]>(() => parseWorkItemsFromCSV(SAMPLE_CSV))
  const [settings, setSettings] = useState<DashboardSettings>(DEFAULT_SETTINGS)
  const [csvText, setCsvText] = useState(SAMPLE_CSV)
  const [showDataEntry, setShowDataEntry] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  /* ── Derived ───────────────────────────────────────────────────────── */
  const result = useMemo(() => computeDashboard(items, settings), [items, settings])

  /* ── Auto-parse CSV on text change ────────────────────────────── */
  useEffect(() => {
    const timer = setTimeout(() => {
      const parsed = parseWorkItemsFromCSV(csvText)
      if (parsed.length > 0) setItems(parsed)
    }, 500)
    return () => clearTimeout(timer)
  }, [csvText])

  /* ── Handlers ──────────────────────────────────────────────────────── */
  const handleFileUpload = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = () => {
      const text = reader.result as string
      setCsvText(text)
      const parsed = parseWorkItemsFromCSV(text)
      if (parsed.length > 0) setItems(parsed)
    }
    reader.readAsText(file)
  }, [])

  const loadSample = useCallback(() => {
    setCsvText(SAMPLE_CSV)
    setItems(parseWorkItemsFromCSV(SAMPLE_CSV))
  }, [])

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Team Dashboard
        </h1>
        <p className="mb-6 max-w-3xl text-sm text-[var(--sea-ink-soft)]">
          Visualize your team's flow metrics across six dimensions of performance:
          Quality, Responsiveness, Productivity, Predictability, Value, and Sustainability.
          Enter completed date, start date, and type for each work item.
        </p>

        {/* ── Filters ────────────────────────────────────────────────── */}
        <div className="mb-6 flex flex-wrap items-end gap-4">
          <div>
            <label className="field-legend">Timespan</label>
            <select
              className="field-input w-36"
              value={settings.timespanMonths ?? 'all'}
              onChange={e => setSettings(s => ({
                ...s,
                timespanMonths: e.target.value === 'all' ? null : Number(e.target.value),
              }))}
            >
              <option value="1">1 Month</option>
              <option value="3">3 Months</option>
              <option value="6">6 Months</option>
              <option value="9">9 Months</option>
              <option value="12">12 Months</option>
              <option value="all">All</option>
            </select>
          </div>
          <div>
            <label className="field-legend">Work Type</label>
            <select
              className="field-input w-36"
              value={settings.typeFilter}
              onChange={e => setSettings(s => ({
                ...s,
                typeFilter: e.target.value as DashboardSettings['typeFilter'],
              }))}
            >
              <option value="All">All</option>
              <option value="Planned">Planned</option>
              <option value="Unplanned">Unplanned</option>
              <option value="Defect">Defect</option>
            </select>
          </div>
          <div className="ml-auto flex items-end gap-2">
            <button
              className="rounded-lg border border-[var(--line)] bg-[var(--surface-strong)] px-4 py-2 text-sm font-semibold text-[var(--sea-ink)] transition hover:bg-[var(--surface)]"
              onClick={() => setShowDataEntry(!showDataEntry)}
            >
              {showDataEntry ? 'Hide Data Entry' : 'Edit Data'}
            </button>
          </div>
        </div>

        {/* ── Date range info ────────────────────────────────────────── */}
        {result.dateRange.from && (
          <div className="mb-6 flex flex-wrap gap-4 text-xs text-[var(--sea-ink-soft)]">
            <span>Showing <strong className="text-[var(--sea-ink)]">{result.totalItems}</strong> items</span>
            <span>from <strong className="text-[var(--sea-ink)]">{result.dateRange.from}</strong> to <strong className="text-[var(--sea-ink)]">{result.dateRange.to}</strong></span>
            <span>across <strong className="text-[var(--sea-ink)]">{result.weeks.length}</strong> weeks</span>
          </div>
        )}

        {/* ── Data entry panel ───────────────────────────────────────── */}
        {showDataEntry && (
          <div className="mb-8 rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-5">
            <div className="mb-3 flex items-center justify-between">
              <h2 className="text-sm font-semibold text-[var(--sea-ink)]">Data Entry (CSV)</h2>
              <div className="flex gap-2">
                <button
                  className="rounded-lg bg-[rgba(79,184,178,0.14)] px-3 py-1.5 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:bg-[rgba(79,184,178,0.24)]"
                  onClick={loadSample}
                >
                  Load Sample
                </button>
                <input ref={fileInputRef} type="file" accept=".csv,.txt" className="hidden" onChange={handleFileUpload} />
                <button
                  className="rounded-lg bg-[rgba(79,184,178,0.14)] px-3 py-1.5 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:bg-[rgba(79,184,178,0.24)]"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Upload CSV
                </button>
              </div>
            </div>
            <p className="mb-2 text-xs text-[var(--sea-ink-soft)]">
              Format: <code className="rounded bg-[var(--surface)] px-1">Completed Date,Start Date,Type</code> where
              Type is Planned (default if blank), Unplanned, or Defect. Dates in YYYY-MM-DD format.
            </p>
            <textarea
              className="field-input min-h-[200px] font-mono text-xs leading-relaxed"
              value={csvText}
              onChange={(e) => setCsvText(e.target.value)}
            />
            <div className="mt-2 flex items-center gap-3">
              <span className="text-xs text-[var(--sea-ink-soft)]">
                {items.length} item{items.length !== 1 ? 's' : ''} loaded
              </span>
            </div>
          </div>
        )}

        {/* ── Six dimensions header ──────────────────────────────────── */}
        <SixDimensionCards result={result} />

        {/* ── Charts ─────────────────────────────────────────────────── */}
        <div className="mt-8 space-y-8">
          {/* Throughput run chart */}
          <ChartSection title="Weekly Throughput" subtitle="How much work is the team completing each week?">
            <ThroughputRunChart weeks={result.weeks} percentiles={result.throughputPercentiles} />
          </ChartSection>

          {/* Throughput histogram */}
          <ChartSection title="Throughput Distribution" subtitle="Frequency of weekly throughput counts">
            <HistogramChart bins={result.throughputHistogram} xLabel="Items per week" color="var(--lagoon)" />
          </ChartSection>

          {/* Cycle Time scatter */}
          <ChartSection title="Cycle Time Scatter Plot" subtitle="Calendar days from start to completion for each item">
            <CycleTimeScatter items={result.cycleTimeItems} p85Stories={result.storyCycleTimeP85} p85Defects={result.defectCycleTimeP85} />
          </ChartSection>

          {/* Cycle Time histogram */}
          <ChartSection title="Cycle Time Distribution" subtitle="How long do items typically take?">
            <HistogramChart bins={result.cycleTimeHistogram} xLabel="Calendar days" color="#6366f1" showStacked />
          </ChartSection>

          {/* Cycle Time by Week (box plot) */}
          <ChartSection title="Cycle Time by Week" subtitle="Weekly min/25th/median/75th/max range of cycle times">
            <CycleTimeBoxChart cycleTimeWeekly={result.cycleTimeWeekly} />
          </ChartSection>

          {/* Average Cycle Time per Week */}
          <ChartSection title="Average Cycle Time per Week" subtitle="Weekly average time from start to completion">
            <AvgCycleTimeChart cycleTimeWeekly={result.cycleTimeWeekly} />
          </ChartSection>

          {/* Defect / Unplanned Rate */}
          <ChartSection
            title="Defect Rate Over Time"
            subtitle={`Average defect rate: ${(result.avgDefectRate * 100).toFixed(1)}%`}
          >
            <DefectRateChart weeks={result.weeks} />
          </ChartSection>

          {/* Planned vs Unplanned Throughput */}
          <ChartSection title="Planned vs Unplanned Throughput" subtitle="Weekly breakdown of stories vs defects delivered">
            <PlannedVsUnplannedChart weeks={result.weeks} />
          </ChartSection>

          {/* WIP chart */}
          <ChartSection title="Work In Progress" subtitle="How many items are in flight each week?">
            <WipChart wipWeekly={result.wipWeekly} />
          </ChartSection>

          {/* Net Flow per Week */}
          <ChartSection title="Net Flow per Week" subtitle="Items completed minus items started each week (positive = clearing backlog)">
            <NetFlowChart wipWeekly={result.wipWeekly} />
          </ChartSection>

          {/* Started vs Completed per Week */}
          <ChartSection title="Started vs Completed per Week" subtitle="Side-by-side comparison of work entering and leaving the system">
            <StartedVsCompletedChart wipWeekly={result.wipWeekly} />
          </ChartSection>

          {/* Cumulative Flow */}
          <ChartSection title="Cumulative Flow" subtitle="Cumulative items started vs completed over time">
            <CumulativeFlowChart wipWeekly={result.wipWeekly} />
          </ChartSection>

          {/* Percentile summary cards */}
          <PercentileSummary
            throughput={result.throughputPercentiles}
            cycleTime={result.cycleTimePercentiles}
            storyCTP85={result.storyCycleTimeP85}
            defectCTP85={result.defectCycleTimeP85}
          />

          {/* WIP and Age by Day */}
          <ChartSection title="Work in Progress (WIP) and Age by Day" subtitle="Daily snapshot of in-progress items colored by how long they've been open">
            <WipAgeChart
              wipDaily={result.wipDaily}
              ageThresholds={settings.ageThresholds}
              wipWarningPct={settings.wipWarningPct}
              ageWarningPct={settings.ageWarningPct}
              onThresholdsChange={t => setSettings(s => ({ ...s, ageThresholds: t }))}
              onWipWarningChange={v => setSettings(s => ({ ...s, wipWarningPct: v }))}
              onAgeWarningChange={v => setSettings(s => ({ ...s, ageWarningPct: v }))}
            />
          </ChartSection>
        </div>
      </section>
    </main>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Six Dimension Cards                                                    */
/* ══════════════════════════════════════════════════════════════════════════ */

/* ── Linear regression for trendline ──────────────────────────────────── */

function linearRegression(ys: number[]): { slope: number; intercept: number } {
  const n = ys.length
  if (n < 2) return { slope: 0, intercept: ys[0] ?? 0 }
  let sx = 0, sy = 0, sxy = 0, sxx = 0
  for (let i = 0; i < n; i++) {
    sx += i; sy += ys[i]; sxy += i * ys[i]; sxx += i * i
  }
  const denom = n * sxx - sx * sx
  if (denom === 0) return { slope: 0, intercept: sy / n }
  const slope = (n * sxy - sx * sy) / denom
  const intercept = (sy - slope * sx) / n
  return { slope, intercept }
}

function trendDirection(ys: number[]): 'up' | 'down' | 'flat' {
  if (ys.length < 2) return 'flat'
  const { slope } = linearRegression(ys)
  const range = Math.max(...ys) - Math.min(...ys)
  // Consider "flat" if slope over the series produces less than 10% of the range
  if (range === 0 || Math.abs(slope * (ys.length - 1)) / range < 0.10) return 'flat'
  return slope > 0 ? 'up' : 'down'
}

/* ── Sparkline with trendline ────────────────────────────────────────── */

function Sparkline({ data, color, height = 48, invertGood }: {
  data: number[]
  color: string
  height?: number
  /** If true, "down" is good (green) and "up" is bad (red). Default: "up" is good. */
  invertGood?: boolean
}) {
  if (data.length < 2) {
    return (
      <div className="flex items-center justify-center text-[10px] text-[var(--sea-ink-soft)]" style={{ height }}>
        Not enough data
      </div>
    )
  }

  const W = 200
  const H = height
  const pad = { top: 4, bottom: 4, left: 2, right: 2 }
  const plotW = W - pad.left - pad.right
  const plotH = H - pad.top - pad.bottom

  const minV = Math.min(...data)
  const maxV = Math.max(...data)
  const range = maxV - minV || 1

  const xScale = (i: number) => pad.left + (i / (data.length - 1)) * plotW
  const yScale = (v: number) => pad.top + plotH - ((v - minV) / range) * plotH

  // Data line
  const pts = data.map((v, i) => `${xScale(i)},${yScale(v)}`).join(' ')

  // Fill area
  const fillPts = `${pts} ${xScale(data.length - 1)},${yScale(minV)} ${xScale(0)},${yScale(minV)}`

  // Trend line
  const { slope, intercept } = linearRegression(data)
  const trendY0 = intercept
  const trendY1 = intercept + slope * (data.length - 1)

  // Trend color: green=good, red=bad, gray=flat
  const dir = trendDirection(data)
  let trendColor = 'var(--sea-ink-soft)'
  if (dir === 'up') trendColor = invertGood ? '#ef4444' : '#10b981'
  if (dir === 'down') trendColor = invertGood ? '#10b981' : '#ef4444'

  // Arrow indicator
  const arrow = dir === 'up' ? '↑' : dir === 'down' ? '↓' : '→'

  return (
    <div className="relative">
      <svg viewBox={`0 0 ${W} ${H}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
        {/* Fill under curve */}
        <polygon points={fillPts} fill={color} opacity="0.10" />

        {/* Data line */}
        <polyline points={pts} fill="none" stroke={color} strokeWidth="1.8" strokeLinejoin="round" strokeLinecap="round" />

        {/* Trendline */}
        <line
          x1={xScale(0)} y1={yScale(trendY0)}
          x2={xScale(data.length - 1)} y2={yScale(trendY1)}
          stroke={trendColor} strokeWidth="1.5" strokeDasharray="4 3" opacity="0.85"
        />

        {/* End dot */}
        <circle cx={xScale(data.length - 1)} cy={yScale(data[data.length - 1])} r="2.5" fill={color} />
      </svg>

      {/* Trend arrow badge */}
      <span
        className="absolute right-1 top-0 text-sm font-bold leading-none"
        style={{ color: trendColor }}
        title={`Trend: ${dir}`}
      >
        {arrow}
      </span>
    </div>
  )
}

/* ── Six Dimension Cards ─────────────────────────────────────────────── */

function SixDimensionCards({ result }: { result: DashboardResult }) {
  // Build per-week series for each dimension
  const qualitySeries = result.weeks.map(w => w.defectRate * 100)

  // Cycle time: use weekly median. Fall back to empty.
  const ctWeekMap = new Map(result.cycleTimeWeekly.map(cw => [cw.label, cw.median]))
  const responseSeries = result.weeks.map(w => ctWeekMap.get(w.label) ?? 0).filter((_, i) =>
    ctWeekMap.has(result.weeks[i].label)
  )

  const productivitySeries = result.weeks.map(w => w.throughput)

  // Predictability: weekly delta (completed - started). Positive = good.
  const wipWeekMap = new Map(result.wipWeekly.map(ww => [ww.label, ww]))
  const predictabilitySeries = result.weeks.map(w => wipWeekMap.get(w.label)?.delta ?? 0)

  // WIP trend for sustainability (lower is more sustainable)
  const wipSeries = result.weeks.map(w => wipWeekMap.get(w.label)?.avgWip ?? 0)

  const dims = [
    {
      title: 'Quality',
      motto: '"Do It Right"',
      color: '#ef4444',
      value: `${(result.avgDefectRate * 100).toFixed(1)}%`,
      label: 'Avg defect rate',
      advice: 'How much defect debt do we carry that impedes consistent delivery? Should be level, not up OR down!',
      series: qualitySeries,
      invertGood: true,  // lower defect rate = better
    },
    {
      title: 'Responsiveness',
      motto: '"Do It Fast"',
      color: '#f59e0b',
      value: `${result.cycleTimePercentiles.p50.toFixed(1)}d`,
      label: 'Median cycle time',
      advice: 'How fast do we deliver from starting something to finishing it? Look for trends.',
      series: responseSeries,
      invertGood: true,  // lower cycle time = better
    },
    {
      title: 'Productivity',
      motto: '"Do Lots"',
      color: '#10b981',
      value: `${result.throughputPercentiles.p50.toFixed(0)}`,
      label: 'Median weekly throughput',
      advice: 'What pace do we deliver work? Look for ways to increase. Watch quality & predictability if you overdrive!',
      series: productivitySeries,
      invertGood: false, // higher throughput = better
    },
    {
      title: 'Predictability',
      motto: '"Do It Predictably"',
      color: '#6366f1',
      value: result.wipWeekly.length > 0
        ? `${result.wipWeekly.filter(w => w.delta >= 0).length}/${result.wipWeekly.length}`
        : '—',
      label: 'Weeks completing ≥ started',
      advice: 'How consistent is our completion pace? Complete something in preference to starting something new.',
      series: predictabilitySeries,
      invertGood: false, // positive delta is good
    },
    {
      title: 'Value / Impact',
      motto: '"Do the Right Stuff"',
      color: '#8b5cf6',
      value: '—',
      label: 'Measure customer outcome',
      advice: 'What should you measure to make sure customers are seeing an outcome in what you are delivering?',
      series: [] as number[],
      invertGood: false,
    },
    {
      title: 'Sustainability',
      motto: 'Maintain The Pace',
      color: '#ec4899',
      value: result.wipWeekly.length > 0
        ? `${(wipSeries.reduce((a, b) => a + b, 0) / wipSeries.length).toFixed(1)}`
        : '—',
      label: 'Avg WIP (lower = sustainable)',
      advice: 'Are team members happy? Working at a sustainable pace? About to take on unfamiliar challenges?',
      series: wipSeries,
      invertGood: true, // lower WIP = more sustainable
    },
  ]

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {dims.map(d => (
        <div key={d.title} className="rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-4">
          <div className="mb-1 flex items-center gap-2">
            <div className="h-3 w-3 rounded-full" style={{ background: d.color }} />
            <h3 className="text-sm font-bold text-[var(--sea-ink)]">{d.title}</h3>
            <span className="ml-auto text-xs italic text-[var(--sea-ink-soft)]">{d.motto}</span>
          </div>
          <div className="flex items-start gap-3">
            <div className="shrink-0">
              <div className="text-2xl font-bold text-[var(--sea-ink)]">{d.value}</div>
              <div className="text-[10px] text-[var(--sea-ink-soft)]">{d.label}</div>
            </div>
            <div className="min-w-0 flex-1">
              <Sparkline data={d.series} color={d.color} height={52} invertGood={d.invertGood} />
            </div>
          </div>
          <p className="m-0 mt-2 text-[11px] leading-relaxed text-[var(--sea-ink-soft)]">{d.advice}</p>
        </div>
      ))}
    </div>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Chart wrapper                                                          */
/* ══════════════════════════════════════════════════════════════════════════ */

function ChartSection({ title, subtitle, children }: {
  title: string
  subtitle?: string
  children: React.ReactNode
}) {
  return (
    <div className="rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-5">
      <h2 className="mb-0.5 text-base font-bold text-[var(--sea-ink)]">{title}</h2>
      {subtitle && <p className="mb-4 text-xs text-[var(--sea-ink-soft)]">{subtitle}</p>}
      {children}
    </div>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   SVG chart constants                                                    */
/* ══════════════════════════════════════════════════════════════════════════ */

const CHART_H = 260
const CHART_PAD = { top: 20, right: 20, bottom: 50, left: 55 }

function useChartWidth() {
  // responsive: use a ref and measure, but for simplicity default to 100%
  return 800 // We'll use viewBox scaling
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Throughput Run Chart                                                   */
/* ══════════════════════════════════════════════════════════════════════════ */

function ThroughputRunChart({ weeks, percentiles }: { weeks: WeekBucket[]; percentiles: Percentiles }) {
  if (weeks.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom
  const maxT = Math.max(...weeks.map(w => w.throughput), 1)
  const yMax = Math.ceil(maxT * 1.15)

  const barW = Math.max(4, plotW / weeks.length - 2)

  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / weeks.length)

  // Y-axis ticks
  const yTicks = buildTicks(0, yMax, 5)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {/* Grid */}
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* Percentile lines */}
      <PercentileLine y={yScale(percentiles.p50)} label="50th" color="#10b981" w={W} left={left} right={right} />
      <PercentileLine y={yScale(percentiles.p25)} label="25th" color="#f59e0b" w={W} left={left} right={right} />
      <PercentileLine y={yScale(percentiles.p75)} label="75th" color="#f59e0b" w={W} left={left} right={right} />

      {/* Bars */}
      {weeks.map((w, i) => {
        const storiesH = (w.stories / yMax) * plotH
        const defectsH = (w.defects / yMax) * plotH
        const x = xScale(i) - barW / 2
        return (
          <g key={w.label}>
            <title>{`${w.label}\nThroughput: ${w.throughput} (Stories: ${w.stories}, Defects: ${w.defects})\nDefect rate: ${(w.defectRate * 100).toFixed(0)}%`}</title>
            <rect x={x} y={yScale(w.stories + w.defects)} width={barW} height={storiesH + defectsH} rx={2} fill="var(--lagoon)" opacity={0.8} />
            {w.defects > 0 && (
              <rect x={x} y={yScale(w.defects)} width={barW} height={defectsH} rx={0} fill="#ef4444" opacity={0.9} />
            )}
          </g>
        )
      })}

      {/* X labels */}
      {weeks.map((w, i) => (
        weeks.length <= 20 || i % Math.ceil(weeks.length / 15) === 0
      ) && (
        <text
          key={`xl-${w.label}`}
          x={xScale(weeks.indexOf(w))}
          y={pH - bottom + 16}
          textAnchor="middle"
          className="fill-[var(--sea-ink-soft)]"
          fontSize="9"
          transform={`rotate(-45 ${xScale(weeks.indexOf(w))} ${pH - bottom + 16})`}
        >
          {w.weekStart.slice(5)}
        </text>
      ))}

      {/* Axis lines */}
      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />

      {/* Y label */}
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Items / week</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Cycle Time Scatter Plot                                                */
/* ══════════════════════════════════════════════════════════════════════════ */

function CycleTimeScatter({ items, p85Stories, p85Defects }: {
  items: CycleTimeItem[]
  p85Stories: number
  p85Defects: number
}) {
  if (items.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H + 20
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxCT = Math.max(...items.map(i => i.cycleTime), 1)
  const yMax = Math.ceil(maxCT * 1.15)

  // X axis: dates
  const dates = items.map(i => toDateNum(i.completedDate))
  const xMin = Math.min(...dates)
  const xMax = Math.max(...dates)
  const xRange = xMax - xMin || 1

  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (d: number) => left + ((d - xMin) / xRange) * plotW

  const yTicks = buildTicks(0, yMax, 5)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {/* Grid */}
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* 85th percentile lines */}
      {p85Stories > 0 && (
        <PercentileLine y={yScale(p85Stories)} label={`Stories 85th: ${p85Stories.toFixed(1)}d`} color="#10b981" w={W} left={left} right={right} />
      )}
      {p85Defects > 0 && (
        <PercentileLine y={yScale(p85Defects)} label={`Defects 85th: ${p85Defects.toFixed(1)}d`} color="#ef4444" w={W} left={left} right={right} />
      )}

      {/* Dots */}
      {items.map((item, idx) => (
        <g key={idx}>
          <title>{`${item.completedDate}\nCycle: ${item.cycleTime}d | ${item.type}`}</title>
          <circle
            cx={xScale(toDateNum(item.completedDate))}
            cy={yScale(item.cycleTime)}
            r={3.5}
            fill={item.type === 'Defect' ? '#ef4444' : 'var(--lagoon)'}
            opacity={0.7}
          />
        </g>
      ))}

      {/* Axes */}
      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Calendar days</text>

      {/* Legend */}
      <circle cx={left + 10} cy={top + 8} r={4} fill="var(--lagoon)" />
      <text x={left + 18} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Planned/Unplanned</text>
      <circle cx={left + 140} cy={top + 8} r={4} fill="#ef4444" />
      <text x={left + 148} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Defect</text>
    </svg>
  )
}

function toDateNum(s: string): number {
  return new Date(s + 'T00:00:00').getTime()
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Histogram Chart (shared for throughput & cycle time)                   */
/* ══════════════════════════════════════════════════════════════════════════ */

function HistogramChart({ bins, xLabel, color, showStacked }: {
  bins: HistogramBin[]
  xLabel: string
  color: string
  showStacked?: boolean
}) {
  if (bins.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxF = Math.max(...bins.map(b => b.frequency), 1)
  const yMax = Math.ceil(maxF * 1.15)

  const barW = Math.max(4, plotW / bins.length - 2)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / bins.length)

  const yTicks = buildTicks(0, yMax, 5)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {bins.map((b, i) => {
        const x = xScale(i) - barW / 2
        if (showStacked && b.defects > 0) {
          const storiesH = (b.stories / yMax) * plotH
          const defectsH = (b.defects / yMax) * plotH
          return (
            <g key={b.bin}>
              <title>{`${b.bin}: ${b.frequency} (Stories: ${b.stories}, Defects: ${b.defects})`}</title>
              <rect x={x} y={yScale(b.frequency)} width={barW} height={storiesH + defectsH} rx={1} fill={color} opacity={0.8} />
              <rect x={x} y={yScale(b.defects)} width={barW} height={defectsH} rx={0} fill="#ef4444" opacity={0.9} />
            </g>
          )
        }
        return (
          <g key={b.bin}>
            <title>{`${b.bin}: ${b.frequency}`}</title>
            <rect x={x} y={yScale(b.frequency)} width={barW} height={(b.frequency / yMax) * plotH} rx={1} fill={color} opacity={0.8} />
          </g>
        )
      })}

      {/* X labels */}
      {bins.map((b, i) => (
        (bins.length <= 25 || i % Math.ceil(bins.length / 18) === 0) ? (
          <text key={b.bin} x={xScale(i)} y={pH - bottom + 14} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9">{b.bin}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={W / 2} y={pH - 4} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10">{xLabel}</text>
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Frequency</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Defect Rate Chart                                                      */
/* ══════════════════════════════════════════════════════════════════════════ */

function DefectRateChart({ weeks }: { weeks: WeekBucket[] }) {
  if (weeks.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const yMax = 1 // 100%
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / weeks.length)

  const avgRate = weeks.reduce((s, w) => s + w.defectRate, 0) / weeks.length

  const yTicks = [0, 0.25, 0.5, 0.75, 1]

  // Line path
  const linePts = weeks.map((w, i) => `${xScale(i)},${yScale(w.defectRate)}`).join(' ')

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{(t * 100).toFixed(0)}%</text>
        </g>
      ))}

      {/* Average line */}
      <PercentileLine y={yScale(avgRate)} label={`Avg: ${(avgRate * 100).toFixed(1)}%`} color="#ef4444" w={W} left={left} right={right} />

      {/* Line chart */}
      <polyline
        points={linePts}
        fill="none"
        stroke="#ef4444"
        strokeWidth="2"
        strokeLinejoin="round"
      />
      {weeks.map((w, i) => (
        <g key={w.label}>
          <title>{`${w.weekStart}: ${(w.defectRate * 100).toFixed(0)}% (${w.defects}/${w.throughput})`}</title>
          <circle cx={xScale(i)} cy={yScale(w.defectRate)} r={3} fill="#ef4444" />
        </g>
      ))}

      {/* X labels */}
      {weeks.map((w, i) => (
        (weeks.length <= 20 || i % Math.ceil(weeks.length / 15) === 0) ? (
          <text
            key={w.label}
            x={xScale(i)}
            y={pH - bottom + 16}
            textAnchor="middle"
            className="fill-[var(--sea-ink-soft)]"
            fontSize="9"
            transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}
          >
            {w.weekStart.slice(5)}
          </text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Defect %</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Net Flow per Week (bar chart — delta)                                  */
/* ══════════════════════════════════════════════════════════════════════════ */

function NetFlowChart({ wipWeekly }: { wipWeekly: WipWeekly[] }) {
  if (wipWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const deltas = wipWeekly.map(w => w.delta)
  const absMax = Math.max(...deltas.map(Math.abs), 1)
  const yMax = Math.ceil(absMax * 1.15)
  const yScale = (v: number) => top + plotH / 2 - (v / yMax) * (plotH / 2)
  const xScale = (i: number) => left + (i + 0.5) * (plotW / wipWeekly.length)
  const barW = Math.max(plotW / wipWeekly.length * 0.65, 2)

  const yTicks = buildTicks(-yMax, yMax, 6).filter(t => t !== 0)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {/* Grid */}
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}
      {/* Zero line */}
      <line x1={left} x2={W - right} y1={yScale(0)} y2={yScale(0)} stroke="var(--sea-ink-soft)" strokeWidth="1" />

      {/* Bars */}
      {wipWeekly.map((w, i) => {
        const h = Math.abs(w.delta / yMax) * (plotH / 2)
        const y = w.delta >= 0 ? yScale(0) - h : yScale(0)
        return (
          <g key={w.label}>
            <title>{`${w.weekStart}\nStarted: ${w.started}  Completed: ${w.completed}\nNet: ${w.delta >= 0 ? '+' : ''}${w.delta}`}</title>
            <rect
              x={xScale(i) - barW / 2}
              y={y}
              width={barW}
              height={Math.max(h, 0.5)}
              fill={w.delta >= 0 ? '#10b981' : '#ef4444'}
              opacity={0.85}
              rx={2}
            />
          </g>
        )
      })}

      {/* X labels */}
      {wipWeekly.map((w, i) => (
        (wipWeekly.length <= 20 || i % Math.ceil(wipWeekly.length / 15) === 0) ? (
          <text key={w.label} x={xScale(i)} y={pH - bottom + 16} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9" transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}>{w.weekStart.slice(5)}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Net Flow</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Started vs Completed per Week (paired bars)                            */
/* ══════════════════════════════════════════════════════════════════════════ */

function StartedVsCompletedChart({ wipWeekly }: { wipWeekly: WipWeekly[] }) {
  if (wipWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxVal = Math.max(...wipWeekly.map(w => Math.max(w.started, w.completed)), 1)
  const yMax = Math.ceil(maxVal * 1.15)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / wipWeekly.length)
  const halfBar = Math.max(plotW / wipWeekly.length * 0.3, 2)

  const yTicks = buildTicks(0, yMax, 5)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {wipWeekly.map((w, i) => {
        const cx = xScale(i)
        return (
          <g key={w.label}>
            <title>{`${w.weekStart}\nStarted: ${w.started}  Completed: ${w.completed}`}</title>
            {/* Started bar (left) */}
            <rect
              x={cx - halfBar - 0.5}
              y={yScale(w.started)}
              width={halfBar}
              height={yScale(0) - yScale(w.started)}
              fill="#f59e0b"
              opacity={0.85}
              rx={1}
            />
            {/* Completed bar (right) */}
            <rect
              x={cx + 0.5}
              y={yScale(w.completed)}
              width={halfBar}
              height={yScale(0) - yScale(w.completed)}
              fill="var(--lagoon)"
              opacity={0.85}
              rx={1}
            />
          </g>
        )
      })}

      {/* X labels */}
      {wipWeekly.map((w, i) => (
        (wipWeekly.length <= 20 || i % Math.ceil(wipWeekly.length / 15) === 0) ? (
          <text key={w.label} x={xScale(i)} y={pH - bottom + 16} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9" transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}>{w.weekStart.slice(5)}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Items</text>

      {/* Legend */}
      <rect x={left + 6} y={top + 2} width={10} height={10} rx={2} fill="#f59e0b" opacity={0.85} />
      <text x={left + 20} y={top + 11} className="fill-[var(--sea-ink-soft)]" fontSize="10">Started</text>
      <rect x={left + 76} y={top + 2} width={10} height={10} rx={2} fill="var(--lagoon)" opacity={0.85} />
      <text x={left + 90} y={top + 11} className="fill-[var(--sea-ink-soft)]" fontSize="10">Completed</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Planned vs Unplanned Throughput (line chart)                           */
/* ══════════════════════════════════════════════════════════════════════════ */

function PlannedVsUnplannedChart({ weeks }: { weeks: WeekBucket[] }) {
  if (weeks.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxTP = Math.max(...weeks.map(w => w.throughput), 1)
  const yMax = Math.ceil(maxTP * 1.15)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / weeks.length)
  const yTicks = buildTicks(0, yMax, 5)

  const totalPts = weeks.map((w, i) => `${xScale(i)},${yScale(w.throughput)}`).join(' ')
  const storyPts = weeks.map((w, i) => `${xScale(i)},${yScale(w.stories)}`).join(' ')
  const defectPts = weeks.map((w, i) => `${xScale(i)},${yScale(w.defects)}`).join(' ')

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* Lines */}
      <polyline points={totalPts} fill="none" stroke="var(--lagoon)" strokeWidth="2" strokeLinejoin="round" />
      <polyline points={storyPts} fill="none" stroke="#10b981" strokeWidth="1.5" strokeLinejoin="round" strokeDasharray="4 2" />
      <polyline points={defectPts} fill="none" stroke="#ef4444" strokeWidth="1.5" strokeLinejoin="round" strokeDasharray="4 2" />

      {weeks.map((w, i) => (
        <g key={w.label}>
          <title>{`${w.weekStart}\nTotal: ${w.throughput}  Stories: ${w.stories}  Defects: ${w.defects}`}</title>
          <circle cx={xScale(i)} cy={yScale(w.throughput)} r={3} fill="var(--lagoon)" />
          <circle cx={xScale(i)} cy={yScale(w.stories)} r={2} fill="#10b981" />
          <circle cx={xScale(i)} cy={yScale(w.defects)} r={2} fill="#ef4444" />
        </g>
      ))}

      {/* X labels */}
      {weeks.map((w, i) => (
        (weeks.length <= 20 || i % Math.ceil(weeks.length / 15) === 0) ? (
          <text key={w.label} x={xScale(i)} y={pH - bottom + 16} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9" transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}>{w.weekStart.slice(5)}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Items</text>

      {/* Legend */}
      <line x1={left + 10} x2={left + 26} y1={top + 8} y2={top + 8} stroke="var(--lagoon)" strokeWidth="2" />
      <text x={left + 30} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Total</text>
      <line x1={left + 66} x2={left + 82} y1={top + 8} y2={top + 8} stroke="#10b981" strokeWidth="1.5" strokeDasharray="4 2" />
      <text x={left + 86} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Stories</text>
      <line x1={left + 136} x2={left + 152} y1={top + 8} y2={top + 8} stroke="#ef4444" strokeWidth="1.5" strokeDasharray="4 2" />
      <text x={left + 156} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Defects</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Cycle Time by Week (box‐whisker)                                       */
/* ══════════════════════════════════════════════════════════════════════════ */

function CycleTimeBoxChart({ cycleTimeWeekly }: { cycleTimeWeekly: CycleTimeWeekly[] }) {
  if (cycleTimeWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxCT = Math.max(...cycleTimeWeekly.map(w => w.max), 1)
  const yMax = Math.ceil(maxCT * 1.15)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / cycleTimeWeekly.length)
  const boxW = Math.max(plotW / cycleTimeWeekly.length * 0.5, 4)
  const yTicks = buildTicks(0, yMax, 5)

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {cycleTimeWeekly.map((w, i) => {
        const cx = xScale(i)
        return (
          <g key={w.label}>
            <title>{`${w.weekStart}\nMin: ${w.min.toFixed(1)}  25th: ${w.p25.toFixed(1)}  Median: ${w.median.toFixed(1)}  75th: ${w.p75.toFixed(1)}  Max: ${w.max.toFixed(1)}`}</title>
            {/* Whisker: min to max */}
            <line x1={cx} x2={cx} y1={yScale(w.max)} y2={yScale(w.min)} stroke="#6366f1" strokeWidth="1" />
            {/* Min cap */}
            <line x1={cx - boxW / 4} x2={cx + boxW / 4} y1={yScale(w.min)} y2={yScale(w.min)} stroke="#6366f1" strokeWidth="1.5" />
            {/* Max cap */}
            <line x1={cx - boxW / 4} x2={cx + boxW / 4} y1={yScale(w.max)} y2={yScale(w.max)} stroke="#6366f1" strokeWidth="1.5" />
            {/* Box: 25th to 75th */}
            <rect
              x={cx - boxW / 2}
              y={yScale(w.p75)}
              width={boxW}
              height={Math.max(yScale(w.p25) - yScale(w.p75), 1)}
              fill="#6366f1"
              opacity={0.3}
              stroke="#6366f1"
              strokeWidth="1"
              rx={1}
            />
            {/* Median line */}
            <line x1={cx - boxW / 2} x2={cx + boxW / 2} y1={yScale(w.median)} y2={yScale(w.median)} stroke="#6366f1" strokeWidth="2" />
          </g>
        )
      })}

      {/* X labels */}
      {cycleTimeWeekly.map((w, i) => (
        (cycleTimeWeekly.length <= 20 || i % Math.ceil(cycleTimeWeekly.length / 15) === 0) ? (
          <text key={w.label} x={xScale(i)} y={pH - bottom + 16} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9" transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}>{w.weekStart.slice(5)}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Calendar days</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Average Cycle Time per Week (line chart)                               */
/* ══════════════════════════════════════════════════════════════════════════ */

function AvgCycleTimeChart({ cycleTimeWeekly }: { cycleTimeWeekly: CycleTimeWeekly[] }) {
  if (cycleTimeWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxAvg = Math.max(...cycleTimeWeekly.map(w => w.avg), 1)
  const yMax = Math.ceil(maxAvg * 1.25)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / cycleTimeWeekly.length)
  const yTicks = buildTicks(0, yMax, 5)

  const overallAvg = cycleTimeWeekly.reduce((s, w) => s + w.avg, 0) / cycleTimeWeekly.length
  const linePts = cycleTimeWeekly.map((w, i) => `${xScale(i)},${yScale(w.avg)}`).join(' ')

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* Overall average */}
      <PercentileLine y={yScale(overallAvg)} label={`Avg: ${overallAvg.toFixed(1)}d`} color="#6366f1" w={W} left={left} right={right} />

      {/* Line */}
      <polyline points={linePts} fill="none" stroke="#6366f1" strokeWidth="2" strokeLinejoin="round" />
      {cycleTimeWeekly.map((w, i) => (
        <g key={w.label}>
          <title>{`${w.weekStart}\nAvg CT: ${w.avg.toFixed(1)} days`}</title>
          <circle cx={xScale(i)} cy={yScale(w.avg)} r={3} fill="#6366f1" />
        </g>
      ))}

      {/* X labels */}
      {cycleTimeWeekly.map((w, i) => (
        (cycleTimeWeekly.length <= 20 || i % Math.ceil(cycleTimeWeekly.length / 15) === 0) ? (
          <text key={w.label} x={xScale(i)} y={pH - bottom + 16} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="9" transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}>{w.weekStart.slice(5)}</text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Calendar days</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   WIP Chart                                                              */
/* ══════════════════════════════════════════════════════════════════════════ */

function WipChart({ wipWeekly }: { wipWeekly: WipWeekly[] }) {
  if (wipWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxWip = Math.max(...wipWeekly.map(w => w.maxWip), 1)
  const yMax = Math.ceil(maxWip * 1.15)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / wipWeekly.length)

  const yTicks = buildTicks(0, yMax, 5)

  // Area (avg WIP)
  const areaTop = wipWeekly.map((w, i) => `${xScale(i)},${yScale(w.avgWip)}`).join(' ')
  const areaBot = wipWeekly.map((_, i) => `${xScale(wipWeekly.length - 1 - i)},${yScale(0)}`).join(' ')

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* Fill area */}
      <polygon points={`${areaTop} ${areaBot}`} fill="var(--lagoon)" opacity="0.15" />

      {/* Max WIP line */}
      <polyline
        points={wipWeekly.map((w, i) => `${xScale(i)},${yScale(w.maxWip)}`).join(' ')}
        fill="none" stroke="var(--lagoon)" strokeWidth="1" strokeDasharray="4 2" opacity="0.5"
      />

      {/* Avg WIP line */}
      <polyline
        points={areaTop}
        fill="none" stroke="var(--lagoon)" strokeWidth="2" strokeLinejoin="round"
      />
      {wipWeekly.map((w, i) => (
        <g key={w.label}>
          <title>{`${w.weekStart}\nAvg WIP: ${w.avgWip} | Max: ${w.maxWip}\nStarted: ${w.started} Completed: ${w.completed}`}</title>
          <circle cx={xScale(i)} cy={yScale(w.avgWip)} r={3} fill="var(--lagoon)" />
        </g>
      ))}

      {/* X labels */}
      {wipWeekly.map((w, i) => (
        (wipWeekly.length <= 20 || i % Math.ceil(wipWeekly.length / 15) === 0) ? (
          <text
            key={w.label}
            x={xScale(i)}
            y={pH - bottom + 16}
            textAnchor="middle"
            className="fill-[var(--sea-ink-soft)]"
            fontSize="9"
            transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}
          >
            {w.weekStart.slice(5)}
          </text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>WIP items</text>

      {/* Legend */}
      <line x1={W - right - 160} x2={W - right - 140} y1={top + 8} y2={top + 8} stroke="var(--lagoon)" strokeWidth="2" />
      <text x={W - right - 136} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="9">Avg WIP</text>
      <line x1={W - right - 80} x2={W - right - 60} y1={top + 8} y2={top + 8} stroke="var(--lagoon)" strokeWidth="1" strokeDasharray="4 2" />
      <text x={W - right - 56} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="9">Max WIP</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Cumulative Flow Chart                                                  */
/* ══════════════════════════════════════════════════════════════════════════ */

function CumulativeFlowChart({ wipWeekly }: { wipWeekly: WipWeekly[] }) {
  if (wipWeekly.length === 0) return <EmptyChart />
  const W = useChartWidth()
  const pH = CHART_H
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  const maxVal = Math.max(...wipWeekly.map(w => Math.max(w.cumStarted, w.cumCompleted)), 1)
  const yMax = Math.ceil(maxVal * 1.1)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const xScale = (i: number) => left + (i + 0.5) * (plotW / wipWeekly.length)

  const yTicks = buildTicks(0, yMax, 5)

  const startedPts = wipWeekly.map((w, i) => `${xScale(i)},${yScale(w.cumStarted)}`).join(' ')
  const completedPts = wipWeekly.map((w, i) => `${xScale(i)},${yScale(w.cumCompleted)}`).join(' ')

  return (
    <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
      {yTicks.map(t => (
        <g key={t}>
          <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
          <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
        </g>
      ))}

      {/* Area between started and completed */}
      <polygon
        points={`${startedPts} ${wipWeekly.map((_, i) => `${xScale(wipWeekly.length - 1 - i)},${yScale(wipWeekly[wipWeekly.length - 1 - i].cumCompleted)}`).join(' ')}`}
        fill="var(--lagoon)" opacity="0.1"
      />

      {/* Started line */}
      <polyline points={startedPts} fill="none" stroke="#f59e0b" strokeWidth="2" strokeLinejoin="round" />
      {/* Completed line */}
      <polyline points={completedPts} fill="none" stroke="var(--lagoon)" strokeWidth="2" strokeLinejoin="round" />

      {wipWeekly.map((w, i) => (
        <g key={w.label}>
          <title>{`${w.weekStart}\nStarted: ${w.cumStarted} | Completed: ${w.cumCompleted}`}</title>
          <circle cx={xScale(i)} cy={yScale(w.cumStarted)} r={2.5} fill="#f59e0b" />
          <circle cx={xScale(i)} cy={yScale(w.cumCompleted)} r={2.5} fill="var(--lagoon)" />
        </g>
      ))}

      {/* X labels */}
      {wipWeekly.map((w, i) => (
        (wipWeekly.length <= 20 || i % Math.ceil(wipWeekly.length / 15) === 0) ? (
          <text
            key={w.label}
            x={xScale(i)}
            y={pH - bottom + 16}
            textAnchor="middle"
            className="fill-[var(--sea-ink-soft)]"
            fontSize="9"
            transform={`rotate(-45 ${xScale(i)} ${pH - bottom + 16})`}
          >
            {w.weekStart.slice(5)}
          </text>
        ) : null
      ))}

      <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
      <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>Cumulative</text>

      {/* Legend */}
      <line x1={left + 10} x2={left + 26} y1={top + 8} y2={top + 8} stroke="#f59e0b" strokeWidth="2" />
      <text x={left + 30} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Started</text>
      <line x1={left + 80} x2={left + 96} y1={top + 8} y2={top + 8} stroke="var(--lagoon)" strokeWidth="2" />
      <text x={left + 100} y={top + 12} className="fill-[var(--sea-ink-soft)]" fontSize="10">Completed</text>
    </svg>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Percentile Summary                                                     */
/* ══════════════════════════════════════════════════════════════════════════ */

function PercentileSummary({ throughput, cycleTime, storyCTP85, defectCTP85 }: {
  throughput: Percentiles
  cycleTime: Percentiles
  storyCTP85: number
  defectCTP85: number
}) {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <div className="rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-5">
        <h3 className="mb-3 text-sm font-bold text-[var(--sea-ink)]">Throughput Percentiles (items/week)</h3>
        <PercentileTable p={throughput} unit="" />
      </div>
      <div className="rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-5">
        <h3 className="mb-3 text-sm font-bold text-[var(--sea-ink)]">Cycle Time Percentiles (days)</h3>
        <PercentileTable p={cycleTime} unit="d" />
        <div className="mt-3 flex gap-4 text-xs text-[var(--sea-ink-soft)]">
          <span>Stories 85th: <strong className="text-[var(--sea-ink)]">{storyCTP85.toFixed(1)}d</strong></span>
          <span>Defects 85th: <strong className="text-[var(--sea-ink)]">{defectCTP85.toFixed(1)}d</strong></span>
        </div>
      </div>
    </div>
  )
}

function PercentileTable({ p, unit }: { p: Percentiles; unit: string }) {
  const rows = [
    ['Min', p.min],
    ['5th', p.p5],
    ['25th', p.p25],
    ['50th (Median)', p.p50],
    ['75th', p.p75],
    ['95th', p.p95],
    ['Max', p.max],
  ] as const

  return (
    <table className="w-full text-xs">
      <tbody>
        {rows.map(([label, val]) => (
          <tr key={label} className="border-b border-[var(--line)] last:border-0">
            <td className="py-1.5 text-[var(--sea-ink-soft)]">{label}</td>
            <td className="py-1.5 text-right font-mono font-semibold text-[var(--sea-ink)]">
              {typeof val === 'number' ? val.toFixed(1) : val}{unit}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   WIP and Age by Day (stacked bar chart)                                 */
/* ══════════════════════════════════════════════════════════════════════════ */

const AGE_COLORS = [
  '#d4d4d8',  // light grey  — lowest
  '#fbbf24',  // yellow/gold — middle
  '#f97316',  // orange      — largest
  '#ef4444',  // red         — above largest
]

function ageBands(t: [number, number, number]) {
  return [
    { key: 'ageLE1'  as const, label: `Age ≤ ${t[0]}d`,  color: AGE_COLORS[0] },
    { key: 'ageLE7'  as const, label: `Age ≤ ${t[1]}d`,  color: AGE_COLORS[1] },
    { key: 'ageLE14' as const, label: `Age ≤ ${t[2]}d`,  color: AGE_COLORS[2] },
    { key: 'ageGT14' as const, label: `Age > ${t[2]}d`,  color: AGE_COLORS[3] },
  ]
}

interface WipAgeChartProps {
  wipDaily: WipDay[]
  ageThresholds: [number, number, number]
  wipWarningPct: number
  ageWarningPct: number
  onThresholdsChange: (t: [number, number, number]) => void
  onWipWarningChange: (v: number) => void
  onAgeWarningChange: (v: number) => void
}

function WipAgeChart({
  wipDaily, ageThresholds,
  wipWarningPct, ageWarningPct,
  onThresholdsChange, onWipWarningChange, onAgeWarningChange,
}: WipAgeChartProps) {
  const bands = useMemo(() => ageBands(ageThresholds), [ageThresholds])

  // Compute warning markers
  const warnings = useMemo(() => {
    const wipWarnings: { idx: number; value: number }[] = []
    const ageWarnings: { idx: number; value: number }[] = []
    for (let i = 1; i < wipDaily.length; i++) {
      const prev = wipDaily[i - 1]
      const cur = wipDaily[i]
      // WIP Warning: total WIP jumped > wipWarningPct from previous day
      if (prev.wip > 0 && (cur.wip - prev.wip) / prev.wip > wipWarningPct) {
        wipWarnings.push({ idx: i, value: cur.wip })
      }
      // Age Warning: count of items in top-2 buckets jumped > ageWarningPct
      const prevOld = prev.ageLE14 + prev.ageGT14
      const curOld = cur.ageLE14 + cur.ageGT14
      if (prevOld > 0 && (curOld - prevOld) / prevOld > ageWarningPct) {
        ageWarnings.push({ idx: i, value: curOld })
      }
    }
    return { wipWarnings, ageWarnings }
  }, [wipDaily, wipWarningPct, ageWarningPct])

  if (wipDaily.length === 0) return (
    <>
      <WipAgeSettings
        thresholds={ageThresholds} onThresholdsChange={onThresholdsChange}
        wipWarningPct={wipWarningPct} onWipWarningChange={onWipWarningChange}
        ageWarningPct={ageWarningPct} onAgeWarningChange={onAgeWarningChange}
      />
      <EmptyChart />
    </>
  )

  const W = useChartWidth()
  const pH = CHART_H + 30          // extra height for daily x-labels
  const { top, right, bottom, left } = CHART_PAD
  const plotW = W - left - right
  const plotH = pH - top - bottom

  // Determine maximum stacked total
  const maxTotal = Math.max(
    ...wipDaily.map(d => d.ageLE1 + d.ageLE7 + d.ageLE14 + d.ageGT14),
    1,
  )
  const yMax = Math.ceil(maxTotal * 1.1)
  const yScale = (v: number) => top + plotH - (v / yMax) * plotH
  const yTicks = buildTicks(0, yMax, 5)

  // Bar geometry — collapse to thin bars when many days
  const barGap = wipDaily.length > 120 ? 0 : wipDaily.length > 60 ? 0.5 : 1
  const barW = Math.max((plotW - barGap * wipDaily.length) / wipDaily.length, 1)
  const barCenter = (i: number) => left + i * (barW + barGap) + barW / 2

  return (
    <>
      <WipAgeSettings
        thresholds={ageThresholds} onThresholdsChange={onThresholdsChange}
        wipWarningPct={wipWarningPct} onWipWarningChange={onWipWarningChange}
        ageWarningPct={ageWarningPct} onAgeWarningChange={onAgeWarningChange}
      />
      <svg viewBox={`0 0 ${W} ${pH}`} className="h-auto w-full" preserveAspectRatio="xMidYMid meet">
        {/* Grid lines */}
        {yTicks.map(t => (
          <g key={t}>
            <line x1={left} x2={W - right} y1={yScale(t)} y2={yScale(t)} stroke="var(--line)" strokeWidth="0.5" />
            <text x={left - 6} y={yScale(t) + 4} textAnchor="end" className="fill-[var(--sea-ink-soft)]" fontSize="10">{t}</text>
          </g>
        ))}

        {/* Stacked bars */}
        {wipDaily.map((d, i) => {
          const x = left + i * (barW + barGap)
          let y0 = yScale(0) // bottom

          return (
            <g key={d.date}>
              <title>{`${d.date}\n≤${ageThresholds[0]}d: ${d.ageLE1}  ≤${ageThresholds[1]}d: ${d.ageLE7}  ≤${ageThresholds[2]}d: ${d.ageLE14}  >${ageThresholds[2]}d: ${d.ageGT14}\nTotal WIP: ${d.wip}`}</title>
              {bands.map(band => {
                const val = d[band.key]
                if (val === 0) return null
                const segH = (val / yMax) * plotH
                const segY = y0 - segH
                y0 = segY
                return (
                  <rect
                    key={band.key}
                    x={x}
                    y={segY}
                    width={barW}
                    height={segH}
                    fill={band.color}
                    opacity={0.85}
                  />
                )
              })}
            </g>
          )
        })}

        {/* WIP Warning markers — black outlined squares */}
        {warnings.wipWarnings.map(w => {
          const cx = barCenter(w.idx)
          const cy = yScale(w.value) - 6
          return (
            <g key={`wip-warn-${w.idx}`}>
              <title>{`WIP Warning: ${wipDaily[w.idx].date}\nWIP jumped to ${w.value}`}</title>
              <rect x={cx - 4} y={cy - 4} width={8} height={8} fill="none" stroke="#1e293b" strokeWidth="1.5" />
            </g>
          )
        })}

        {/* Age Warning markers — red outlined circles */}
        {warnings.ageWarnings.map(w => {
          const cx = barCenter(w.idx)
          const cy = yScale(w.value) - 6
          return (
            <g key={`age-warn-${w.idx}`}>
              <title>{`Age Warning: ${wipDaily[w.idx].date}\nOlder items jumped to ${w.value}`}</title>
              <circle cx={cx} cy={cy} r={4} fill="none" stroke="#ef4444" strokeWidth="1.5" />
            </g>
          )
        })}

        {/* X-axis labels — sample to avoid overcrowding */}
        {wipDaily.map((d, i) => {
          const labelInterval = Math.max(1, Math.ceil(wipDaily.length / 20))
          if (i % labelInterval !== 0) return null
          const x = left + i * (barW + barGap) + barW / 2
          return (
            <text
              key={d.date}
              x={x}
              y={pH - bottom + 16}
              textAnchor="middle"
              className="fill-[var(--sea-ink-soft)]"
              fontSize="8"
              transform={`rotate(-45 ${x} ${pH - bottom + 16})`}
            >
              {d.date.slice(5)}
            </text>
          )
        })}

        {/* Axes */}
        <line x1={left} x2={left} y1={top} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
        <line x1={left} x2={W - right} y1={pH - bottom} y2={pH - bottom} stroke="var(--sea-ink-soft)" strokeWidth="1" />
        <text x={14} y={pH / 2} textAnchor="middle" className="fill-[var(--sea-ink-soft)]" fontSize="10" transform={`rotate(-90 14 ${pH / 2})`}>In-Progress Items</text>

        {/* Legend — age bands + warning markers */}
        {(() => {
          const legendItems = [
            ...bands.slice().reverse().map(b => ({ type: 'band' as const, ...b })),
            { type: 'wip-warn' as const, label: 'WIP Warning', key: 'wip-warn' },
            { type: 'age-warn' as const, label: 'Age Warning', key: 'age-warn' },
          ]
          const lx = W - right - 170
          return legendItems.map((item, idx) => {
            const ly = top + 4 + idx * 16
            if (item.type === 'band') {
              return (
                <g key={item.key}>
                  <rect x={lx} y={ly} width={12} height={10} rx={2} fill={item.color} opacity={0.85} />
                  <text x={lx + 16} y={ly + 9} className="fill-[var(--sea-ink-soft)]" fontSize="9">{item.label}</text>
                </g>
              )
            }
            if (item.type === 'wip-warn') {
              return (
                <g key={item.key}>
                  <rect x={lx + 2} y={ly + 1} width={8} height={8} fill="none" stroke="#1e293b" strokeWidth="1.5" />
                  <text x={lx + 16} y={ly + 9} className="fill-[var(--sea-ink-soft)]" fontSize="9">{item.label}</text>
                </g>
              )
            }
            // age-warn
            return (
              <g key={item.key}>
                <circle cx={lx + 6} cy={ly + 5} r={4} fill="none" stroke="#ef4444" strokeWidth="1.5" />
                <text x={lx + 16} y={ly + 9} className="fill-[var(--sea-ink-soft)]" fontSize="9">{item.label}</text>
              </g>
            )
          })
        })()}
      </svg>
    </>
  )
}

/* ── WIP & Age Settings Panel ─────────────────────────────────────────── */

function WipAgeSettings({
  thresholds, onThresholdsChange,
  wipWarningPct, onWipWarningChange,
  ageWarningPct, onAgeWarningChange,
}: {
  thresholds: [number, number, number]
  onThresholdsChange: (t: [number, number, number]) => void
  wipWarningPct: number
  onWipWarningChange: (v: number) => void
  ageWarningPct: number
  onAgeWarningChange: (v: number) => void
}) {
  const [lo, mid, hi] = thresholds

  const updateThreshold = (idx: 0 | 1 | 2, raw: number) => {
    const next: [number, number, number] = [...thresholds]
    next[idx] = raw
    // Enforce lo < mid < hi
    if (idx === 0) {
      if (next[0] >= next[1]) next[1] = next[0] + 1
      if (next[1] >= next[2]) next[2] = next[1] + 1
    } else if (idx === 1) {
      if (next[1] <= next[0]) next[0] = Math.max(1, next[1] - 1)
      if (next[1] >= next[2]) next[2] = next[1] + 1
    } else {
      if (next[2] <= next[1]) next[1] = Math.max(1, next[2] - 1)
      if (next[1] <= next[0]) next[0] = Math.max(1, next[1] - 1)
    }
    onThresholdsChange(next)
  }

  const wipPctDisplay = Math.round(wipWarningPct * 100)
  const agePctDisplay = Math.round(ageWarningPct * 100)

  return (
    <div className="mb-4 space-y-3">
      {/* Age bucket thresholds */}
      <p className="text-xs font-semibold uppercase tracking-wide text-[var(--sea-ink-soft)]">Age Buckets</p>
      <div className="grid grid-cols-1 gap-x-6 gap-y-3 sm:grid-cols-3">
        {/* Lowest bucket */}
        <div className="flex items-center gap-3">
          <span className="inline-block h-3 w-3 rounded-sm" style={{ background: AGE_COLORS[0] }} />
          <label className="field-legend whitespace-nowrap">≤ {lo} day{lo !== 1 ? 's' : ''}</label>
          <input
            type="range" min={1} max={30} value={lo}
            onChange={e => updateThreshold(0, Number(e.target.value))}
            className="h-1.5 w-full cursor-pointer accent-gray-400"
          />
          <span className="w-6 text-right text-xs tabular-nums text-[var(--sea-ink-soft)]">{lo}</span>
        </div>

        {/* Middle bucket */}
        <div className="flex items-center gap-3">
          <span className="inline-block h-3 w-3 rounded-sm" style={{ background: AGE_COLORS[1] }} />
          <label className="field-legend whitespace-nowrap">≤ {mid} days</label>
          <input
            type="range" min={2} max={60} value={mid}
            onChange={e => updateThreshold(1, Number(e.target.value))}
            className="h-1.5 w-full cursor-pointer accent-yellow-400"
          />
          <span className="w-6 text-right text-xs tabular-nums text-[var(--sea-ink-soft)]">{mid}</span>
        </div>

        {/* Largest bucket */}
        <div className="flex items-center gap-3">
          <span className="inline-block h-3 w-3 rounded-sm" style={{ background: AGE_COLORS[2] }} />
          <label className="field-legend whitespace-nowrap">≤ {hi} days</label>
          <input
            type="range" min={3} max={90} value={hi}
            onChange={e => updateThreshold(2, Number(e.target.value))}
            className="h-1.5 w-full cursor-pointer accent-orange-500"
          />
          <span className="w-6 text-right text-xs tabular-nums text-[var(--sea-ink-soft)]">{hi}</span>
        </div>
      </div>

      {/* Warning thresholds */}
      <p className="text-xs font-semibold uppercase tracking-wide text-[var(--sea-ink-soft)]">Warning Thresholds</p>
      <div className="grid grid-cols-1 gap-x-6 gap-y-3 sm:grid-cols-2">
        {/* WIP Warning % */}
        <div className="flex items-center gap-3">
          <span className="inline-flex h-4 w-4 items-center justify-center">
            <svg width="10" height="10"><rect x="1" y="1" width="8" height="8" fill="none" stroke="#1e293b" strokeWidth="1.5" /></svg>
          </span>
          <label className="field-legend whitespace-nowrap">WIP Warning {wipPctDisplay}%</label>
          <input
            type="range" min={5} max={100} value={wipPctDisplay}
            onChange={e => onWipWarningChange(Number(e.target.value) / 100)}
            className="h-1.5 w-full cursor-pointer accent-slate-600"
          />
          <span className="w-8 text-right text-xs tabular-nums text-[var(--sea-ink-soft)]">{wipPctDisplay}%</span>
        </div>

        {/* Age Warning % */}
        <div className="flex items-center gap-3">
          <span className="inline-flex h-4 w-4 items-center justify-center">
            <svg width="10" height="10"><circle cx="5" cy="5" r="4" fill="none" stroke="#ef4444" strokeWidth="1.5" /></svg>
          </span>
          <label className="field-legend whitespace-nowrap">Age Warning {agePctDisplay}%</label>
          <input
            type="range" min={5} max={100} value={agePctDisplay}
            onChange={e => onAgeWarningChange(Number(e.target.value) / 100)}
            className="h-1.5 w-full cursor-pointer accent-red-500"
          />
          <span className="w-8 text-right text-xs tabular-nums text-[var(--sea-ink-soft)]">{agePctDisplay}%</span>
        </div>
      </div>
    </div>
  )
}

/* ══════════════════════════════════════════════════════════════════════════ */
/*   Shared SVG helpers                                                     */
/* ══════════════════════════════════════════════════════════════════════════ */

function PercentileLine({ y, label, color, w, left, right }: {
  y: number; label: string; color: string; w: number; left: number; right: number
}) {
  return (
    <g>
      <line x1={left} x2={w - right} y1={y} y2={y} stroke={color} strokeWidth="1" strokeDasharray="6 3" opacity="0.8" />
      <text x={w - right + 4} y={y + 4} className="fill-current" fill={color} fontSize="9">{label}</text>
    </g>
  )
}

function EmptyChart() {
  return (
    <div className="flex h-40 items-center justify-center text-sm text-[var(--sea-ink-soft)]">
      No data to display. Add work items above.
    </div>
  )
}

function buildTicks(min: number, max: number, count: number): number[] {
  const step = Math.ceil((max - min) / count)
  const ticks: number[] = []
  for (let v = min; v <= max; v += step) ticks.push(v)
  if (ticks[ticks.length - 1] < max) ticks.push(max)
  return ticks
}
