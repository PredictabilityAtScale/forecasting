import { createFileRoute } from '@tanstack/react-router'
import { useState, useEffect } from 'react'
import { runMultiFeatureForecaster } from '#/lib/monte-carlo'
import type { MultiFeatureInputs, MultiFeatureResults, Feature } from '#/lib/monte-carlo'
import Field from '#/components/Field'
import NumberInput from '#/components/NumberInput'

export const Route = createFileRoute('/multi-feature')({
  component: MultiFeatureForecasterPage,
})

/* ── Setting tables ──────────────────────────────────────────────────────── */

const DURATION_OPTIONS = [
  { label: 'Week', days: 7 },
  { label: 'Sprint (2 week)', days: 14 },
  { label: 'Sprint (3 week)', days: 21 },
  { label: 'Sprint (4 week)', days: 28 },
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

const MONTHS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
]

/* ── Feature row type ────────────────────────────────────────────────────── */

interface FeatureRow {
  name: string
  storyLow: number
  storyHigh: number
  complexityIdx: number
}

const emptyFeature = (i: number): FeatureRow => ({
  name: `Feature ${i + 1}`,
  storyLow: 0,
  storyHigh: 0,
  complexityIdx: 0,
})

/* ── Page ────────────────────────────────────────────────────────────────── */

