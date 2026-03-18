import { createFileRoute } from '@tanstack/react-router'
import { useState, useEffect } from 'react'
import { runThroughputForecaster } from '#/lib/monte-carlo'
import type { ThroughputForecasterInputs, ThroughputForecasterResults } from '#/lib/monte-carlo'
import Field from '#/components/Field'
import NumberInput from '#/components/NumberInput'
import { Slider } from '#/components/ui/slider'

type ThroughputSearch = {
  startDate?: string
  storyLow?: number
  storyHigh?: number
  complexity?: number
  splitLow?: number
  splitHigh?: number
  durationDays?: number
  throughputMode?: 'estimate' | 'data'
  tpLow?: number
  tpMostLikely?: number
  tpHigh?: number
  samples?: string
  focus?: number
  weeksToForecast?: number
  numTrials?: number
}

export const Route = createFileRoute('/throughput')({
  validateSearch: (search): ThroughputSearch => ({
    startDate: readDateParam(search.startDate),
    storyLow: readNumberParam(search.storyLow),
    storyHigh: readNumberParam(search.storyHigh),
    complexity: readIntegerParam(search.complexity, 0, 3),
    splitLow: readNumberParam(search.splitLow),
    splitHigh: readNumberParam(search.splitHigh),
    durationDays: readAllowedNumberParam(search.durationDays, [7, 14, 21, 28]),
    throughputMode: readThroughputModeParam(search.throughputMode),
    tpLow: readNumberParam(search.tpLow),
    tpMostLikely: readNumberParam(search.tpMostLikely),
    tpHigh: readNumberParam(search.tpHigh),
    samples: readStringParam(search.samples),
    focus: readAllowedNumberParam(search.focus, [1, 0.75, 0.5, 0.25]),
    weeksToForecast: readIntegerParam(search.weeksToForecast, 1, 52),
    numTrials: readIntegerParam(search.numTrials, 1, 1000),
  }),
  component: ThroughputForecasterPage,
})

/* ── Setting tables (mirrored from spreadsheet Settings tab) ────────────── */

const DEFAULT_SAMPLES_TEXT = '1\n3\n5\n3\n7\n8'

const QUERY_PARAM_DOCS = [
  ['startDate', 'Optional start date in YYYY-MM-DD format.'],
  ['storyLow, storyHigh', 'Remaining story-count range before complexity adjustment.'],
  ['complexity', 'Complexity preset index: 0-3.'],
  ['splitLow, splitHigh', 'Story split-rate range.'],
  ['durationDays', 'Throughput unit size: 7, 14, 21, or 28 days.'],
  ['throughputMode', 'Use estimate or data.'],
  ['tpLow, tpMostLikely, tpHigh', 'Estimate-mode throughput inputs.'],
  ['samples', 'Historical throughput samples, comma-separated or URL-encoded newlines.'],
  ['focus', 'Team focus: 1, 0.75, 0.5, or 0.25.'],
  ['weeksToForecast', 'Story-count forecast window, from 1 to 52 intervals.'],
  ['numTrials', 'Simulation trials, from 1 to 1000.'],
] as const

const QUERY_PARAM_EXAMPLES = [
  {
    title: 'Estimate mode with a likely throughput value',
    description: 'Prefills the form with a weekly estimate-based scenario and a start date.',
    search:
      '?startDate=2026-04-01&storyLow=18&storyHigh=26&complexity=1&splitLow=1&splitHigh=1.4&durationDays=7&throughputMode=estimate&tpLow=3&tpMostLikely=5&tpHigh=8&focus=0.75&weeksToForecast=6&numTrials=750',
  },
  {
    title: 'Historical-data mode with 2-week throughput samples',
    description: 'Uses historical samples and a 2-week unit to exercise the data-backed path.',
    search:
      '?startDate=2026-04-14&storyLow=24&storyHigh=32&complexity=2&splitLow=1.1&splitHigh=1.8&durationDays=14&throughputMode=data&samples=2,3,4,5,3,6,4,5&focus=0.5&weeksToForecast=4&numTrials=600',
  },
] as const

const DURATION_OPTIONS = [
  { label: '1 week', days: 7 },
  { label: '2 weeks', days: 14 },
  { label: '3 weeks', days: 21 },
  { label: '4 weeks', days: 28 },
]

