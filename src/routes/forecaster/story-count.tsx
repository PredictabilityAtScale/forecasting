import { createFileRoute } from '@tanstack/react-router'
import { useState, useCallback } from 'react'
import { runStoryCountForecaster } from '#/lib/monte-carlo'
import type {
  FeatureEstimate,
  StoryCountInputs,
  StoryCountResults,
} from '#/lib/monte-carlo'

export const Route = createFileRoute('/forecaster/story-count')({
  component: StoryCountForecasterPage,
})

/* ── Helpers ─────────────────────────────────────────────────────────────── */

let nextId = 1
function makeFeature(overrides?: Partial<FeatureEstimate>): FeatureEstimate {
  return {
    id: `F${nextId}`,
    name: `Feature ${nextId++}`,
    estimate: null,
    actual: null,
    ...overrides,
  }
}

function StoryCountForecasterPage() {
  // ── Input state ───────────────────────────────────────────────────────
  const [totalFeatureCount, setTotalFeatureCount] = useState(15)
  const [splitLow, setSplitLow] = useState(1)
  const [splitHigh, setSplitHigh] = useState(1)
  const [numTrials, setNumTrials] = useState(1000)
  const [features, setFeatures] = useState<FeatureEstimate[]>(() => {
    nextId = 1
    return Array.from({ length: 10 }, () => makeFeature())
  })

  // ── Results state ─────────────────────────────────────────────────────
  const [results, setResults] = useState<StoryCountResults | null>(null)
  const [running, setRunning] = useState(false)

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

  // ── Feature table handlers ────────────────────────────────────────────
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
      setFeatures((prev) => [
        ...prev,
        ...Array.from({ length: count }, () => makeFeature()),
      ])
    },
    [],
  )
  const removeRow = useCallback((idx: number) => {
    setFeatures((prev) => prev.filter((_, i) => i !== idx))
  }, [])

  // ── Run simulation ────────────────────────────────────────────────────
  const run = useCallback(() => {
    if (!canRun) return
    setRunning(true)
    // Defer to allow UI to update
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
  }, [canRun, totalFeatureCount, splitLow, splitHigh, features, numTrials])

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
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
      <section className="rise-in mb-10">
        <h1 className="display-title text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Story Count Forecaster
        </h1>
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
      </section>

      {/* ── Run button ───────────────────────────────────────────────── */}
      <div className="mb-10 flex items-center gap-4">
        <button
          disabled={!canRun || running}
          onClick={run}
          className="rounded-xl bg-[var(--lagoon-deep)] px-6 py-2.5 text-sm font-bold text-white shadow-md transition hover:brightness-110 disabled:opacity-40"
        >
          {running ? 'Simulating…' : 'Run Simulation'}
        </button>
        {estimateError && (
          <span className="text-xs text-red-600">{estimateError}</span>
        )}
      </div>

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
    </div>
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