function MultiFeatureForecasterPage() {
  // Inputs
  const [startDate, setStartDate] = useState('2025-03-01')
  const [targetDate, setTargetDate] = useState('2025-06-01')
  const [targetLikelihood, setTargetLikelihood] = useState(0.85)
  const [splitLow, setSplitLow] = useState(1)
  const [splitHigh, setSplitHigh] = useState(2)
  const [durationIdx, setDurationIdx] = useState(0)
  const [throughputMode, setThroughputMode] = useState<'estimate' | 'data'>(
    'estimate',
  )
  const [tpLow, setTpLow] = useState(5)
  const [tpHigh, setTpHigh] = useState(8)
  const [samplesText, setSamplesText] = useState('1\n3\n5\n3\n7\n8')
  const [focusIdx, setFocusIdx] = useState(0)
  const [monthMultipliers, setMonthMultipliers] = useState<number[]>(
    Array(12).fill(1),
  )
  const [features, setFeatures] = useState<FeatureRow[]>([
    { name: 'Feature 1', storyLow: 5, storyHigh: 10, complexityIdx: 0 },
    { name: 'Feature 2', storyLow: 8, storyHigh: 15, complexityIdx: 0 },
    { name: 'Feature 3', storyLow: 15, storyHigh: 25, complexityIdx: 0 },
    { name: 'Feature 4', storyLow: 20, storyHigh: 30, complexityIdx: 0 },
    { name: 'Feature 5', storyLow: 10, storyHigh: 40, complexityIdx: 0 },
  ])
  const [numTrials, setNumTrials] = useState(500)

  // Results
  const [results, setResults] = useState<MultiFeatureResults | null>(null)
  const [running, setRunning] = useState(false)

  const dur = DURATION_OPTIONS[durationIdx]

  const splitError =
    splitLow > splitHigh ? 'Low split must be ≤ high split.' : ''
  const tpError =
    throughputMode === 'estimate' && tpLow > tpHigh
      ? 'Low must be ≤ high.'
      : ''
  const hasActiveFeatures = features.some((f) => f.storyLow > 0 || f.storyHigh > 0)
  const canRun = !splitError && !tpError && startDate && targetDate && hasActiveFeatures

  /* ── Auto-run simulation on input change ───────────────────── */
  useEffect(() => {
    if (!canRun) {
      setResults(null)
      return
    }
    setRunning(true)
    const timer = setTimeout(() => {
      const dur = DURATION_OPTIONS[durationIdx]
      const focus = FOCUS_OPTIONS[focusIdx]
      const samples = samplesText
        .split(/[\n,]+/)
        .map((s) => Number(s.trim()))
        .filter((n) => !isNaN(n) && n > 0)

      const activeFeatures: Feature[] = features
        .filter((f) => f.storyLow > 0 || f.storyHigh > 0)
        .map((f) => {
          const c = COMPLEXITY_OPTIONS[f.complexityIdx]
          return {
            name: f.name,
            storyCountLow: f.storyLow,
            storyCountHigh: f.storyHigh,
            complexityLowMultiplier: c.lowMult,
            complexityHighMultiplier: c.highMult,
          }
        })

      const input: MultiFeatureInputs = {
        startDate,
        targetDate,
        targetLikelihood,
        splitRateLow: splitLow,
        splitRateHigh: splitHigh,
        throughputMode,
        throughputLow: tpLow,
        throughputHigh: tpHigh,
        samples,
        focusPercentage: focus.value,
        daysPerUnit: dur.days,
        monthlyAdjustments: monthMultipliers,
        features: activeFeatures,
        numTrials,
        maxPeriods: 104,
      }

      requestAnimationFrame(() => {
        const res = runMultiFeatureForecaster(input)
        setResults(res)
        setRunning(false)
      })
    }, 300)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    canRun,
    startDate,
    targetDate,
    targetLikelihood,
    splitLow,
    splitHigh,
    throughputMode,
    tpLow,
    tpHigh,
    samplesText,
    focusIdx,
    durationIdx,
    monthMultipliers,
    features,
    numTrials,
  ])

  const updateFeature = (
    idx: number,
    field: keyof FeatureRow,
    value: string | number,
  ) => {
    setFeatures((prev) => {
      const next = [...prev]
      next[idx] = { ...next[idx], [field]: value }
      return next
    })
  }

  const updateMonth = (idx: number, value: number) => {
    setMonthMultipliers((prev) => {
      const next = [...prev]
      next[idx] = value
      return next
    })
  }

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Multiple Feature Cut Line Forecaster
        </h1>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Monte Carlo simulation for multiple features in priority order.
          Determines which features can be completed by a target date and
          forecasts completion dates at a chosen confidence level.
        </p>

        <div className="space-y-8">
          {/* ── Top inputs row ─────────────────────────────────────────── */}
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <Field label="1. Start Date">
              <input
                type="date"
                className="field-input"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </Field>
            <Field label="2. Target Date">
              <input
                type="date"
                className="field-input"
                value={targetDate}
                onChange={(e) => setTargetDate(e.target.value)}
              />
            </Field>
            <Field label="3. Confidence Level">
              <select
                className="field-input"
                value={targetLikelihood}
                onChange={(e) => setTargetLikelihood(Number(e.target.value))}
              >
                {[0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5].map(
                  (v) => (
                    <option key={v} value={v}>
                      {(v * 100).toFixed(0)}%
                    </option>
                  ),
                )}
              </select>
            </Field>
            <Field label="Simulation trials">
              <NumberInput
                value={numTrials}
                onChange={setNumTrials}
                min={100}
                max={10000}
                step={100}
              />
            </Field>
          </div>

          {/* ── Split / Throughput / Focus row ─────────────────────────── */}
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <fieldset className="space-y-2">
              <legend className="field-legend">4. Story split rate</legend>
              <div className="flex gap-4">
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
              {splitError && (
                <p className="text-xs text-red-600">{splitError}</p>
              )}
            </fieldset>

            <fieldset className="space-y-2">
              <legend className="field-legend">5. Throughput</legend>
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
                <div className="flex gap-4">
                  <Field label="Worst" inline>
                    <NumberInput value={tpLow} onChange={setTpLow} min={0} />
                  </Field>
                  <Field label="Best" inline>
                    <NumberInput value={tpHigh} onChange={setTpHigh} min={0} />
                  </Field>
                </div>
              ) : (
                <Field label="Samples (one per line)">
                  <textarea
                    className="field-input min-h-[4rem] font-mono text-xs"
                    value={samplesText}
                    onChange={(e) => setSamplesText(e.target.value)}
                  />
                </Field>
              )}
              {tpError && <p className="text-xs text-red-600">{tpError}</p>}
            </fieldset>

            <Field label="Team focus">
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
          </div>

          {/* ── Features table ─────────────────────────────────────────── */}
          <fieldset className="space-y-2">
            <legend className="field-legend">
              6. Features / Epics (in priority order)
            </legend>
            <div className="rounded-xl border border-[var(--line)] overflow-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                    <th className="w-8 px-2 py-2 text-center">#</th>
                    <th className="px-2 py-2 text-left">Feature Name</th>
                    <th className="px-2 py-2 text-right">Low Stories</th>
                    <th className="px-2 py-2 text-right">High Stories</th>
                    <th className="px-2 py-2 text-left">Complexity</th>
                    {results && (
                      <>
                        <th className="px-2 py-2 text-right">
                          Complete by {dur.label}
                        </th>
                        <th className="px-2 py-2 text-right">
                          Forecast Date ({(targetLikelihood * 100).toFixed(0)}%
                          CI)
                        </th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {features.map((f, i) => {
                    const res = results?.features[i]
                    return (
                      <tr
                        key={i}
                        className="border-b border-[var(--line)] last:border-0"
                      >
                        <td className="px-2 py-1 text-center text-[var(--sea-ink-soft)]">
                          {i + 1}
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="text"
                            className="field-input w-full"
                            value={f.name}
                            onChange={(e) =>
                              updateFeature(i, 'name', e.target.value)
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-20 text-right"
                            value={f.storyLow || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'storyLow',
                                Number(e.target.value),
                              )
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-20 text-right"
                            value={f.storyHigh || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'storyHigh',
                                Number(e.target.value),
                              )
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <select
                            className="field-input"
                            value={f.complexityIdx}
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'complexityIdx',
                                Number(e.target.value),
                              )
                            }
                          >
                            {COMPLEXITY_OPTIONS.map((c, ci) => (
                              <option key={c.label} value={ci}>
                                {c.label}
                              </option>
                            ))}
                          </select>
                        </td>
                        {results && res && (
                          <>
                            <td className="px-2 py-1 text-right font-mono">
                              {res.intervalsAtLikelihood ?? '—'}
                            </td>
                            <td className="px-2 py-1 text-right">
                              <span
                                className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${statusClasses(res.status)}`}
                              >
                                {res.forecastDate ?? '—'}
                              </span>
                            </td>
                          </>
                        )}
                        {results && !res && (
                          <>
                            <td className="px-2 py-1 text-right text-[var(--sea-ink-soft)]">
                              —
                            </td>
                            <td className="px-2 py-1 text-right text-[var(--sea-ink-soft)]">
                              —
                            </td>
                          </>
                        )}
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
            <button
              type="button"
              className="text-xs text-[var(--lagoon-deep)] hover:underline"
              onClick={() =>
                setFeatures((prev) => [...prev, emptyFeature(prev.length)])
              }
            >
              + Add feature
            </button>
          </fieldset>

          {/* ── Monthly Throughput Adjustments ──────────────────────────── */}
          <details className="space-y-2">
            <summary className="field-legend cursor-pointer">
              7. Monthly Throughput Adjustments (optional)
            </summary>
            <p className="text-xs text-[var(--sea-ink-soft)]">
              Multiply throughput by a factor each month (e.g., 0.5 for December
              holidays).
            </p>
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 lg:grid-cols-6">
              {MONTHS.map((m, i) => (
                <Field key={m} label={m} inline>
                  <NumberInput
                    value={monthMultipliers[i]}
                    onChange={(v) => updateMonth(i, v)}
                    min={0}
                    step={0.1}
                  />
                </Field>
              ))}
            </div>
          </details>

          {/* ── Status ───────────────────────────────────────────────── */}
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
                  {splitError || tpError || (!startDate && 'Start date is required.') || (!targetDate && 'Target date is required.') || (!hasActiveFeatures && 'At least one feature needs story counts > 0.')}
                </p>
              </div>
            </div>
          )}
          {running && (
            <p className="text-sm font-medium text-[var(--lagoon-deep)] animate-pulse">Simulating…</p>
          )}

          {/* ── Results legend ──────────────────────────────────────────── */}
          {results && (
            <div className="island-shell rounded-2xl p-5">
              <h2 className="mb-3 text-base font-semibold text-[var(--sea-ink)]">
                Results Summary
              </h2>
              <p className="mb-3 text-xs text-[var(--sea-ink-soft)]">
                Start: {new Date(startDate).toLocaleDateString()} · Target:{' '}
                {new Date(targetDate).toLocaleDateString()} ·{' '}
                {(targetLikelihood * 100).toFixed(0)}% confidence · {numTrials}{' '}
                trials
              </p>
              <div className="flex gap-3 text-xs">
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-blue-200 dark:bg-blue-800" />
                  On or before target
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-amber-200 dark:bg-amber-800" />
                  Within 1 {dur.label}
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-red-200 dark:bg-red-800" />
                  After target
                </span>
              </div>

              {/* Gantt-style visual */}
              <div className="mt-4 space-y-1">
                {results.features.map((f, i) => (
                  <div key={i} className="flex items-center gap-2 text-xs">
                    <span className="w-28 truncate text-[var(--sea-ink-soft)]">
                      {f.name}
                    </span>
                    <div className="relative flex-1 h-5 rounded bg-[var(--line)]">
                      <GanttBar
                        intervals={f.intervalsAtLikelihood ?? 0}
                        maxIntervals={Math.max(
                          ...results.features.map(
                            (x) => x.intervalsAtLikelihood ?? 1,
                          ),
                        )}
                        status={f.status}
                      />
                    </div>
                    <span className="w-20 text-right text-[var(--sea-ink-soft)]">
                      {f.forecastDate}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </section>
    </main>
  )
}

/* ── Sub-components ──────────────────────────────────────────────────────── */

function statusClasses(status: 1 | 2 | 3): string {
  switch (status) {
    case 1:
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300'
    case 2:
      return 'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300'
    case 3:
      return 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300'
  }
}

function GanttBar({
  intervals,
  maxIntervals,
  status,
}: {
  intervals: number
  maxIntervals: number
  status: 1 | 2 | 3
}) {
  const pct = maxIntervals > 0 ? (intervals / maxIntervals) * 100 : 0
  const bg =
    status === 1
      ? 'bg-blue-400 dark:bg-blue-600'
      : status === 2
        ? 'bg-amber-400 dark:bg-amber-600'
        : 'bg-red-400 dark:bg-red-600'

  return (
    <div
      className={`absolute inset-y-0 left-0 rounded ${bg}`}
      style={{ width: `${Math.max(pct, 2)}%` }}
    />
  )
}
