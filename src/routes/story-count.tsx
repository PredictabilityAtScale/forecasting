import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useState, useCallback, useEffect } from 'react'
import { z } from 'zod'
import { runStoryCountForecaster } from '#/lib/monte-carlo'
import type {
  FeatureEstimate,
  StoryCountInputs,
  StoryCountResults,
} from '#/lib/monte-carlo'
import CopyLinkButton from '#/components/CopyLinkButton'

const featureEstimateSearchSchema = z.object({
  id: z.string(),
  name: z.string(),
  estimate: z.number().nullable(),
  actual: z.number().nullable(),
})

const storyCountSearchSchema = z.object({
  totalFeatureCount: z.number().int().optional(),
  splitLow: z.number().optional(),
  splitHigh: z.number().optional(),
  numTrials: z.number().int().optional(),
  featuresText: z.string().optional(),
})

export const Route = createFileRoute('/story-count')({
  validateSearch: (search) =>
    storyCountSearchSchema.parse({
      totalFeatureCount: parseSearchInteger(search.totalFeatureCount),
      splitLow: parseSearchNumber(search.splitLow),
      splitHigh: parseSearchNumber(search.splitHigh),
      numTrials: parseSearchInteger(search.numTrials),
      featuresText: encodeFeatureEstimates(
        parseFeatureEstimates(search.featuresText ?? search.features),
      ),
    }),
  component: StoryCountForecasterPage,
})

/* ── Helpers ─────────────────────────────────────────────────────────────── */

const DEFAULT_TOTAL_FEATURE_COUNT = 15
const DEFAULT_SPLIT_LOW = 1
const DEFAULT_SPLIT_HIGH = 1
const DEFAULT_NUM_TRIALS = 1000

function makeFeature(index: number, overrides?: Partial<FeatureEstimate>): FeatureEstimate {
  return {
    id: `F${index}`,
    name: `Feature ${index}`,
    estimate: null,
    actual: null,
    ...overrides,
  }
}

function makeDefaultFeatures(count = 10): FeatureEstimate[] {
  return Array.from({ length: count }, (_, index) => makeFeature(index + 1))
}

const DEFAULT_FEATURES = makeDefaultFeatures()

function parseSearchMaybeJson(value: unknown): unknown {
  if (typeof value !== 'string') return value
  try {
    return JSON.parse(value)
  } catch {
    return value
  }
}

function parseSearchNumber(value: unknown): number | undefined {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value)
    if (Number.isFinite(parsed)) return parsed
  }
  return undefined
}

function parseSearchInteger(value: unknown): number | undefined {
  const parsed = parseSearchNumber(value)
  return parsed != null ? Math.trunc(parsed) : undefined
}

function parseNullableNumber(value: unknown): number | null {
  if (value == null || value === '') return null
  return parseSearchNumber(value) ?? null
}

function decodeSearchToken(value: string): string {
  try {
    return decodeURIComponent(value)
  } catch {
    return value
  }
}

function encodeSearchToken(value: string): string {
  return encodeURIComponent(value)
}

function parseFeatureEstimates(value: unknown): FeatureEstimate[] | undefined {
  const parsed = parseSearchMaybeJson(value)
  if (Array.isArray(parsed)) {
    const features = parsed
      .map((entry) => {
        const candidate = parseSearchMaybeJson(entry)
        if (!candidate || typeof candidate !== 'object') return null

        const feature = candidate as Record<string, unknown>
        const result = featureEstimateSearchSchema.safeParse({
          id: typeof feature.id === 'string' ? feature.id : undefined,
          name: typeof feature.name === 'string' ? feature.name : undefined,
          estimate: parseNullableNumber(feature.estimate),
          actual: parseNullableNumber(feature.actual),
        })

        return result.success ? result.data : null
      })
      .filter((feature): feature is FeatureEstimate => feature != null)

    return features.length > 0 ? features : undefined
  }

  if (typeof parsed !== 'string' || parsed.trim() === '') return undefined

  const features = parsed
    .split('|')
    .map((row) => row.trim())
    .filter(Boolean)
    .map((row) => {
      const [idToken = '', nameToken = '', estimateToken = '', actualToken = ''] = row.split('~')
      const result = featureEstimateSearchSchema.safeParse({
        id: decodeSearchToken(idToken),
        name: decodeSearchToken(nameToken),
        estimate: parseNullableNumber(estimateToken),
        actual: parseNullableNumber(actualToken),
      })

      return result.success ? result.data : null
    })
    .filter((feature): feature is FeatureEstimate => feature != null)

  return features.length > 0 ? features : undefined
}

