import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useMemo, useState, startTransition } from 'react'
import {
  loadSimulationExamples,
  parseSimMl,
  runMonteCarlo,
  runSensitivityAnalysis,
  runVisualSimulation,
  type BoardSnapshot,
} from '#/lib/kanban-scrum-sim'

export const Route = createFileRoute('/kanban-scrum-sim')({
  component: KanbanScrumSimPage,
})

const EXAMPLES = loadSimulationExamples()
const DEFAULT_EXAMPLE =
  EXAMPLES.find((example) => example.path.includes('1 - Simplest Board')) ?? EXAMPLES[0]

const KANBAN_EXAMPLES = EXAMPLES.filter((example) => example.type === 'kanban')
const SCRUM_EXAMPLES = EXAMPLES.filter((example) => example.type === 'scrum')

type SimulationResults = {
  visual: ReturnType<typeof runVisualSimulation>
  monteCarlo: ReturnType<typeof runMonteCarlo>
  sensitivity: ReturnType<typeof runSensitivityAnalysis>
}

function KanbanScrumSimPage() {
  const [isClient, setIsClient] = useState(false)
  const [selectedExampleId, setSelectedExampleId] = useState(DEFAULT_EXAMPLE?.id ?? '')
  const [source, setSource] = useState(DEFAULT_EXAMPLE?.source ?? '')
  const [stepIndex, setStepIndex] = useState(0)
  const [cycles, setCycles] = useState(300)
  const [runVersion, setRunVersion] = useState(0)
  const [results, setResults] = useState<SimulationResults | null>(null)
  const [isSimulating, setIsSimulating] = useState(false)
  const [isExplorerOpen, setIsExplorerOpen] = useState(false)
  const [activeExampleTab, setActiveExampleTab] = useState<'kanban' | 'scrum'>(
    DEFAULT_EXAMPLE?.type ?? 'kanban',
  )

  const selectedExample = EXAMPLES.find((example) => example.id === selectedExampleId) ?? null

  useEffect(() => {
    if (!selectedExample) return
    setSource(selectedExample.source)
    setStepIndex(0)
    setActiveExampleTab(selectedExample.type)
  }, [selectedExampleId, selectedExample])

  useEffect(() => {
    setIsClient(true)
  }, [])

  const parseResult = useMemo(() => {
    if (!isClient) {
      return { model: null, error: null as string | null }
    }
    try {
      return { model: parseSimMl(source), error: null as string | null }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to parse SimML.'
      return { model: null, error: message }
    }
  }, [isClient, source])
  const parsed = parseResult.model
  const modelError = parseResult.error

  useEffect(() => {
    if (!parsed) {
      setResults(null)
      setIsSimulating(false)
      return
    }

    let cancelled = false
    setIsSimulating(true)

    const timer = window.setTimeout(() => {
      if (cancelled) return
      const nextResults = {
        visual: runVisualSimulation(parsed),
        monteCarlo: runMonteCarlo(parsed, cycles),
        sensitivity: runSensitivityAnalysis(parsed),
      }
      if (cancelled) return
      setResults(nextResults)
      setIsSimulating(false)
    }, 10)

    return () => {
      cancelled = true
      window.clearTimeout(timer)
    }
  }, [parsed, cycles, runVersion])

  useEffect(() => {
    setStepIndex(0)
  }, [results?.visual.totalSteps])

  const snapshot = results?.visual.snapshots[Math.min(stepIndex, (results?.visual.snapshots.length ?? 1) - 1)] ?? null

  return (
    <main className="mx-auto max-w-[1500px] px-4 pb-14 pt-8 sm:px-6 lg:px-8">
      <div className="space-y-5">
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <div className="grid gap-4 xl:grid-cols-[minmax(280px,0.9fr)_minmax(320px,1fr)_minmax(320px,1.15fr)]">
            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-5">
              <p className="island-kicker mb-2">Legacy Simulator</p>
              <h1 className="display-title text-3xl font-semibold text-[var(--sea-ink)]">
                Kanban &amp; Scrum Sim
              </h1>
              <p className="mt-3 text-sm leading-6 text-[var(--sea-ink-soft)]">
                Browser port focused on loading legacy <code>.simml</code> examples, stepping through the board, and running Monte Carlo plus one-factor sensitivity analysis.
              </p>
              <Link
                to="/simml-reference"
                className="mt-4 inline-flex items-center gap-1.5 rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-3.5 py-1.5 text-xs font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
              >
                SimML Reference
                <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor"><path d="M4.53 4.53a.75.75 0 0 1 1.06 0l5.72 5.72V5a.75.75 0 0 1 1.5 0v7.25a.75.75 0 0 1-.75.75H4.81a.75.75 0 0 1 0-1.5h5.25L4.53 5.59a.75.75 0 0 1 0-1.06Z" /></svg>
              </Link>
              <Link
                to="/simml-studio"
                className="mt-2 inline-flex items-center gap-1.5 rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-3.5 py-1.5 text-xs font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
              >
                Open SimML Studio
              </Link>
            </div>

            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-5">
              <label className="field-legend">Example library</label>
              <button
                type="button"
                className="field-input mt-1 flex w-full items-center text-left"
                onClick={() => setIsExplorerOpen(true)}
              >
                <span className="min-w-0 flex-1 overflow-hidden text-ellipsis whitespace-nowrap">
                  {selectedExample
                    ? `${selectedExample.section} / ${selectedExample.title}`
                    : 'Choose a bundled example'}
                </span>
                <span className="ml-auto shrink-0 pl-4 text-right text-xs uppercase tracking-[0.12em] text-[var(--kicker)]">
                  Browse
                </span>
              </button>

              <label className="field-legend mt-4">Load local `.simml` file</label>
              <input
                type="file"
                accept=".simml,.SimML,.simML,.xml"
                className="field-input file:mr-3 file:rounded-full file:border-0 file:bg-[rgba(79,184,178,0.18)] file:px-3 file:py-1 file:text-xs file:font-semibold file:text-[var(--lagoon-deep)]"
                onChange={async (event) => {
                  const file = event.target.files?.[0]
                  if (!file) return
                  const text = await file.text()
                  startTransition(() => {
                    setSelectedExampleId('')
                    setSource(text)
                    setStepIndex(0)
                  })
                }}
              />
            </div>

            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-5">
              {(parsed?.example || selectedExample?.metadata.example) ? (
                <>
                  <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--kicker)]">
                    Example note
                  </p>
                  <p className="mt-2 text-sm leading-6 text-[var(--sea-ink-soft)]">
                    {parsed?.example ?? selectedExample?.metadata.example}
                  </p>
                </>
              ) : (
                <p className="text-sm text-[var(--sea-ink-soft)]">
                  Choose an example or load a local file to inspect metadata and model behavior.
                </p>
              )}

              {parsed?.warnings.length ? (
                <div className="mt-4 rounded-2xl border border-amber-300/60 bg-amber-50/70 p-4 text-sm text-amber-900">
                  <p className="font-semibold">Browser-version limitations</p>
                  <ul className="mt-2 space-y-2 pl-4">
                    {parsed.warnings.map((warning) => (
                      <li key={warning}>{warning}</li>
                    ))}
                  </ul>
                </div>
              ) : null}
            </div>
          </div>
        </section>

        {isExplorerOpen ? (
          <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/45 p-4"
            onClick={() => setIsExplorerOpen(false)}
          >
            <div
              className="max-h-[85vh] w-full max-w-4xl overflow-hidden rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] shadow-2xl"
              onClick={(event) => event.stopPropagation()}
            >
              <div className="flex items-center justify-between border-b border-[var(--line)] px-5 py-4">
                <h2 className="text-lg font-semibold text-[var(--sea-ink)]">Example explorer</h2>
                <button type="button" className="text-sm font-semibold text-[var(--sea-ink-soft)]" onClick={() => setIsExplorerOpen(false)}>Close</button>
              </div>
              <div className="px-5 pt-4">
                <div className="inline-flex rounded-full border border-[var(--line)] bg-[var(--surface-strong)] p-1 text-xs font-semibold">
                  {(['kanban', 'scrum'] as const).map((tab) => (
                    <button
                      key={tab}
                      type="button"
                      className={`rounded-full px-4 py-1.5 transition ${
                        activeExampleTab === tab
                          ? 'bg-[rgba(79,184,178,0.2)] text-[var(--lagoon-deep)]'
                          : 'text-[var(--sea-ink-soft)]'
                      }`}
                      onClick={() => setActiveExampleTab(tab)}
                    >
                      {tab === 'kanban' ? `Kanban (${KANBAN_EXAMPLES.length})` : `Scrum (${SCRUM_EXAMPLES.length})`}
                    </button>
                  ))}
                </div>
              </div>
              <div className="mt-4 max-h-[58vh] overflow-y-auto px-5 pb-5">
                {(activeExampleTab === 'kanban' ? KANBAN_EXAMPLES : SCRUM_EXAMPLES).map((example) => (
                  <button
                    key={example.id}
                    type="button"
                    className="mb-2 w-full rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-3 text-left transition hover:border-[rgba(79,184,178,0.45)]"
                    onClick={() => {
                      setSelectedExampleId(example.id)
                      setIsExplorerOpen(false)
                    }}
                  >
                    <p className="text-xs uppercase tracking-[0.12em] text-[var(--kicker)]">{example.section}</p>
                    <p className="mt-1 text-sm font-semibold text-[var(--sea-ink)]">{example.title}</p>
                    <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">Locale: {example.metadata.locale ?? 'n/a'}</p>
                    {example.metadata.example ? (
                      <p className="mt-1 line-clamp-2 text-xs text-[var(--sea-ink-soft)]">{example.metadata.example}</p>
                    ) : null}
                  </button>
                ))}
              </div>
            </div>
          </div>
        ) : null}

        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div>
              <p className="island-kicker">SimML</p>
              <h2 className="mt-1 text-xl font-semibold text-[var(--sea-ink)]">
                {parsed?.name ?? selectedExample?.title ?? 'Model editor'}
              </h2>
            </div>
            <button
              type="button"
              className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)]"
              onClick={() => setRunVersion((value) => value + 1)}
            >
              Re-run
            </button>
          </div>

          <textarea
            className="mt-4 min-h-[420px] w-full rounded-[1.5rem] border border-[rgba(141,229,219,0.18)] bg-[#0d2034] p-4 font-mono text-[12px] leading-6 text-[#e6f3ff] shadow-[inset_0_1px_0_rgba(255,255,255,0.05)] outline-none focus:border-[var(--lagoon)]"
            spellCheck={false}
            value={source}
            onChange={(event) => setSource(event.target.value)}
          />

          {modelError ? (
            <div className="mt-4 rounded-2xl border border-red-300/60 bg-red-50/80 p-4 text-sm text-red-900">
              <p className="font-semibold">Parse error</p>
              <p className="mt-1">{modelError}</p>
            </div>
          ) : parsed ? (
            <div className="mt-4 grid gap-4 md:grid-cols-3">
              <StatCard label="Mode" value={parsed.execute.simulationType.toUpperCase()} />
              <StatCard label="Work items" value={String(results?.visual.completedItems ?? 0)} />
              <StatCard
                label="Forecast"
                value={results?.visual.completionDate ?? `${results?.visual.totalSteps ?? 0} steps`}
              />
            </div>
          ) : null}
        </section>

        <section className="sim-board-shell rounded-[2rem] p-5 sm:p-6">
          {!isClient ? (
            <div className="rounded-[1.6rem] border border-[rgba(33,88,116,0.2)] bg-white/75 p-6 text-sm text-[var(--sea-ink-soft)]">
              Initializing the client-side simulator…
            </div>
          ) : results && snapshot ? (
            <div className="relative space-y-5">
              {isSimulating ? <SimulationSpinner label="Simulating board and forecasts..." /> : null}
              <div className="flex flex-wrap items-end justify-between gap-3">
                <div>
                  <p className="sim-board-kicker">Visual board</p>
                  <h2 className="mt-1 text-2xl font-semibold text-[var(--sea-ink)]">
                    {snapshot.label}
                    {snapshot.activePhase ? (
                      <span className="ml-3 inline-flex items-center gap-1.5 rounded-full border border-[rgba(79,184,178,0.35)] bg-[rgba(79,184,178,0.12)] px-3 py-0.5 align-middle text-sm font-semibold text-[var(--lagoon-deep)]">
                        <span className="inline-block h-2 w-2 rounded-full bg-[var(--lagoon)]" />
                        {snapshot.activePhase}
                      </span>
                    ) : null}
                  </h2>
                  <p className="mt-1 text-sm text-[var(--sea-ink-soft)]">
                    Backlog {snapshot.backlogCount} · Done {snapshot.doneCount}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <LegendChip label="Story" kind="story" />
                    <LegendChip label="Defect" kind="defect" />
                    <LegendChip label="Added scope" kind="addedScope" />
                    <span className="sim-board-legend sim-board-legend-blocked">Blocker</span>
                  </div>
                  <span className="rounded-full border border-[var(--line)] bg-[var(--surface-strong)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink-soft)]">
                    Step {snapshot.step} / {results.visual.totalSteps}
                  </span>
                </div>
              </div>

              <input
                className="w-full accent-[#257da0]"
                type="range"
                min={0}
                max={Math.max(0, results.visual.snapshots.length - 1)}
                step={1}
                value={Math.min(stepIndex, Math.max(0, results.visual.snapshots.length - 1))}
                onChange={(event) => setStepIndex(Number(event.target.value))}
              />

              <BoardView snapshot={snapshot} />
            </div>
          ) : (
            <div className="rounded-[1.6rem] border-2 border-dashed border-amber-300/70 bg-amber-50/70 p-6 text-sm text-amber-900">
              Fix the SimML to render the board.
            </div>
          )}
        </section>

        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          {!isClient ? null : results ? (
            <div className="relative space-y-4">
              {isSimulating ? <SimulationSpinner label="Running Monte Carlo simulation..." /> : null}
              <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
                <StatCard label="Total steps" value={String(results.visual.totalSteps)} />
                <StatCard label="Median steps" value={String(results.monteCarlo.medianSteps)} />
                <StatCard label="Std deviation" value={String(results.monteCarlo.standardDeviation)} />
                <StatCard label="Completion date" value={results.visual.completionDate ?? 'n/a'} />
                <StatCard
                  label="Cost"
                  value={
                    results.visual.totalCost > 0
                      ? results.visual.totalCost.toLocaleString(parsed?.locale ?? 'en-US', {
                          style: 'currency',
                          currency: 'USD',
                          maximumFractionDigits: 0,
                        })
                        : 'n/a'
                    }
                  />
              </div>

              <div className="grid gap-4 xl:grid-cols-[1.15fr_0.95fr]">
                <section className="rounded-[1.6rem] border border-[var(--line)] bg-[var(--surface)] p-4">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="island-kicker">Monte Carlo</p>
                      <h3 className="mt-1 text-lg font-semibold text-[var(--sea-ink)]">
                        Completion forecast
                      </h3>
                    </div>
                    <span className="text-sm font-semibold text-[var(--lagoon-deep)]">
                      avg {results.monteCarlo.averageSteps} steps
                    </span>
                  </div>
                  <div className="mt-4 rounded-2xl border border-[var(--line)] bg-[var(--header-bg)] p-3">
                    <div className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--kicker)]">
                          Monte Carlo cycles
                        </p>
                        <p className="mt-1 text-sm text-[var(--sea-ink-soft)]">
                          Higher cycles smooth the forecast, lower cycles respond faster.
                        </p>
                      </div>
                      <span className="rounded-full border border-[var(--line)] bg-[var(--surface-strong)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink)]">
                        {cycles}
                      </span>
                    </div>
                    <input
                      className="mt-3 w-full accent-[var(--lagoon-deep)]"
                      type="range"
                      min={50}
                      max={1000}
                      step={50}
                      value={cycles}
                      onChange={(event) => setCycles(Number(event.target.value))}
                    />
                  </div>
                  <div className="mt-4 rounded-2xl border border-[var(--line)] overflow-hidden">
                    <table className="w-full text-xs">
                      <thead className="bg-[var(--header-bg)] text-[var(--sea-ink-soft)]">
                        <tr>
                          <th className="px-3 py-2 text-left">Likelihood</th>
                          <th className="px-3 py-2 text-right">Steps</th>
                          <th className="px-3 py-2 text-right">Date</th>
                        </tr>
                      </thead>
                      <tbody>
                        {results.monteCarlo.percentileSteps.map((row) => (
                          <tr key={row.likelihood} className="border-t border-[var(--line)]">
                            <td className="px-3 py-2">{Math.round(row.likelihood * 100)}%</td>
                            <td className="px-3 py-2 text-right font-mono">{row.steps}</td>
                            <td className="px-3 py-2 text-right">{row.date ?? 'n/a'}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  <Histogram data={results.monteCarlo.histogram} />
                </section>

                <section className="rounded-[1.6rem] border border-[var(--line)] bg-[var(--surface)] p-4">
                  <p className="island-kicker">Sensitivity</p>
                  <h3 className="mt-1 text-lg font-semibold text-[var(--sea-ink)]">
                    Biggest forecast movers
                  </h3>
                  <p className="mt-1 text-sm text-[var(--sea-ink-soft)]">
                    One-factor-at-a-time comparison against a baseline average of {results.sensitivity.baselineAverageSteps} steps.
                  </p>
                  <div className="mt-4 space-y-3">
                    {results.sensitivity.tests.slice(0, 8).map((test) => (
                      <div key={`${test.type}-${test.name}`}>
                        <div className="flex items-end justify-between gap-3">
                          <div>
                            <p className="text-sm font-semibold text-[var(--sea-ink)]">{test.name}</p>
                            <p className="text-xs text-[var(--sea-ink-soft)]">{test.type}</p>
                          </div>
                          <p
                            className={`text-sm font-semibold ${
                              test.deltaSteps <= 0 ? 'text-emerald-700' : 'text-rose-700'
                            }`}
                          >
                            {test.deltaSteps > 0 ? '+' : ''}
                            {test.deltaSteps} steps
                          </p>
                        </div>
                        <div className="mt-2 h-2 rounded-full bg-[rgba(23,58,64,0.08)]">
                          <div
                            className={`h-full rounded-full ${
                              test.deltaSteps <= 0 ? 'bg-emerald-500/70' : 'bg-rose-500/70'
                            }`}
                            style={{
                              width: `${Math.min(100, Math.abs(test.deltaPercent) * 3 + 8)}%`,
                            }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </section>
              </div>
            </div>
          ) : null}
        </section>
      </div>
    </main>
  )
}

function BoardView({ snapshot }: { snapshot: BoardSnapshot }) {
  return (
    <div className="overflow-x-auto">
      <div className="grid min-w-[920px] gap-4" style={{ gridTemplateColumns: `repeat(${snapshot.columns.length}, minmax(200px, 1fr))` }}>
        {snapshot.columns.map((column) => (
          <section
            key={column.id}
            className="sim-board-column"
          >
            <div className="flex items-center justify-between gap-2">
              <div>
                <h3 className="text-sm font-semibold tracking-[0.01em] text-[var(--sea-ink)]">{column.label}</h3>
                <p className="text-[11px] font-medium text-[var(--sea-ink-soft)]">
                  {column.cards.length} items{column.wipLimit ? ` / WIP ${column.wipLimit}` : ''}
                </p>
              </div>
            </div>
            <div className="mt-3 min-h-[100px] space-y-2">
              {column.cards.length > 0 ? (
                column.cards.slice(0, 18).map((card) => (
                  <article
                    key={card.id}
                    className={`sim-board-card sim-board-card-${card.kind} ${card.status === 'queued' ? 'sim-board-card-queued' : ''} ${card.isBlocked ? 'sim-board-card-blocked' : ''}`}
                  >
                    <div className="sim-board-card-pin" />
                    {card.isBlocked ? (
                      <div
                        className="sim-board-card-blocker"
                        title={card.blockerLabel ? `Blocked by ${card.blockerLabel}` : 'Blocked'}
                        aria-label={card.blockerLabel ? `Blocked by ${card.blockerLabel}` : 'Blocked'}
                      >
                        {card.blockerLabel ? (
                          <span>{card.blockerLabel}</span>
                        ) : null}
                      </div>
                    ) : null}
                    <p className="sim-board-card-kind">
                      {card.kind === 'addedScope'
                        ? 'Scope'
                        : card.kind === 'defect'
                          ? 'Defect'
                          : 'Story'}
                    </p>
                    <p className="sim-board-card-title">{card.label}</p>
                    {card.deliverable ? (
                      <p className="sim-board-card-meta">
                        {card.deliverable}
                      </p>
                    ) : null}
                  </article>
                ))
              ) : (
                <div className="col-span-full rounded-2xl border border-dashed border-[var(--line)] bg-[var(--surface)] px-3 py-10 text-center text-xs font-medium text-[var(--sea-ink-soft)]">
                  Empty
                </div>
              )}
              {column.cards.length > 18 ? (
                <p className="col-span-full text-center text-[11px] font-semibold text-[var(--sea-ink-soft)]">
                  +{column.cards.length - 18} more
                </p>
              ) : null}
            </div>
          </section>
        ))}
      </div>
    </div>
  )
}

function Histogram({ data }: { data: { step: number; count: number }[] }) {
  if (!data.length) {
    return <p className="mt-4 text-sm text-[var(--sea-ink-soft)]">No histogram data.</p>
  }

  const maxCount = Math.max(...data.map((entry) => entry.count))

  return (
    <div className="mt-4 space-y-1">
      {data.map((entry) => (
        <div key={entry.step} className="grid grid-cols-[48px_1fr_48px] items-center gap-2 text-[11px]">
          <span className="font-mono text-[var(--sea-ink-soft)]">{entry.step}</span>
          <div className="h-4 rounded-full bg-[rgba(23,58,64,0.08)]">
            <div
              className="h-full rounded-full bg-[linear-gradient(90deg,rgba(50,143,151,0.75),rgba(125,211,195,0.9))]"
              style={{ width: `${(entry.count / maxCount) * 100}%` }}
            />
          </div>
          <span className="text-right font-mono text-[var(--sea-ink-soft)]">{entry.count}</span>
        </div>
      ))}
    </div>
  )
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-[1.4rem] border border-[var(--line)] bg-[var(--surface)] p-4">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--kicker)]">
        {label}
      </p>
      <p className="mt-2 text-lg font-semibold text-[var(--sea-ink)]">{value}</p>
    </div>
  )
}

function LegendChip({
  label,
  kind,
}: {
  label: string
  kind: 'story' | 'defect' | 'addedScope'
}) {
  return <span className={`sim-board-legend sim-board-legend-${kind}`}>{label}</span>
}

function SimulationSpinner({ label }: { label: string }) {
  return (
    <div className="absolute inset-0 z-10 flex items-center justify-center rounded-[1.6rem] bg-[rgba(8,24,40,0.18)] backdrop-blur-[2px]">
      <div className="flex items-center gap-3 rounded-full border border-[rgba(50,143,151,0.24)] bg-[rgba(255,255,255,0.92)] px-4 py-2 text-sm font-semibold text-[var(--sea-ink)] shadow-[0_12px_24px_rgba(23,58,64,0.12)]">
        <span className="h-4 w-4 animate-spin rounded-full border-2 border-[rgba(50,143,151,0.2)] border-t-[var(--lagoon-deep)]" />
        {label}
      </div>
    </div>
  )
}