const COMPLEXITY_OPTIONS: {
  label: string
  lowMult: number
  highMult: number
}[] = [
  { label: 'Clear and understood', lowMult: 1, highMult: 1 },
  { label: 'Somewhat understood', lowMult: 1, highMult: 1.5 },
  { label: 'Not really understood yet', lowMult: 1.5, highMult: 2 },
  { label: 'Very unclear', lowMult: 1.75, highMult: 3 },
]

const FOCUS_OPTIONS = [
  { label: '100% (only this work)', value: 1 },
  { label: '75% (mostly this work)', value: 0.75 },
  { label: '50% (half this work)', value: 0.5 },
  { label: '25% (some of this work)', value: 0.25 },
]

/* ── Default risk row ────────────────────────────────────────────────────── */
interface Risk {
  likelihood: number
  impactLow: number
  impactHigh: number
  description: string
}
const emptyRisk = (): Risk => ({
  likelihood: 0,
  impactLow: 0,
  impactHigh: 0,
  description: '',
})

function readFirstParam(value: unknown): string | undefined {
  if (typeof value === 'string') {
    return value
  }
  if (Array.isArray(value) && typeof value[0] === 'string') {
    return value[0]
  }
  return undefined
}

function readStringParam(value: unknown): string | undefined {
  const raw = readFirstParam(value)
  if (!raw) {
    return undefined
  }
  return raw
}

function readNumberParam(value: unknown): number | undefined {
  const raw = readFirstParam(value)
  if (!raw) {
    return undefined
  }
  const parsed = Number(raw)
  return Number.isFinite(parsed) ? parsed : undefined
}

function readIntegerParam(value: unknown, min: number, max: number): number | undefined {
  const parsed = readNumberParam(value)
  if (parsed == null) {
    return undefined
  }
  if (!Number.isInteger(parsed) || parsed < min || parsed > max) {
    return undefined
  }
  return parsed
}

function readAllowedNumberParam(
  value: unknown,
  allowedValues: readonly number[],
): number | undefined {
  const parsed = readNumberParam(value)
  if (parsed == null) {
    return undefined
  }
  return allowedValues.includes(parsed) ? parsed : undefined
}

function readDateParam(value: unknown): string | undefined {
  const raw = readFirstParam(value)
  if (!raw) {
    return undefined
  }
  return /^\d{4}-\d{2}-\d{2}$/.test(raw) ? raw : undefined
}

function readThroughputModeParam(value: unknown): 'estimate' | 'data' | undefined {
  const raw = readFirstParam(value)
  return raw === 'estimate' || raw === 'data' ? raw : undefined
}

function resolveDurationIndex(durationDays?: number): number {
  const match = DURATION_OPTIONS.findIndex((option) => option.days === durationDays)
  return match >= 0 ? match : 0
}

function resolveFocusIndex(focus?: number): number {
  const match = FOCUS_OPTIONS.findIndex((option) => option.value === focus)
  return match >= 0 ? match : 0
}

function normalizeSamplesText(samples?: string): string {
  if (!samples) {
    return DEFAULT_SAMPLES_TEXT
  }
  return samples
    .split(/[\n,]+/)
    .map((sample) => sample.trim())
    .filter(Boolean)
    .join('\n')
}

function resolveQueryState(search: ThroughputSearch) {
  return {
    startDate: search.startDate ?? '',
    storyLow: search.storyLow ?? 20,
    storyHigh: search.storyHigh ?? 25,
    complexity: search.complexity ?? 0,
    splitLow: search.splitLow ?? 1,
    splitHigh: search.splitHigh ?? 1.5,
    durationIdx: resolveDurationIndex(search.durationDays),
    throughputMode: search.throughputMode ?? 'estimate',
    tpLow: search.tpLow ?? 1,
    tpMostLikely: search.tpMostLikely ?? '',
    tpHigh: search.tpHigh ?? 10,
    samplesText: normalizeSamplesText(search.samples),
    focusIdx: resolveFocusIndex(search.focus),
    weeksToForecast: search.weeksToForecast ?? 6,
    numTrials: search.numTrials ?? 500,
  }
}

/* ── Page ────────────────────────────────────────────────────────────────── */

