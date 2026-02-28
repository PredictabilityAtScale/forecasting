import { createFileRoute } from '@tanstack/react-router'
import { useState, useMemo } from 'react'
import { computeWrongOrder } from '#/lib/wrong-order-meter'
import StatCard from '#/components/StatCard'

export const Route = createFileRoute('/wrong-order')({
  component: WrongOrderMeterPage,
})

/* ── Page ────────────────────────────────────────────────────────────────── */

function WrongOrderMeterPage() {
  const [plannedText, setPlannedText] = useState(
    'Feature 1\nFeature 2\nFeature 3\nFeature 4\nFeature 5\nFeature 6',
  )
  const [deliveredText, setDeliveredText] = useState(
    'Unplanned feature 1\nFeature 3\nFeature 2\nFeature 4\nFeature 5',
  )

  const planned = useMemo(
    () => plannedText.split('\n').filter((s) => s.trim() !== ''),
    [plannedText],
  )
  const delivered = useMemo(
    () => deliveredText.split('\n').filter((s) => s.trim() !== ''),
    [deliveredText],
  )

  /* ── Live result ────────────────────────────────────────────── */
  const result = useMemo(() => {
    if (planned.length === 0 || delivered.length === 0) return null
    return computeWrongOrder({ planned, delivered })
  }, [planned, delivered])

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Wrong Order-O-Meter
        </h1>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Measures how far the actual delivery order diverged from the plan.
          Enter planned features in priority order, then enter what was actually
          delivered. Items not in the plan are treated as unplanned work.
        </p>

        <div className="grid gap-8 lg:grid-cols-2">
          {/* ── Inputs ──────────────────────────────────────────────── */}
          <div className="space-y-5">
            <div>
              <label className="field-legend">
                Planned Order{' '}
                <span className="font-normal text-[var(--sea-ink-soft)]">
                  (one feature per line, in priority order)
                </span>
              </label>
              <textarea
                className="field-input min-h-[220px] font-mono text-xs leading-relaxed"
                value={plannedText}
                onChange={(e) => setPlannedText(e.target.value)}
                rows={10}
              />
              <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                {planned.length} item{planned.length !== 1 ? 's' : ''}
              </p>
            </div>

            <div>
              <label className="field-legend">
                Delivered Order{' '}
                <span className="font-normal text-[var(--sea-ink-soft)]">
                  (one feature per line, in order delivered)
                </span>
              </label>
              <textarea
                className="field-input min-h-[220px] font-mono text-xs leading-relaxed"
                value={deliveredText}
                onChange={(e) => setDeliveredText(e.target.value)}
                rows={10}
              />
              <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                {delivered.length} item{delivered.length !== 1 ? 's' : ''}
              </p>
            </div>
          </div>

          {/* ── Results ─────────────────────────────────────────────── */}
          <div className="space-y-6">
            {result ? (
              <>
                {/* Duplicate warnings */}
                {(result.plannedDuplicates.length > 0 ||
                  result.deliveredDuplicates.length > 0) && (
                  <div className="rounded-xl border border-amber-300/50 bg-amber-50/60 p-4 dark:border-amber-500/30 dark:bg-amber-950/30">
                    <p className="mb-1 text-xs font-bold text-amber-700 dark:text-amber-300">
                      Duplicate names detected
                    </p>
                    {result.plannedDuplicates.length > 0 && (
                      <p className="text-xs text-amber-600 dark:text-amber-400">
                        Planned: {result.plannedDuplicates.join(', ')}
                      </p>
                    )}
                    {result.deliveredDuplicates.length > 0 && (
                      <p className="text-xs text-amber-600 dark:text-amber-400">
                        Delivered: {result.deliveredDuplicates.join(', ')}
                      </p>
                    )}
                  </div>
                )}

                {/* Gauge */}
                <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-start sm:justify-center">
                  <GaugeChart
                    label="With Unplanned"
                    score={result.scoreWithUnplanned}
                    max={result.maxScore}
                  />
                  <GaugeChart
                    label="Planned Only"
                    score={result.scorePlannedOnly}
                    max={result.maxScore}
                  />
                </div>

                {/* Summary stats */}
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <StatCard
                    label="With Unplanned"
                    value={result.scoreWithUnplanned}
                  />
                  <StatCard
                    label="Planned Only"
                    value={result.scorePlannedOnly}
                  />
                  <StatCard
                    label="Planned Delivered"
                    value={result.plannedDeliveredCount}
                  />
                  <StatCard
                    label="Unplanned Delivered"
                    value={result.unplannedDeliveredCount}
                  />
                </div>

                {/* Delivered item details */}
                <div>
                  <h3 className="mb-2 text-sm font-bold text-[var(--sea-ink)]">
                    Delivered Items
                  </h3>
                  <div className="overflow-x-auto rounded-xl border border-[var(--line)]">
                    <table className="w-full text-xs">
                      <thead>
                        <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                          <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                            #
                          </th>
                          <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                            Feature
                          </th>
                          <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                            Type
                          </th>
                          <th className="px-3 py-2 text-right font-semibold text-[var(--sea-ink-soft)]">
                            Planned #
                          </th>
                          <th className="px-3 py-2 text-right font-semibold text-[var(--sea-ink-soft)]">
                            Penalty
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {result.deliveredDetails.map((d) => (
                          <tr
                            key={`d-${d.deliveredPosition}`}
                            className="border-b border-[var(--line)] last:border-0"
                          >
                            <td className="px-3 py-2 tabular-nums text-[var(--sea-ink-soft)]">
                              {d.deliveredPosition}
                            </td>
                            <td className="px-3 py-2 font-medium text-[var(--sea-ink)]">
                              {d.name}
                            </td>
                            <td className="px-3 py-2">
                              <span
                                className={`inline-block rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider ${
                                  d.kind === 'planned'
                                    ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300'
                                    : 'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300'
                                }`}
                              >
                                {d.kind}
                              </span>
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums text-[var(--sea-ink-soft)]">
                              {d.plannedPosition ?? '—'}
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums font-semibold">
                              <span
                                className={
                                  d.penalty === 0
                                    ? 'text-emerald-600 dark:text-emerald-400'
                                    : 'text-[var(--sea-ink)]'
                                }
                              >
                                {d.penalty}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Undelivered items */}
                {result.undeliveredDetails.length > 0 && (
                  <div>
                    <h3 className="mb-2 text-sm font-bold text-[var(--sea-ink)]">
                      Planned But Not Delivered
                    </h3>
                    <div className="overflow-x-auto rounded-xl border border-[var(--line)]">
                      <table className="w-full text-xs">
                        <thead>
                          <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                            <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                              Planned #
                            </th>
                            <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                              Feature
                            </th>
                            <th className="px-3 py-2 text-right font-semibold text-[var(--sea-ink-soft)]">
                              Penalty
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          {result.undeliveredDetails.map((u) => (
                            <tr
                              key={`u-${u.plannedPosition}`}
                              className="border-b border-[var(--line)] last:border-0"
                            >
                              <td className="px-3 py-2 tabular-nums text-[var(--sea-ink-soft)]">
                                {u.plannedPosition}
                              </td>
                              <td className="px-3 py-2 font-medium text-[var(--sea-ink)]">
                                {u.name}
                              </td>
                              <td className="px-3 py-2 text-right tabular-nums font-semibold text-red-600 dark:text-red-400">
                                {u.penalty}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}

                {/* Score range context */}
                <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-4">
                  <p className="text-xs text-[var(--sea-ink-soft)]">
                    <strong>Score range:</strong> 0 (perfect — everything
                    delivered in planned order) to {result.maxScore} (worst
                    possible). Lower is better.
                  </p>
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
                  <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Missing required inputs</p>
                  <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                    Enter at least <strong>one planned item</strong> and <strong>one delivered item</strong> to calculate the score.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* Explanation */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">How It Works</p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Displacement Penalty
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              For each planned item that was delivered, the penalty is the
              absolute difference between its planned position and actual
              delivery position. Delivered in exactly the right slot = 0 penalty.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Unplanned Work
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Items delivered that weren't in the plan receive the maximum
              penalty (planned count + 1). The "Planned Only" score excludes
              these so you can see the impact of re-ordering alone.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Undelivered Items
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Planned items that never appeared in the delivered list also
              receive the maximum penalty. This captures the cost of planned
              work that didn't get done.
            </p>
          </div>
        </div>
      </section>
    </main>
  )
}

/* ── Sub-components ──────────────────────────────────────────────────────── */


/* ── Speedometer Gauge (SVG) ─────────────────────────────────────────────── */

function GaugeChart({
  label,
  score,
  max,
}: {
  label: string
  score: number
  max: number
}) {
  const size = 280
  const cx = size / 2
  const cy = size / 2 + 12
  const r = 110
  const strokeW = 24

  // Arc from 180° (left) to 0° (right) — a half circle
  const startAngle = Math.PI // 180°
  const endAngle = 0 // 0°

  // Gauge zones: Great (20%), Good (30%), Bad (30%), Poor (20%)
  const zones = [
    { frac: 0.2, color: '#10b981' }, // green — Great
    { frac: 0.3, color: '#facc15' }, // yellow — Good
    { frac: 0.3, color: '#f97316' }, // orange — Bad
    { frac: 0.2, color: '#ef4444' }, // red — Poor
  ]

  // Build arc paths
  const arcPaths: { d: string; color: string }[] = []
  let cumFrac = 0
  for (const zone of zones) {
    const a1 = startAngle - cumFrac * Math.PI
    const a2 = startAngle - (cumFrac + zone.frac) * Math.PI
    cumFrac += zone.frac

    const x1 = cx + r * Math.cos(a1)
    const y1 = cy - r * Math.sin(a1)
    const x2 = cx + r * Math.cos(a2)
    const y2 = cy - r * Math.sin(a2)
    const largeArc = zone.frac > 0.5 ? 1 : 0

    arcPaths.push({
      d: `M ${x1} ${y1} A ${r} ${r} 0 ${largeArc} 1 ${x2} ${y2}`,
      color: zone.color,
    })
  }

  // Needle angle
  const pct = Math.min(score / max, 1)
  const needleAngle = startAngle - pct * (startAngle - endAngle)
  const needleLen = r - 12
  const nx = cx + needleLen * Math.cos(needleAngle)
  const ny = cy - needleLen * Math.sin(needleAngle)

  // Zone label for the current score
  let zoneLabel: string
  if (pct <= 0.2) zoneLabel = 'Great'
  else if (pct <= 0.5) zoneLabel = 'Good'
  else if (pct <= 0.8) zoneLabel = 'Bad'
  else zoneLabel = 'Poor'

  const zoneLabelColor =
    pct <= 0.2
      ? '#10b981'
      : pct <= 0.5
        ? '#ca8a04'
        : pct <= 0.8
          ? '#f97316'
          : '#ef4444'

  return (
    <div className="flex flex-col items-center">
      <svg
        width={size}
        height={size / 2 + 48}
        viewBox={`0 0 ${size} ${size / 2 + 48}`}
        className="drop-shadow-sm"
      >
        {/* Zone arcs */}
        {arcPaths.map((arc, i) => (
          <path
            key={i}
            d={arc.d}
            fill="none"
            stroke={arc.color}
            strokeWidth={strokeW}
            strokeLinecap="butt"
            opacity={0.8}
          />
        ))}

        {/* Needle */}
        <line
          x1={cx}
          y1={cy}
          x2={nx}
          y2={ny}
          stroke="var(--sea-ink)"
          strokeWidth={3}
          strokeLinecap="round"
        />
        <circle cx={cx} cy={cy} r={6} fill="var(--sea-ink)" />

        {/* Score text */}
        <text
          x={cx}
          y={cy + 28}
          textAnchor="middle"
          fill="var(--sea-ink)"
          fontSize="28"
          fontWeight="700"
        >
          {score}
        </text>

        {/* Zone label */}
        <text
          x={cx}
          y={cy + 46}
          textAnchor="middle"
          fontSize="14"
          fontWeight="600"
          fill={zoneLabelColor}
        >
          {zoneLabel}
        </text>

        {/* Min / Max labels */}
        <text
          x={cx - r - 6}
          y={cy + 8}
          textAnchor="end"
          fontSize="12"
          fill="var(--sea-ink-soft)"
        >
          0
        </text>
        <text
          x={cx + r + 6}
          y={cy + 8}
          textAnchor="start"
          fontSize="12"
          fill="var(--sea-ink-soft)"
        >
          {max}
        </text>
      </svg>
      <p className="mt-2 text-sm font-semibold text-[var(--sea-ink-soft)]">
        {label}
      </p>
    </div>
  )
}