function encodeFeatureEstimates(features: FeatureEstimate[] | undefined): string | undefined {
  if (!features || features.length === 0) return undefined
  return features
    .map((feature) => {
      const estimate = feature.estimate == null ? '' : String(feature.estimate)
      const actual = feature.actual == null ? '' : String(feature.actual)
      return [
        encodeSearchToken(feature.id),
        encodeSearchToken(feature.name),
        estimate,
        actual,
      ].join('~')
    })
    .join('|')
}

function areFeaturesEqual(left: FeatureEstimate[], right: FeatureEstimate[]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function buildExampleSearch(params: Record<string, string>): string {
  const search = new URLSearchParams(params)
  return `?${search.toString()}`
}

function getNextFeatureIndex(features: FeatureEstimate[]): number {
  const highest = features.reduce((max, feature, index) => {
    const idMatch = feature.id.match(/(\d+)$/)
    const nameMatch = feature.name.match(/(\d+)$/)
    const candidate = Number(idMatch?.[1] ?? nameMatch?.[1] ?? index + 1)
    return Number.isFinite(candidate) ? Math.max(max, candidate) : max
  }, 0)

  return highest + 1
}

const QUERY_PARAM_DOCS = [
  ['totalFeatureCount', 'How many total features or epics to forecast.'],
  ['splitLow, splitHigh', 'Story split-rate range applied to sampled estimates.'],
  ['numTrials', 'Simulation trials, typically 100-10000.'],
  ['featuresText', 'Compact feature rows in the form id~name~estimate~actual, separated by |.'],
] as const

const QUERY_PARAM_EXAMPLES = [
  {
    title: 'Ten sampled features for a larger portfolio forecast',
    description:
      'Prefills a reference-class dataset with mixed actuals so the split-range guidance appears immediately.',
    search: buildExampleSearch({
      totalFeatureCount: '18',
      splitLow: '1',
      splitHigh: '1.4',
      numTrials: '1200',
      featuresText: encodeFeatureEstimates([
        { id: 'F1', name: 'Onboarding', estimate: 5, actual: 8 },
        { id: 'F2', name: 'Billing', estimate: 8, actual: 13 },
        { id: 'F3', name: 'Audit Trail', estimate: 3, actual: 5 },
        { id: 'F4', name: 'Notifications', estimate: 5, actual: 5 },
        { id: 'F5', name: 'Reporting', estimate: 13, actual: 21 },
        { id: 'F6', name: 'Workflow Rules', estimate: 8, actual: 13 },
        { id: 'F7', name: 'API Migration', estimate: 20, actual: 34 },
        { id: 'F8', name: 'Partner Sync', estimate: 13, actual: 13 },
      ]) ?? '',
    }),
  },
  {
    title: 'Minimal dataset for quick sanity checks',
    description:
      'Uses a shorter historical sample to exercise the basic forecast and stability messaging path.',
    search: buildExampleSearch({
      totalFeatureCount: '9',
      splitLow: '1',
      splitHigh: '1',
      numTrials: '800',
      featuresText: encodeFeatureEstimates([
        { id: 'A1', name: 'Search', estimate: 5, actual: 8 },
        { id: 'A2', name: 'Profiles', estimate: 8, actual: 8 },
        { id: 'A3', name: 'Exports', estimate: 3, actual: null },
        { id: 'A4', name: 'SSO', estimate: 13, actual: null },
      ]) ?? '',
    }),
  },
] as const

function StoryCountForecasterPage() {
  const search = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })

  const parsedFeatures = parseFeatureEstimates(search.featuresText) ?? DEFAULT_FEATURES

  // ── Input state ───────────────────────────────────────────────────────
  const [totalFeatureCount, setTotalFeatureCount] = useState(
    search.totalFeatureCount ?? DEFAULT_TOTAL_FEATURE_COUNT,
  )
  const [splitLow, setSplitLow] = useState(search.splitLow ?? DEFAULT_SPLIT_LOW)
  const [splitHigh, setSplitHigh] = useState(search.splitHigh ?? DEFAULT_SPLIT_HIGH)
  const [numTrials, setNumTrials] = useState(search.numTrials ?? DEFAULT_NUM_TRIALS)
  const [features, setFeatures] = useState<FeatureEstimate[]>(parsedFeatures)

  // ── Results state ─────────────────────────────────────────────────────
  const [results, setResults] = useState<StoryCountResults | null>(null)
  const [running, setRunning] = useState(false)

  // Sync URL → State only when the URL search params change (e.g. external
  // navigation, back/forward).  Functional setState avoids reading stale state
  // and keeps the dep array limited to `search`.
  useEffect(() => {
    const nextTotalFeatureCount = search.totalFeatureCount ?? DEFAULT_TOTAL_FEATURE_COUNT
    const nextSplitLow = search.splitLow ?? DEFAULT_SPLIT_LOW
    const nextSplitHigh = search.splitHigh ?? DEFAULT_SPLIT_HIGH
    const nextNumTrials = search.numTrials ?? DEFAULT_NUM_TRIALS
    const nextFeatures = parseFeatureEstimates(search.featuresText) ?? DEFAULT_FEATURES

    setTotalFeatureCount(prev => prev === nextTotalFeatureCount ? prev : nextTotalFeatureCount)
    setSplitLow(prev => prev === nextSplitLow ? prev : nextSplitLow)
    setSplitHigh(prev => prev === nextSplitHigh ? prev : nextSplitHigh)
    setNumTrials(prev => prev === nextNumTrials ? prev : nextNumTrials)
    setFeatures(prev => areFeaturesEqual(prev, nextFeatures) ? prev : nextFeatures)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search])

  useEffect(() => {
    const nextSearch = {
      totalFeatureCount,
      splitLow,
      splitHigh,
      numTrials,
      featuresText: encodeFeatureEstimates(features),
    }

    const currentSearch = {
      totalFeatureCount: search.totalFeatureCount,
      splitLow: search.splitLow,
      splitHigh: search.splitHigh,
      numTrials: search.numTrials,
      featuresText: search.featuresText,
    }

    if (JSON.stringify(currentSearch) === JSON.stringify(nextSearch)) return

    void navigate({ search: nextSearch, replace: true })
  }, [features, navigate, numTrials, search, splitHigh, splitLow, totalFeatureCount])

  // ── Derived counts ────────────────────────────────────────────────────
  const featuresEntered = features.filter(
    (f) => f.name.trim() !== '' || f.estimate != null,
  ).length
  const estimateCount = features.filter(
    (f) => f.estimate != null && f.estimate > 0,
  ).length

  // ── Validation ────────────────────────────────────────────────────────
  const splitError = splitLow > splitHigh ? 'Low split must be ≤ high split.' : ''
  const estimateError = estimateCount === 0 ? 'Enter at least one feature estimate.' : ''
  const featureCountError =
    totalFeatureCount < 1 ? 'Must forecast at least 1 feature.' : ''
  const canRun = !splitError && !estimateError && !featureCountError

  /* ── Auto-run simulation on input change ───────────────────── */
  useEffect(() => {
    if (!canRun) {
      setResults(null)
      return
    }
    setRunning(true)
    const timer = setTimeout(() => {
      requestAnimationFrame(() => {
        const input: StoryCountInputs = {
          totalFeatureCount,
          splitRateLow: splitLow,
          splitRateHigh: splitHigh,
          features,
          numTrials,
        }
        const res = runStoryCountForecaster(input)
        setResults(res)
        setRunning(false)
      })
    }, 300)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRun, totalFeatureCount, splitLow, splitHigh, features, numTrials])

  // ── Feature table handlers ────────────────────────────────────────
  const updateFeature = useCallback(
    (idx: number, patch: Partial<FeatureEstimate>) => {
      setFeatures((prev) =>
        prev.map((f, i) => (i === idx ? { ...f, ...patch } : f)),
      )
    },
    [],
  )
  const addRows = useCallback(
    (count: number) => {
      const nextIndex = getNextFeatureIndex(features)
      setFeatures((prev) => [
        ...prev,
        ...Array.from({ length: count }, (_, index) =>
          makeFeature(nextIndex + index),
        ),
      ])
    },
    [features],
  )
  const removeRow = useCallback((idx: number) => {
    setFeatures((prev) => prev.filter((_, i) => i !== idx))
  }, [])

  // ── Actual split range from data ──────────────────────────────────────
  const splitRates = features
    .filter(
      (f) =>
        f.estimate != null &&
        f.estimate > 0 &&
        f.actual != null &&
        f.actual > 0,
    )
    .map((f) => f.actual! / f.estimate!)
  const actualSplitRange =
    splitRates.length > 0
      ? { min: Math.min(...splitRates), max: Math.max(...splitRates) }
      : null

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="rise-in mb-10">
        <div className="flex items-start justify-between gap-4">
          <h1 className="display-title text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
            Story Count Forecaster
          </h1>
          <CopyLinkButton />
        </div>
        <p className="mt-2 max-w-2xl text-[var(--sea-ink-soft)]">
          Enter estimates for previously sized features or epics.
          The Monte Carlo simulation resamples from your data to project total
          story count for any number of features, accounting for work splitting.
        </p>
      </section>

      {/* ── Feature / Epic input table ────────────────────────────────── */}
      <section className="island-shell mb-8 rounded-2xl p-5 sm:p-7">
        <h2 className="mb-4 text-lg font-bold text-[var(--sea-ink)]">
          1. Enter Features or Epics
        </h2>
        <p className="mb-4 text-sm text-[var(--sea-ink-soft)]">
          Enter the estimated story count / points for each feature before
          starting. Optionally enter the actual count after completion to
          compute observed split rates.
        </p>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--line)] text-left text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                <th className="px-2 py-2 w-16">ID</th>
                <th className="px-2 py-2 min-w-[160px]">Feature / Epic Name</th>
                <th className="px-2 py-2 w-36">
                  Estimated Stories
                </th>
                <th className="px-2 py-2 w-36">
                  Actual Stories
                </th>
                <th className="px-2 py-2 w-12" />
              </tr>
            </thead>
            <tbody>
              {features.map((f, i) => (
                <tr
                  key={f.id + i}
                  className="border-b border-[var(--line)] last:border-b-0"
                >
                  <td className="px-2 py-1.5">
                    <input
                      className="field-input w-full"
                      value={f.id}
                      onChange={(e) =>
                        updateFeature(i, { id: e.target.value })
                      }
                    />
                  </td>
                  <td className="px-2 py-1.5">
                    <input
                      className="field-input w-full"
                      value={f.name}
                      onChange={(e) =>
                        updateFeature(i, { name: e.target.value })
                      }
                    />
                  </td>
                  <td className="px-2 py-1.5">
                    <input
                      type="number"
                      min={0}
                      className="field-input w-full"
                      value={f.estimate ?? ''}
                      onChange={(e) =>
                        updateFeature(i, {
                          estimate:
                            e.target.value === ''
                              ? null
                              : Number(e.target.value),
                        })
                      }
                    />
                  </td>
                  <td className="px-2 py-1.5">
                    <input
                      type="number"
                      min={0}
                      className="field-input w-full"
                      value={f.actual ?? ''}
                      onChange={(e) =>
                        updateFeature(i, {
                          actual:
                            e.target.value === ''
                              ? null
                              : Number(e.target.value),
                        })
                      }
                    />
                  </td>
                  <td className="px-2 py-1.5 text-center">
                    <button
                      className="text-[var(--sea-ink-soft)] hover:text-red-600 transition"
                      title="Remove row"
                      onClick={() => removeRow(i)}
                    >
                      &times;
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="mt-3 flex gap-2">
          <button
            onClick={() => addRows(1)}
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-3 py-1.5 text-xs font-semibold text-[var(--sea-ink-soft)] transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
          >
            + Add Row
          </button>
          <button
            onClick={() => addRows(5)}
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-3 py-1.5 text-xs font-semibold text-[var(--sea-ink-soft)] transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
          >
            + Add 5 Rows
          </button>
        </div>
      </section>

      {/* ── Forecast settings ─────────────────────────────────────────── */}
      <section className="island-shell mb-8 rounded-2xl p-5 sm:p-7">
        <h2 className="mb-4 text-lg font-bold text-[var(--sea-ink)]">
          2. Forecast Settings
        </h2>

        <div className="grid gap-x-8 gap-y-5 sm:grid-cols-2 lg:grid-cols-3">
          {/* Total Feature Count */}
          <div>
            <label className="field-legend">
              Total features to forecast
            </label>
            <input
              type="number"
              min={1}
              className="field-input"
              value={totalFeatureCount}
              onChange={(e) => setTotalFeatureCount(Number(e.target.value))}
            />
            {featureCountError && (
              <p className="mt-1 text-xs text-red-600">{featureCountError}</p>
            )}
            <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
              Features entered on input table:{' '}
              <span className="font-semibold">{featuresEntered}</span>
            </p>
          </div>

          {/* Split Rate */}
          <div>
            <label className="field-legend">
              Split rate (low guess)
            </label>
            <input
              type="number"
              min={1}
              step={0.1}
              className="field-input"
              value={splitLow}
              onChange={(e) => setSplitLow(Number(e.target.value))}
            />
          </div>
          <div>
            <label className="field-legend">
              Split rate (high guess)
            </label>
            <input
              type="number"
              min={1}
              step={0.1}
              className="field-input"
              value={splitHigh}
              onChange={(e) => setSplitHigh(Number(e.target.value))}
            />
            {splitError && (
              <p className="mt-1 text-xs text-red-600">{splitError}</p>
            )}
            {actualSplitRange && (
              <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                Actual growth rate range seen:{' '}
                <span className="font-semibold">
                  {actualSplitRange.min.toFixed(2)} to{' '}
                  {actualSplitRange.max.toFixed(2)}
                </span>
              </p>
            )}
          </div>

          {/* Trials */}
          <div>
            <label className="field-legend">
              Number of trials
            </label>
            <input
              type="number"
              min={100}
              step={100}
              max={10000}
              className="field-input"
              value={numTrials}
              onChange={(e) => setNumTrials(Number(e.target.value))}
            />
          </div>
        </div>

        <p className="mt-3 text-xs text-[var(--sea-ink-soft)]">
          Work often splits into smaller pieces when started. 1 = no change, 2 = every item
          might become two, 3 = every item might become three, etc.
        </p>
        <p className="mt-2 text-xs text-[var(--sea-ink-soft)]">
          The current inputs are stored in the URL for sharing.
        </p>
      </section>

      {/* ── Status ─────────────────────────────────────────────────────── */}
      {!canRun && (
        <div className="mb-10 flex items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-4 dark:border-amber-500/30 dark:bg-amber-950/30">
          <svg className="h-5 w-5 flex-shrink-0 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
            <line x1="12" y1="9" x2="12" y2="13" />
            <line x1="12" y1="17" x2="12.01" y2="17" />
          </svg>
          <div>
            <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Missing required inputs</p>
            <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
              {estimateError || splitError || featureCountError}
            </p>
          </div>
        </div>
      )}
      {running && (
        <p className="mb-10 text-sm font-medium text-[var(--lagoon-deep)] animate-pulse">Simulating…</p>
      )}

      {/* ── Results ──────────────────────────────────────────────────── */}
      {results && (
        <>
          {/* Percentile table */}
          <section className="island-shell mb-8 rounded-2xl p-5 sm:p-7">
            <h2 className="mb-4 text-lg font-bold text-[var(--sea-ink)]">
              3. Forecast: Total Story Count / Points
            </h2>

            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--line)] text-left text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                    <th className="px-3 py-2">Likelihood</th>
                    <th className="px-3 py-2">Total Story Count / Points</th>
                    <th className="px-3 py-2">Explanation</th>
                  </tr>
                </thead>
                <tbody>
                  {results.percentiles.map((p) => (
                    <tr
                      key={p.likelihood}
                      className="border-b border-[var(--line)] last:border-b-0"
                    >
                      <td className="px-3 py-2 font-mono">
                        {(p.likelihood * 100).toFixed(0)}%
                      </td>
                      <td className="px-3 py-2 font-bold text-[var(--lagoon-deep)]">
                        {p.count.toLocaleString()}
                      </td>
                      <td className="px-3 py-2 text-[var(--sea-ink-soft)]">
                        {p.likelihood === 0.5 &&
                          'Coin toss odds. Same chance being above or below this count.'}
                        {p.likelihood === 0.85 &&
                          'Pretty sure to be equal or less than this count.'}
                        {p.likelihood === 0.95 &&
                          'Almost certain to be equal or less than this count.'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          {/* Histogram */}
          <section className="island-shell mb-8 rounded-2xl p-5 sm:p-7">
            <h2 className="mb-4 text-lg font-bold text-[var(--sea-ink)]">
              Distribution: Total Story Count for {totalFeatureCount} Features
            </h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--line)] text-left text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                    <th className="px-3 py-2">Bin (≥)</th>
                    <th className="px-3 py-2">Count</th>
                    <th className="px-3 py-2">Probability</th>
                    <th className="px-3 py-2">Cumulative</th>
                    <th className="px-3 py-2 w-48">Bar</th>
                  </tr>
                </thead>
                <tbody>
                  {results.histogram.map((h, i) => {
                    const maxCount = Math.max(
                      ...results.histogram.map((b) => b.count),
                    )
                    const barPct =
                      maxCount > 0 ? (h.count / maxCount) * 100 : 0
                    return (
                      <tr
                        key={i}
                        className="border-b border-[var(--line)] last:border-b-0"
                      >
                        <td className="px-3 py-1.5 font-mono">
                          {h.binMin.toLocaleString()}
                        </td>
                        <td className="px-3 py-1.5">{h.count}</td>
                        <td className="px-3 py-1.5">
                          {(h.probability * 100).toFixed(1)}%
                        </td>
                        <td className="px-3 py-1.5">
                          {(h.cumProbability * 100).toFixed(1)}%
                        </td>
                        <td className="px-3 py-1.5">
                          <div className="h-4 w-full rounded bg-[var(--line)]">
                            <div
                              className="h-full rounded bg-[var(--lagoon)]"
                              style={{ width: `${barPct}%` }}
                            />
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </section>

          {/* Stability analysis */}
          <section className="island-shell mb-8 rounded-2xl p-5 sm:p-7">
            <h2 className="mb-4 text-lg font-bold text-[var(--sea-ink)]">
              Should I Believe This Forecast?
            </h2>

            <div className="grid gap-6 sm:grid-cols-2">
              <div>
                <span className="field-legend">Number of samples</span>
                <p className="text-lg font-bold text-[var(--sea-ink)]">
                  {results.stability.sampleCount}
                </p>
                <p className="text-sm">
                  <StabilityBadge
                    quality={results.stability.sampleQuality}
                  />
                </p>
              </div>

              <div>
                <span className="field-legend">
                  Error of average in two random groups
                </span>
                <p className="text-lg font-bold text-[var(--sea-ink)]">
                  {results.stability.errorOfAvgRatio != null
                    ? `${(results.stability.errorOfAvgRatio * 100).toFixed(1)}%`
                    : '—'}
                </p>
                <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                  0–25% good · 25–75% fair · {'>'}75% too unstable to forecast
                </p>
              </div>

              {results.stability.actualSplitRange && (
                <div>
                  <span className="field-legend">
                    Observed actual / estimate split range
                  </span>
                  <p className="text-lg font-bold text-[var(--sea-ink)]">
                    {results.stability.actualSplitRange.min.toFixed(2)} –{' '}
                    {results.stability.actualSplitRange.max.toFixed(2)}
                  </p>
                </div>
              )}
            </div>

            <p className="mt-4 text-xs text-[var(--sea-ink-soft)]">
              With fewer than 7 samples the error is often "unstable."
              Re-run the simulation a few times to see how the error changes
              (use best of 5!).
            </p>
          </section>
        </>
      )}

      <section className="island-shell rounded-2xl p-5 sm:p-7">
        <details className="space-y-4">
          <summary className="field-legend cursor-pointer text-sm">
            URL Parameters (advanced)
          </summary>
          <p className="max-w-3xl text-xs text-[var(--sea-ink-soft)]">
            Append query parameters to prefill the form for testing or shared
            scenarios. Historical feature rows are stored in a compact encoded
            string rather than raw JSON.
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
                    href={`/story-count${example.search}`}
                    className="rounded-full border border-[var(--line)] px-3 py-1 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:border-[var(--lagoon)] hover:text-[var(--sea-ink)]"
                  >
                    Open example
                  </a>
                </div>
                <p className="mt-3 break-all rounded-xl bg-[var(--header-bg)] px-3 py-2 font-mono text-[11px] text-[var(--sea-ink-soft)]">
                  /story-count{example.search}
                </p>
              </div>
            ))}
          </div>
        </details>
      </section>
    </main>
  )
}

/* ── Sub-components ──────────────────────────────────────────────────────── */

function StabilityBadge({ quality }: { quality: string }) {
  let color: string
  switch (quality) {
    case 'Excellent':
      color = 'bg-emerald-100 text-emerald-800'
      break
    case 'Good':
      color = 'bg-green-100 text-green-800'
      break
    case 'Acceptable':
      color = 'bg-amber-100 text-amber-800'
      break
    default:
      color = 'bg-red-100 text-red-800'
  }
  return (
    <span
      className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold ${color}`}
    >
      {quality}
    </span>
  )
}