function ThroughputForecasterPage() {
  const search = Route.useSearch()
  const initialState = resolveQueryState(search)
  const {
    startDate: searchStartDate,
    storyLow: searchStoryLow,
    storyHigh: searchStoryHigh,
    complexity: searchComplexity,
    splitLow: searchSplitLow,
    splitHigh: searchSplitHigh,
    durationDays: searchDurationDays,
    throughputMode: searchThroughputMode,
    tpLow: searchTpLow,
    tpMostLikely: searchTpMostLikely,
    tpHigh: searchTpHigh,
    samples: searchSamples,
    focus: searchFocus,
    weeksToForecast: searchWeeksToForecast,
    numTrials: searchNumTrials,
  } = search

  // --- Input state ---
  const [startDate, setStartDate] = useState(initialState.startDate)
  const [storyLow, setStoryLow] = useState(initialState.storyLow)
  const [storyHigh, setStoryHigh] = useState(initialState.storyHigh)
  const [complexity, setComplexity] = useState(initialState.complexity)
  const [splitLow, setSplitLow] = useState(initialState.splitLow)
  const [splitHigh, setSplitHigh] = useState(initialState.splitHigh)
  const [durationIdx, setDurationIdx] = useState(initialState.durationIdx)
  const [throughputMode, setThroughputMode] = useState<'estimate' | 'data'>(
    initialState.throughputMode,
  )
  const [tpLow, setTpLow] = useState(initialState.tpLow)
  const [tpMostLikely, setTpMostLikely] = useState<number | ''>(initialState.tpMostLikely)
  const [tpHigh, setTpHigh] = useState(initialState.tpHigh)
  const [samplesText, setSamplesText] = useState(initialState.samplesText)
  const [focusIdx, setFocusIdx] = useState(initialState.focusIdx)
  const [risks, setRisks] = useState<Risk[]>([
    emptyRisk(),
    emptyRisk(),
    emptyRisk(),
  ])
  const [weeksToForecast, setWeeksToForecast] = useState(initialState.weeksToForecast)
  const [numTrials, setNumTrials] = useState(initialState.numTrials)

  // --- Results state ---
  const [results, setResults] = useState<ThroughputForecasterResults | null>(null)
  const [running, setRunning] = useState(false)

  const comp = COMPLEXITY_OPTIONS[complexity]
  const dur = DURATION_OPTIONS[durationIdx]

  // Validation
  const storyError =
    storyLow > storyHigh
      ? 'Low guess must be ≤ high guess.'
      : ''
  const splitError =
    splitLow > splitHigh
      ? 'Low split must be ≤ high split.'
      : ''
  const tpError =
    throughputMode === 'estimate' && tpLow > tpHigh
      ? 'Low throughput must be ≤ high throughput.'
      : ''

  const canRun = !storyError && !splitError && !tpError

  useEffect(() => {
    const nextState = resolveQueryState({
      startDate: searchStartDate,
      storyLow: searchStoryLow,
      storyHigh: searchStoryHigh,
      complexity: searchComplexity,
      splitLow: searchSplitLow,
      splitHigh: searchSplitHigh,
      durationDays: searchDurationDays,
      throughputMode: searchThroughputMode,
      tpLow: searchTpLow,
      tpMostLikely: searchTpMostLikely,
      tpHigh: searchTpHigh,
      samples: searchSamples,
      focus: searchFocus,
      weeksToForecast: searchWeeksToForecast,
      numTrials: searchNumTrials,
    })
    setStartDate(nextState.startDate)
    setStoryLow(nextState.storyLow)
    setStoryHigh(nextState.storyHigh)
    setComplexity(nextState.complexity)
    setSplitLow(nextState.splitLow)
    setSplitHigh(nextState.splitHigh)
    setDurationIdx(nextState.durationIdx)
    setThroughputMode(nextState.throughputMode)
    setTpLow(nextState.tpLow)
    setTpMostLikely(nextState.tpMostLikely)
    setTpHigh(nextState.tpHigh)
    setSamplesText(nextState.samplesText)
    setFocusIdx(nextState.focusIdx)
    setWeeksToForecast(nextState.weeksToForecast)
    setNumTrials(nextState.numTrials)
    setRisks([emptyRisk(), emptyRisk(), emptyRisk()])
  }, [
    searchComplexity,
    searchDurationDays,
    searchFocus,
    searchNumTrials,
    searchSamples,
    searchSplitHigh,
    searchSplitLow,
    searchStartDate,
    searchStoryHigh,
    searchStoryLow,
    searchThroughputMode,
    searchTpHigh,
    searchTpLow,
    searchTpMostLikely,
    searchWeeksToForecast,
  ])

  /* ── Auto-run simulation on input change ───────────────────── */
  useEffect(() => {
    if (!canRun) {
      setResults(null)
      return
    }
    setRunning(true)
    const timer = setTimeout(() => {
      const comp = COMPLEXITY_OPTIONS[complexity]
      const focus = FOCUS_OPTIONS[focusIdx]
      const dur = DURATION_OPTIONS[durationIdx]

      // Parse samples
      const samples = samplesText
        .split(/[\n,]+/)
        .map((s) => Number(s.trim()))
        .filter((n) => !isNaN(n) && n > 0)

      const input: ThroughputForecasterInputs = {
        startDate: startDate || undefined,
        storyCountLow: storyLow,
        storyCountHigh: storyHigh,
        complexityLowMultiplier: comp.lowMult,
        complexityHighMultiplier: comp.highMult,
        splitRateLow: splitLow,
        splitRateHigh: splitHigh,
        throughputMode,
        throughputLow: tpLow,
        throughputHigh: tpHigh,
        throughputMostLikely: tpMostLikely === '' ? null : tpMostLikely,
        samples,
        focusPercentage: focus.value,
        daysPerUnit: dur.days,
        risks: risks.filter((r) => r.likelihood > 0),
        numTrials,
        maxPeriods: 104,
        weeksToForecast,
      }

      requestAnimationFrame(() => {
        const res = runThroughputForecaster(input)
        setResults(res)
        setRunning(false)
      })
    }, 300)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    canRun,
    startDate,
    storyLow,
    storyHigh,
    complexity,
    splitLow,
    splitHigh,
    throughputMode,
    tpLow,
    tpHigh,
    tpMostLikely,
    samplesText,
    focusIdx,
    durationIdx,
    risks,
    numTrials,
    weeksToForecast,
  ])

  const updateRisk = (idx: number, field: keyof Risk, value: string | number) => {
    setRisks((prev) => {
      const next = [...prev]
      next[idx] = { ...next[idx], [field]: value }
      return next
    })
  }

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Throughput Forecaster
        </h1>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Monte Carlo simulation for a single body of work. Estimates when work
          will complete and how many stories can be finished in a given time.
        </p>

        <div className="grid gap-8 lg:grid-cols-[1fr_1fr]">
          {/* ── LEFT – Inputs ────────────────────────────────────────── */}
          <div className="space-y-6">
            {/* 1. Start Date */}
            <Field label="1. Start Date (optional)">
              <input
                type="date"
                className="field-input"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </Field>

            {/* 2. Story Count */}
            <fieldset className="space-y-2">
              <legend className="field-legend">
                2. How many stories remaining?
              </legend>
              <div className="grid gap-4 md:grid-cols-3">
                <Field label="Low guess" inline>
                  <NumberInput value={storyLow} onChange={setStoryLow} min={1} />
                </Field>
                <Field label="High guess" inline>
                  <NumberInput value={storyHigh} onChange={setStoryHigh} min={1} />
                </Field>
                <Field label="Scope complexity" inline>
                  <select
                    className="field-input w-full"
                    value={complexity}
                    onChange={(e) => setComplexity(Number(e.target.value))}
                  >
                    {COMPLEXITY_OPTIONS.map((c, i) => (
                      <option key={c.label} value={i}>
                        {c.label}
                      </option>
                    ))}
                  </select>
                </Field>
              </div>
              {storyError && <p className="text-xs text-red-600">{storyError}</p>}
              <p className="text-xs text-[var(--sea-ink-soft)]">
                Adjusted range: {Math.round(storyLow * comp.lowMult)} –{' '}
                {Math.round(storyHigh * comp.highMult)}
              </p>
            </fieldset>

            {/* 3. Split Rate */}
            <fieldset className="space-y-2">
              <legend className="field-legend">3. Story split rate</legend>
              <div className="flex flex-wrap gap-4">
                <Field label="Low" inline>
                  <NumberInput
                    value={splitLow}
                    onChange={setSplitLow}
                    min={1}
                    step={0.1}
                  />
                </Field>
                <Field label="High" inline>
                  <NumberInput
                    value={splitHigh}
                    onChange={setSplitHigh}
                    min={1}
                    step={0.1}
                  />
                </Field>
              </div>
              {splitError && <p className="text-xs text-red-600">{splitError}</p>}
            </fieldset>

            {/* 4. Throughput */}
            <fieldset className="space-y-2">
              <legend className="field-legend">4. Throughput</legend>
              <div className="flex flex-wrap gap-4">
                <Field label="Unit" inline>
                  <select
                    className="field-input"
                    value={durationIdx}
                    onChange={(e) => setDurationIdx(Number(e.target.value))}
                  >
                    {DURATION_OPTIONS.map((d, i) => (
                      <option key={d.label} value={i}>
                        {d.label}
                      </option>
                    ))}
                  </select>
                </Field>
                <Field label="Source" inline>
                  <select
                    className="field-input"
                    value={throughputMode}
                    onChange={(e) =>
                      setThroughputMode(e.target.value as 'estimate' | 'data')
                    }
                  >
                    <option value="estimate">Estimate</option>
                    <option value="data">Historical Data</option>
                  </select>
                </Field>
              </div>

              {throughputMode === 'estimate' ? (
                <div className="grid gap-4 md:grid-cols-3">
                  <Field label="Worst case" inline>
                    <NumberInput value={tpLow} onChange={setTpLow} min={0} />
                  </Field>
                  <Field label="Most likely (optional)" inline>
                    <input
                      type="number"
                      className="field-input w-full"
                      value={tpMostLikely}
                      onChange={(e) =>
                        setTpMostLikely(
                          e.target.value === '' ? '' : Number(e.target.value),
                        )
                      }
                      placeholder="—"
                    />
                  </Field>
                  <Field label="Best case" inline>
                    <NumberInput value={tpHigh} onChange={setTpHigh} min={0} />
                  </Field>
                </div>
              ) : (
                <Field label="Historical samples (one per line or comma-separated)">
                  <textarea
                    className="field-input min-h-[5rem] font-mono text-xs"
                    value={samplesText}
                    onChange={(e) => setSamplesText(e.target.value)}
                  />
                </Field>
              )}
              {tpError && <p className="text-xs text-red-600">{tpError}</p>}
            </fieldset>

            {/* Focus */}
            <Field label="Team focus on THIS work">
              <select
                className="field-input"
                value={focusIdx}
                onChange={(e) => setFocusIdx(Number(e.target.value))}
              >
                {FOCUS_OPTIONS.map((f, i) => (
                  <option key={f.label} value={i}>
                    {f.label}
                  </option>
                ))}
              </select>
            </Field>

            {/* 5. Risks */}
            <fieldset className="space-y-2">
              <legend className="field-legend">5. Risks (optional)</legend>
              <div className="rounded-xl border border-[var(--line)] overflow-auto">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                      <th className="px-2 py-1 text-left">Likelihood</th>
                      <th className="px-2 py-1 text-left">Impact Low</th>
                      <th className="px-2 py-1 text-left">Impact High</th>
                      <th className="px-2 py-1 text-left">Description</th>
                    </tr>
                  </thead>
                  <tbody>
                    {risks.map((r, i) => (
                      <tr key={i} className="border-b border-[var(--line)] last:border-0">
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-16"
                            value={r.likelihood || ''}
                            min={0}
                            max={1}
                            step={0.05}
                            placeholder="0"
                            onChange={(e) =>
                              updateRisk(i, 'likelihood', Number(e.target.value))
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-16"
                            value={r.impactLow || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateRisk(i, 'impactLow', Number(e.target.value))
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-16"
                            value={r.impactHigh || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateRisk(i, 'impactHigh', Number(e.target.value))
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="text"
                            className="field-input w-full"
                            value={r.description}
                            placeholder="Risk description"
                            onChange={(e) =>
                              updateRisk(i, 'description', e.target.value)
                            }
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <button
                type="button"
                className="text-xs text-[var(--lagoon-deep)] hover:underline"
                onClick={() => setRisks((r) => [...r, emptyRisk()])}
              >
                + Add risk
              </button>
            </fieldset>

            {/* Status */}
            {!canRun && (
              <div className="flex items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-4 dark:border-amber-500/30 dark:bg-amber-950/30">
                <svg className="h-5 w-5 flex-shrink-0 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
                  <line x1="12" y1="9" x2="12" y2="13" />
                  <line x1="12" y1="17" x2="12.01" y2="17" />
                </svg>
                <div>
                  <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Missing required inputs</p>
                  <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                    {storyError || splitError || tpError}
                  </p>
                </div>
              </div>
            )}
            {running && (
              <p className="text-sm font-medium text-[var(--lagoon-deep)] animate-pulse">Simulating…</p>
            )}
          </div>

          {/* ── RIGHT – Results ───────────────────────────────────────── */}
          <div className="space-y-6">
            {results ? (
              <>
                {/* Completion percentiles */}
                <div className="island-shell rounded-2xl p-5">
                  <h2 className="mb-3 text-base font-semibold text-[var(--sea-ink)]">
                    Completion Forecast
                  </h2>
                  <div className="rounded-xl border border-[var(--line)] overflow-auto">
                    <table className="w-full text-xs">
                      <thead>
                        <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                          <th className="px-3 py-2 text-left">Likelihood</th>
                          <th className="px-3 py-2 text-right">
                            {dur.label} intervals
                          </th>
                          {startDate && (
                            <th className="px-3 py-2 text-right">Date</th>
                          )}
                        </tr>
                      </thead>
                      <tbody>
                        {results.completionPercentiles.map((row) => (
                          <tr
                            key={row.likelihood}
                            className="border-b border-[var(--line)] last:border-0"
                          >
                            <td className="px-3 py-1.5">
                              {(row.likelihood * 100).toFixed(0)}%
                            </td>
                            <td className="px-3 py-1.5 text-right font-mono">
                              {row.weeks}
                            </td>
                            {startDate && (
                              <td className="px-3 py-1.5 text-right">
                                {row.date}
                              </td>
                            )}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Histogram */}
                <div className="island-shell rounded-2xl p-5">
                  <h2 className="mb-3 text-base font-semibold text-[var(--sea-ink)]">
                    Completion Distribution
                  </h2>
                  <Histogram
                    data={results.completionHistogram}
                    numTrials={numTrials}
                    label={`${dur.label} intervals`}
                  />
                </div>

                {/* Simulation settings */}
                <div className="island-shell rounded-2xl p-5">
                  <div className="mb-3 flex items-center justify-between gap-4">
                    <h2 className="text-base font-semibold text-[var(--sea-ink)]">
                      Simulation Trials
                    </h2>
                    <span className="text-xs font-mono text-[var(--sea-ink-soft)]">
                      {numTrials}
                    </span>
                  </div>
                  <Slider
                    value={[numTrials]}
                    min={1}
                    max={1000}
                    step={1}
                    onValueChange={(value) => setNumTrials(value[0] ?? 500)}
                  />
                  <p className="mt-2 text-xs text-[var(--sea-ink-soft)]">
                    1–1000 trials (default 500)
                  </p>
                </div>

                <div className="h-px w-full bg-[var(--line)]" />

                {/* Story count forecast */}
                <div className="island-shell rounded-2xl p-5">
                  <h2 className="mb-3 text-base font-semibold text-[var(--sea-ink)]">
                    Story Count Forecast (separate from completion forecast)
                  </h2>
                  <div className="mb-4 grid gap-4 md:grid-cols-[auto_1fr] md:items-end">
                    <Field label={`How many ${dur.label} intervals?`} inline>
                      <NumberInput
                        value={weeksToForecast}
                        onChange={setWeeksToForecast}
                        min={1}
                        max={52}
                      />
                    </Field>
                    <p className="text-sm font-medium text-[var(--sea-ink)] md:text-right">
                      Story count in {weeksToForecast} {dur.label}
                      {weeksToForecast > 1 ? 's' : ''}
                    </p>
                  </div>
                  <h3 className="mb-3 text-sm font-semibold text-[var(--sea-ink)]">
                    Story Count in {weeksToForecast} {dur.label}
                    {weeksToForecast > 1 ? 's' : ''}
                  </h3>
                  <p className="mb-2 text-xs text-[var(--sea-ink-soft)]">
                    Pre-split story count (splitting IS accounted for)
                  </p>
                  <div className="rounded-xl border border-[var(--line)] overflow-auto">
                    <table className="w-full text-xs">
                      <thead>
                        <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                          <th className="px-3 py-2 text-left">Confidence</th>
                          <th className="px-3 py-2 text-right">Stories</th>
                        </tr>
                      </thead>
                      <tbody>
                        {results.storyCountPercentiles.map((row) => (
                          <tr
                            key={row.likelihood}
                            className="border-b border-[var(--line)] last:border-0"
                          >
                            <td className="px-3 py-1.5">
                              {(row.likelihood * 100).toFixed(0)}%
                            </td>
                            <td className="px-3 py-1.5 text-right font-mono">
                              {row.count}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </>
            ) : (
              <div className="flex h-full items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-6 dark:border-amber-500/30 dark:bg-amber-950/30">
                <svg className="h-6 w-6 flex-shrink-0 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
                  <line x1="12" y1="9" x2="12" y2="13" />
                  <line x1="12" y1="17" x2="12.01" y2="17" />
                </svg>
                <div>
                  <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Waiting for valid inputs</p>
                  <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                    Configure the inputs on the left. Results will appear here automatically.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>

        <div className="mt-10 border-t border-[var(--line)] pt-6">
          <details className="space-y-4">
            <summary className="field-legend cursor-pointer text-sm">
              URL Parameters (advanced)
            </summary>
            <p className="max-w-3xl text-xs text-[var(--sea-ink-soft)]">
              Append query parameters to prefill the form for testing or shared
              scenarios. This supports the main forecasting inputs. Risk rows are
              not currently populated from the URL.
            </p>

            <div className="rounded-2xl border border-[var(--line)] overflow-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                    <th className="px-3 py-2 text-left font-semibold">Parameter</th>
                    <th className="px-3 py-2 text-left font-semibold">Meaning</th>
                  </tr>
                </thead>
                <tbody>
                  {QUERY_PARAM_DOCS.map(([name, description]) => (
                    <tr key={name} className="border-b border-[var(--line)] last:border-0">
                      <td className="px-3 py-2 font-mono text-[var(--sea-ink)]">
                        {name}
                      </td>
                      <td className="px-3 py-2 text-[var(--sea-ink-soft)]">
                        {description}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
              {QUERY_PARAM_EXAMPLES.map((example) => (
                <div
                  key={example.title}
                  className="rounded-2xl border border-[var(--line)] bg-[var(--surface-strong)] p-4"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <h2 className="text-sm font-semibold text-[var(--sea-ink)]">
                        {example.title}
                      </h2>
                      <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                        {example.description}
                      </p>
                    </div>
                    <a
                      href={`/throughput${example.search}`}
                      className="rounded-full border border-[var(--line)] px-3 py-1 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:border-[var(--lagoon)] hover:text-[var(--sea-ink)]"
                    >
                      Open example
                    </a>
                  </div>
                  <p className="mt-3 break-all rounded-xl bg-[var(--header-bg)] px-3 py-2 font-mono text-[11px] text-[var(--sea-ink-soft)]">
                    /throughput{example.search}
                  </p>
                </div>
              ))}
            </div>
          </details>
        </div>
      </section>
    </main>
  )
}

/* ── Reusable sub-components ──────────────────────────────────────────────── */

function Histogram({
  data,
  numTrials,
  label,
}: {
  data: { week: number; count: number }[]
  numTrials: number
  label: string
}) {
  if (data.length === 0) return <p className="text-xs text-[var(--sea-ink-soft)]">No data</p>
  const maxCount = Math.max(...data.map((d) => d.count))

  // Compute running cumulative %
  let cum = 0
  const withCum = data.map((d) => {
    cum += d.count
    return { ...d, cumPct: cum / numTrials }
  })

  return (
    <div className="space-y-0.5">
      {withCum.map((d) => {
        const pct = (d.count / maxCount) * 100
        const isP85 = d.cumPct >= 0.84 && d.cumPct <= 0.86
        return (
          <div key={d.week} className="flex items-center gap-2 text-[10px]">
            <span className="w-8 text-right font-mono text-[var(--sea-ink-soft)]">
              {d.week}
            </span>
            <div className="relative flex-1 h-4">
              <div
                className={`h-full rounded-sm ${
                  isP85
                    ? 'bg-[rgba(79,184,178,0.6)]'
                    : 'bg-[rgba(79,184,178,0.25)]'
                }`}
                style={{ width: `${pct}%` }}
              />
            </div>
            <span className="w-8 text-right font-mono text-[var(--sea-ink-soft)]">
              {d.count}
            </span>
            <span className="w-10 text-right font-mono text-[var(--sea-ink-soft)]">
              {(d.cumPct * 100).toFixed(0)}%
            </span>
          </div>
        )
      })}
      <p className="mt-1 text-[10px] text-[var(--sea-ink-soft)]">
        {label} → frequency → cumulative %
      </p>
    </div>
  )
}
