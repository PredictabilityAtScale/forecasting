import { createFileRoute } from '@tanstack/react-router'
import { useState, useCallback, useMemo } from 'react'
import {
  computeLatentDefects,
  type Defect,
} from '#/lib/latent-defect'

export const Route = createFileRoute('/latent-defect')({
  component: LatentDefectPage,
})

/* ── Sample data (matches spreadsheet) ───────────────────────────────── */

const SAMPLE_DEFECTS: Defect[] = [
  { name: 'Defect 1', severity: '', foundByA: true, foundByB: false },
  { name: 'Defect 2', severity: '', foundByA: true, foundByB: false },
  { name: 'Defect 3', severity: '', foundByA: false, foundByB: true },
  { name: 'Defect 4', severity: '', foundByA: true, foundByB: true },
  { name: 'Defect 5', severity: '', foundByA: true, foundByB: false },
  { name: 'Defect 6', severity: '', foundByA: true, foundByB: true },
  { name: 'Defect 7', severity: '', foundByA: false, foundByB: true },
]

/* ── Page ────────────────────────────────────────────────────────────── */

function LatentDefectPage() {
  const [defects, setDefects] = useState<Defect[]>(SAMPLE_DEFECTS)
  const [groupALabel, setGroupALabel] = useState('Group A')
  const [groupBLabel, setGroupBLabel] = useState('Group B')

  /* ── Live result ────────────────────────────────────────────── */
  const result = useMemo(() => {
    const valid = defects.filter((d) => d.name.trim() !== '')
    if (valid.length === 0) return null
    return computeLatentDefects(valid)
  }, [defects])

  const addRow = useCallback(() => {
    setDefects((prev) => [
      ...prev,
      {
        name: `Defect ${prev.length + 1}`,
        severity: '',
        foundByA: false,
        foundByB: false,
      },
    ])
  }, [])

  const removeRow = useCallback((idx: number) => {
    setDefects((prev) => prev.filter((_, i) => i !== idx))
  }, [])

  const updateDefect = useCallback(
    (idx: number, patch: Partial<Defect>) => {
      setDefects((prev) =>
        prev.map((d, i) => (i === idx ? { ...d, ...patch } : d)),
      )
    },
    [],
  )

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Latent Defect Estimation
        </h1>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Uses the Lincoln-Petersen capture-recapture method to estimate how
          many defects remain undiscovered. Two independent groups each
          review/test the same product — overlapping finds let us estimate the
          total population of defects.
        </p>

        {/* ── Group labels ─────────────────────────────────────────── */}
        <div className="mb-6 grid gap-4 sm:grid-cols-2">
          <div>
            <label className="field-legend">Group A Name</label>
            <input
              className="field-input"
              value={groupALabel}
              onChange={(e) => setGroupALabel(e.target.value)}
              placeholder="Group A"
            />
          </div>
          <div>
            <label className="field-legend">Group B Name</label>
            <input
              className="field-input"
              value={groupBLabel}
              onChange={(e) => setGroupBLabel(e.target.value)}
              placeholder="Group B"
            />
          </div>
        </div>

        {/* ── Defect table ─────────────────────────────────────────── */}
        <div className="mb-6 overflow-x-auto rounded-xl border border-[var(--line)]">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                  Defect Name
                </th>
                <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                  Severity
                </th>
                <th className="px-3 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  {groupALabel || 'Group A'}
                </th>
                <th className="px-3 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  {groupBLabel || 'Group B'}
                </th>
                <th className="w-10 px-2 py-2" />
              </tr>
            </thead>
            <tbody>
              {defects.map((d, idx) => (
                <tr
                  key={idx}
                  className="border-b border-[var(--line)] last:border-0"
                >
                  <td className="px-3 py-1.5">
                    <input
                      className="w-full rounded-lg border border-[var(--line)] bg-[var(--bg-base)] px-2 py-1 text-sm text-[var(--sea-ink)] outline-none focus:border-[var(--lagoon)]"
                      value={d.name}
                      onChange={(e) =>
                        updateDefect(idx, { name: e.target.value })
                      }
                    />
                  </td>
                  <td className="px-3 py-1.5">
                    <input
                      className="w-full rounded-lg border border-[var(--line)] bg-[var(--bg-base)] px-2 py-1 text-sm text-[var(--sea-ink)] outline-none focus:border-[var(--lagoon)]"
                      value={d.severity}
                      onChange={(e) =>
                        updateDefect(idx, { severity: e.target.value })
                      }
                      placeholder="—"
                    />
                  </td>
                  <td className="px-3 py-1.5 text-center">
                    <input
                      type="checkbox"
                      checked={d.foundByA}
                      onChange={(e) =>
                        updateDefect(idx, { foundByA: e.target.checked })
                      }
                      className="h-4 w-4 rounded border-[var(--line)] accent-[var(--lagoon)]"
                    />
                  </td>
                  <td className="px-3 py-1.5 text-center">
                    <input
                      type="checkbox"
                      checked={d.foundByB}
                      onChange={(e) =>
                        updateDefect(idx, { foundByB: e.target.checked })
                      }
                      className="h-4 w-4 rounded border-[var(--line)] accent-[var(--lagoon)]"
                    />
                  </td>
                  <td className="px-2 py-1.5 text-center">
                    <button
                      type="button"
                      onClick={() => removeRow(idx)}
                      className="text-[var(--sea-ink-soft)] transition hover:text-red-500"
                      title="Remove defect"
                    >
                      <svg
                        className="h-4 w-4"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      >
                        <line x1="18" y1="6" x2="6" y2="18" />
                        <line x1="6" y1="6" x2="18" y2="18" />
                      </svg>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="mb-6 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={addRow}
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-4 py-2 text-sm font-medium text-[var(--sea-ink)] transition hover:bg-[var(--surface-strong)]"
          >
            + Add Defect
          </button>
        </div>

        {/* ── Results ──────────────────────────────────────────────── */}
        {result ? (
          <div className="space-y-6">
            {/* Big number */}
            <div className="flex flex-col items-center gap-2 rounded-2xl border border-[var(--line)] bg-[var(--surface)] py-8">
              {result.estimatedTotal !== null ? (
                <>
                  <p className="m-0 text-5xl font-extrabold tabular-nums text-[var(--lagoon)]">
                    {result.estimatedTotal}
                  </p>
                  <p className="m-0 text-sm font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                    Estimated Total Defects
                  </p>
                  <p className="m-0 mt-2 text-3xl font-bold tabular-nums text-red-500 dark:text-red-400">
                    {result.estimatedUndiscovered}
                  </p>
                  <p className="m-0 text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                    Estimated Undiscovered
                  </p>
                </>
              ) : (
                <>
                  <p className="m-0 text-lg font-semibold text-amber-600 dark:text-amber-400">
                    More data required
                  </p>
                  <p className="m-0 text-xs text-[var(--sea-ink-soft)]">
                    At least one defect must be found by both groups.
                  </p>
                </>
              )}
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
              <StatCard
                label={`Total ${groupALabel}`}
                value={result.totalA}
              />
              <StatCard
                label={`Total ${groupBLabel}`}
                value={result.totalB}
              />
              <StatCard
                label={`Only ${groupALabel}`}
                value={result.onlyA}
              />
              <StatCard
                label={`Only ${groupBLabel}`}
                value={result.onlyB}
              />
              <StatCard label="Found by Both" value={result.both} />
              <StatCard label="Total Found" value={result.totalFound} />
            </div>

            {/* Detail table */}
            <div>
              <h3 className="mb-2 text-sm font-bold text-[var(--sea-ink)]">
                Defect Breakdown
              </h3>
              <div className="overflow-x-auto rounded-xl border border-[var(--line)]">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                      <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                        Defect
                      </th>
                      <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                        Severity
                      </th>
                      <th className="px-3 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                        {groupALabel}
                      </th>
                      <th className="px-3 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                        {groupBLabel}
                      </th>
                      <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                        Category
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {result.details.map((d, i) => (
                      <tr
                        key={i}
                        className="border-b border-[var(--line)] last:border-0"
                      >
                        <td className="px-3 py-2 font-medium text-[var(--sea-ink)]">
                          {d.name}
                        </td>
                        <td className="px-3 py-2 text-[var(--sea-ink-soft)]">
                          {d.severity || '—'}
                        </td>
                        <td className="px-3 py-2 text-center">
                          {d.foundByA ? (
                            <span className="text-emerald-600 dark:text-emerald-400">
                              ✓
                            </span>
                          ) : (
                            <span className="text-[var(--sea-ink-soft)] opacity-30">
                              —
                            </span>
                          )}
                        </td>
                        <td className="px-3 py-2 text-center">
                          {d.foundByB ? (
                            <span className="text-emerald-600 dark:text-emerald-400">
                              ✓
                            </span>
                          ) : (
                            <span className="text-[var(--sea-ink-soft)] opacity-30">
                              —
                            </span>
                          )}
                        </td>
                        <td className="px-3 py-2">
                          <BucketBadge bucket={d.bucket} />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Formula explanation */}
            <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-4">
              <p className="text-xs text-[var(--sea-ink-soft)]">
                <strong>Formula:</strong> Estimated Total =
                ⌈({groupALabel} × {groupBLabel}) / Both⌉ ={' '}
                {result.both > 0
                  ? `⌈(${result.totalA} × ${result.totalB}) / ${result.both}⌉ = ${result.estimatedTotal}`
                  : 'N/A (no overlapping finds)'}
              </p>
            </div>
          </div>
        ) : (
          <div className="flex items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-6 dark:border-amber-500/30 dark:bg-amber-950/30">
            <svg className="h-6 w-6 flex-shrink-0 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
              <line x1="12" y1="9" x2="12" y2="13" />
              <line x1="12" y1="17" x2="12.01" y2="17" />
            </svg>
            <div>
              <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Missing required inputs</p>
              <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                Add at least <strong>one defect with a name</strong> to estimate latent defects.
              </p>
            </div>
          </div>
        )}
      </section>

      {/* ── How It Works ─────────────────────────────────────────── */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">How It Works</p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Capture-Recapture
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Originally used to estimate wildlife populations, the
              Lincoln-Petersen method works by having two independent groups
              "capture" defects. The overlap tells us about the total population.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Independent Testing
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Both groups must test independently and thoroughly. Ask each group:
              "Have you tested thoroughly enough that you feel the major majority
              of issues are found?" The estimate improves with thoroughness.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              The Formula
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Estimated Total = ⌈(Total A × Total B) / Both⌉. If the groups
              find few overlapping defects, the estimate rises sharply — lots of
              territory remains unexplored.
            </p>
          </div>
        </div>
      </section>

      {/* ── Pre-session questions ────────────────────────────────── */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">Questions to Ask Both Groups</p>
        <ol className="m-0 list-inside list-decimal space-y-3 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
          <li>
            Have you tested thoroughly enough that <em>you</em> feel all or the
            major majority of issues are found?
          </li>
          <li>
            How surprised would you be if another group found double the issues
            you did?
          </li>
          <li>
            How surprised would you be if this analysis found there are only a{' '}
            <em>few</em> latent defects?
          </li>
        </ol>
      </section>
    </main>
  )
}

/* ── Sub-components ──────────────────────────────────────────────────── */

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3 text-center">
      <p className="m-0 text-lg font-bold tabular-nums text-[var(--sea-ink)]">
        {value}
      </p>
      <p className="m-0 text-[10px] font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
        {label}
      </p>
    </div>
  )
}

function BucketBadge({ bucket }: { bucket: string }) {
  const styles: Record<string, string> = {
    both: 'bg-violet-100 text-violet-700 dark:bg-violet-900/40 dark:text-violet-300',
    onlyA:
      'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
    onlyB:
      'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
    neither:
      'bg-gray-100 text-gray-500 dark:bg-gray-800/40 dark:text-gray-400',
  }
  const labels: Record<string, string> = {
    both: 'Both',
    onlyA: 'Only A',
    onlyB: 'Only B',
    neither: 'Neither',
  }
  return (
    <span
      className={`inline-block rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider ${styles[bucket] ?? styles.neither}`}
    >
      {labels[bucket] ?? bucket}
    </span>
  )
}
