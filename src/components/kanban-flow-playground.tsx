import { useEffect, useMemo, useRef, useState } from 'react'
import { parseSimMl, runMonteCarlo, runVisualSimulation } from '#/lib/kanban-scrum-sim'
import type { SimModel, SimPullOrder } from '#/lib/kanban-scrum-sim'
import { SimBoardView } from '#/components/sim-board'
import { Slider } from '#/components/ui/slider'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '#/components/ui/select'

interface KanbanFlowPlaygroundProps {
  source: string
}

const LEARNING_LAB_MONTE_CARLO_CYCLES = 25

const PULL_ORDER_OPTIONS: Array<{ value: SimPullOrder; label: string }> = [
  { value: 'FIFO', label: 'FIFO (default Kanban pull)' },
  { value: 'FIFOStrict', label: 'FIFO Strict' },
  { value: 'indexSequence', label: 'Backlog order (index sequence)' },
  { value: 'randomAfterOrdering', label: 'Random after ordering' },
  { value: 'random', label: 'Random pull' },
]

const SIM_DEBOUNCE_MS = 250

function useDebouncedValue<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value)
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined)
  useEffect(() => {
    timer.current = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(timer.current)
  }, [value, delay])
  return debounced
}

function findFirstSnapshotWithDefect(snapshots: ReturnType<typeof runVisualSimulation>['snapshots']) {
  return snapshots.findIndex((snapshot) =>
    snapshot.columns.some((column) => column.cards.some((card) => card.kind === 'defect')),
  )
}

function getAverageRate(low: number, high: number) {
  return Math.round((low + high) / 2)
}

function formatDelta(delta: number) {
  if (Math.abs(delta) < 0.05) return 'No material change'
  return delta > 0 ? `+${delta.toFixed(1)}%` : `${delta.toFixed(1)}%`
}

function getEightyFive(result: ReturnType<typeof runMonteCarlo>) {
  return result.percentileSteps.find((percentile) => percentile.likelihood === 0.85)?.steps ?? result.maxSteps
}

function cloneAndTuneModel({
  baseModel,
  constraintColumnId,
  wipLimit,
  blockerRate,
  defectRate,
  pullOrder,
}: {
  baseModel: SimModel
  constraintColumnId: number
  wipLimit: number
  blockerRate: number
  defectRate: number
  pullOrder: SimPullOrder
}) {
  const model = structuredClone(baseModel)
  const constraintColumn = model.setup.columns.find((column) => column.id === constraintColumnId)
  if (constraintColumn) {
    constraintColumn.wipLimit = wipLimit
  }

  const firstBlockingEvent = model.setup.blockingEvents[0]!
  firstBlockingEvent.columnId = constraintColumnId
  firstBlockingEvent.occurrenceLowBound = blockerRate
  firstBlockingEvent.occurrenceHighBound = blockerRate

  const firstDefect = model.setup.defects[0]!
  firstDefect.startsInColumnId = constraintColumnId
  firstDefect.occurrenceLowBound = defectRate
  firstDefect.occurrenceHighBound = defectRate

  model.execute.pullOrder = pullOrder
  return model
}

export default function KanbanFlowPlayground({ source }: KanbanFlowPlaygroundProps) {
  const parsedModel = useMemo(() => parseSimMl(source), [source])

  const selectableColumns = useMemo(
    () => parsedModel.setup.columns.filter((column) => !column.isBuffer),
    [parsedModel],
  )

  const [constraintColumnId, setConstraintColumnId] = useState(selectableColumns[0]?.id ?? 1)
  const [wipLimit, setWipLimit] = useState(selectableColumns[0]?.wipLimit ?? 1)
  const [blockerRate, setBlockerRate] = useState(
    getAverageRate(
      parsedModel.setup.blockingEvents[0]!.occurrenceLowBound,
      parsedModel.setup.blockingEvents[0]!.occurrenceHighBound,
    ),
  )
  const [defectRate, setDefectRate] = useState(
    getAverageRate(
      parsedModel.setup.defects[0]!.occurrenceLowBound,
      parsedModel.setup.defects[0]!.occurrenceHighBound,
    ),
  )
  const [pullOrder, setPullOrder] = useState<SimPullOrder>(parsedModel.execute.pullOrder)
  const [stepIndex, setStepIndex] = useState(0)

  const debouncedWipLimit = useDebouncedValue(wipLimit, SIM_DEBOUNCE_MS)
  const debouncedBlockerRate = useDebouncedValue(blockerRate, SIM_DEBOUNCE_MS)
  const debouncedDefectRate = useDebouncedValue(defectRate, SIM_DEBOUNCE_MS)

  useEffect(() => {
    const matchingColumn = selectableColumns.find((column) => column.id === constraintColumnId)
    if (matchingColumn) {
      setWipLimit(matchingColumn.wipLimit)
      return
    }

    if (selectableColumns[0]) {
      setConstraintColumnId(selectableColumns[0].id)
      setWipLimit(selectableColumns[0].wipLimit)
    }
  }, [constraintColumnId, selectableColumns])

  const baseline = useMemo(() => {
    const monteCarlo = runMonteCarlo(parsedModel, LEARNING_LAB_MONTE_CARLO_CYCLES)
    const visual = runVisualSimulation(parsedModel)
    return {
      monteCarlo,
      visual,
      p85Steps: getEightyFive(monteCarlo),
    }
  }, [parsedModel])

  const scenario = useMemo(() => {
    const tunedModel = cloneAndTuneModel({
      baseModel: parsedModel,
      constraintColumnId,
      wipLimit: debouncedWipLimit,
      blockerRate: debouncedBlockerRate,
      defectRate: debouncedDefectRate,
      pullOrder,
    })

    const monteCarlo = runMonteCarlo(tunedModel, LEARNING_LAB_MONTE_CARLO_CYCLES)
    const visual = runVisualSimulation(tunedModel)
    return {
      monteCarlo,
      visual,
      p85Steps: getEightyFive(monteCarlo),
    }
  }, [parsedModel, constraintColumnId, debouncedWipLimit, debouncedBlockerRate, debouncedDefectRate, pullOrder])

  useEffect(() => {
    const finalIndex = Math.max(0, scenario.visual.snapshots.length - 1)
    setStepIndex(Math.min(2, finalIndex))
  }, [scenario.visual.totalSteps, scenario.visual.snapshots.length])

  const averageDeltaPercent =
    ((scenario.monteCarlo.averageSteps - baseline.monteCarlo.averageSteps) /
      Math.max(1, baseline.monteCarlo.averageSteps)) *
    100
  const p85DeltaPercent =
    ((scenario.p85Steps - baseline.p85Steps) / Math.max(1, baseline.p85Steps)) * 100
  const selectedSnapshot =
    scenario.visual.snapshots[Math.min(stepIndex, Math.max(0, scenario.visual.snapshots.length - 1))] ?? null
  const firstDefectSnapshotIndex = findFirstSnapshotWithDefect(scenario.visual.snapshots)

  return (
    <section className="rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5 sm:p-6">
      <h2 className="m-0 text-2xl font-semibold text-[var(--sea-ink)]">Inline Kanban model playground</h2>
      <p className="mt-2 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
        Adjust one flow policy at a time and compare against the baseline model. This uses the same
        KanbanSim engine as the full simulator, but keeps controls embedded directly in article text.
      </p>
      <p className="mt-2 text-xs leading-relaxed text-[var(--sea-ink-soft)]">
        Monte Carlo is capped at {LEARNING_LAB_MONTE_CARLO_CYCLES} cycles on this page so the lesson stays responsive while you experiment.
      </p>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <label className="space-y-2">
          <span className="text-xs font-semibold uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">
            Likely constraint column
          </span>
          <Select
            value={String(constraintColumnId)}
            onValueChange={(value) => setConstraintColumnId(Number(value))}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Choose a column" />
            </SelectTrigger>
            <SelectContent>
              {selectableColumns.map((column) => (
                <SelectItem key={column.id} value={String(column.id)}>
                  {column.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </label>

        <label className="space-y-2">
          <span className="text-xs font-semibold uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">
            Pull policy
          </span>
          <Select value={pullOrder} onValueChange={(value) => setPullOrder(value as SimPullOrder)}>
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Choose a pull strategy" />
            </SelectTrigger>
            <SelectContent>
              {PULL_ORDER_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </label>
      </div>

      <div className="mt-5 space-y-5">
        <div>
          <div className="mb-2 flex items-center justify-between">
            <p className="m-0 text-sm font-semibold text-[var(--sea-ink)]">WIP limit at the constraint</p>
            <p className="m-0 text-sm text-[var(--sea-ink-soft)]">{wipLimit}</p>
          </div>
          <Slider
            min={1}
            max={10}
            step={1}
            value={[wipLimit]}
            onValueChange={(value) => setWipLimit(value[0] ?? 1)}
          />
        </div>

        <div>
          <div className="mb-2 flex items-center justify-between">
            <p className="m-0 text-sm font-semibold text-[var(--sea-ink)]">Blocker rate (%)</p>
            <p className="m-0 text-sm text-[var(--sea-ink-soft)]">{blockerRate}%</p>
          </div>
          <Slider
            min={0}
            max={60}
            step={1}
            value={[blockerRate]}
            onValueChange={(value) => setBlockerRate(value[0] ?? 0)}
          />
        </div>

        <div>
          <div className="mb-2 flex items-center justify-between">
            <p className="m-0 text-sm font-semibold text-[var(--sea-ink)]">Defect arrival rate (%)</p>
            <p className="m-0 text-sm text-[var(--sea-ink-soft)]">{defectRate}%</p>
          </div>
          <Slider
            min={0}
            max={80}
            step={1}
            value={[defectRate]}
            onValueChange={(value) => setDefectRate(value[0] ?? 0)}
          />
        </div>
      </div>

      <div className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <article className="rounded-xl border border-[var(--line)] bg-[var(--bg-base)] p-3">
          <p className="m-0 text-xs uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">Avg completion steps</p>
          <p className="m-0 mt-1 text-2xl font-semibold text-[var(--sea-ink)]">{scenario.monteCarlo.averageSteps.toFixed(1)}</p>
          <p className="m-0 mt-1 text-xs text-[var(--sea-ink-soft)]">Baseline {baseline.monteCarlo.averageSteps.toFixed(1)}</p>
        </article>
        <article className="rounded-xl border border-[var(--line)] bg-[var(--bg-base)] p-3">
          <p className="m-0 text-xs uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">85% confidence steps</p>
          <p className="m-0 mt-1 text-2xl font-semibold text-[var(--sea-ink)]">{scenario.p85Steps}</p>
          <p className="m-0 mt-1 text-xs text-[var(--sea-ink-soft)]">Baseline {baseline.p85Steps}</p>
        </article>
        <article className="rounded-xl border border-[var(--line)] bg-[var(--bg-base)] p-3">
          <p className="m-0 text-xs uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">Average step delta</p>
          <p className="m-0 mt-1 text-2xl font-semibold text-[var(--sea-ink)]">{formatDelta(averageDeltaPercent)}</p>
          <p className="m-0 mt-1 text-xs text-[var(--sea-ink-soft)]">Lower is better for flow</p>
        </article>
        <article className="rounded-xl border border-[var(--line)] bg-[var(--bg-base)] p-3">
          <p className="m-0 text-xs uppercase tracking-[0.08em] text-[var(--sea-ink-soft)]">P85 delta</p>
          <p className="m-0 mt-1 text-2xl font-semibold text-[var(--sea-ink)]">{formatDelta(p85DeltaPercent)}</p>
          <p className="m-0 mt-1 text-xs text-[var(--sea-ink-soft)]">Stability under uncertainty</p>
        </article>
      </div>

      {selectedSnapshot ? (
        <div className="mt-6 rounded-2xl border border-[var(--line)] bg-[color-mix(in_oklab,var(--surface)_90%,white_10%)] p-4 sm:p-5">
          <div className="flex flex-wrap items-end justify-between gap-3">
            <div>
              <p className="sim-board-kicker">Inline board</p>
              <h3 className="mt-1 text-lg font-semibold text-[var(--sea-ink)]">{selectedSnapshot.label}</h3>
              <p className="mt-1 text-sm text-[var(--sea-ink-soft)]">
                Backlog {selectedSnapshot.backlogCount} · Done {selectedSnapshot.doneCount}
                {selectedSnapshot.activePhase ? ` · ${selectedSnapshot.activePhase}` : ''}
              </p>
            </div>
            <span className="rounded-full border border-[var(--line)] bg-[var(--surface-strong)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink-soft)]">
              Step {selectedSnapshot.step} / {scenario.visual.totalSteps}
            </span>
          </div>

          {firstDefectSnapshotIndex !== -1 ? (
            <div className="mt-3 flex flex-wrap gap-2">
              <button
                type="button"
                className="rounded-full border border-[rgba(50,143,151,0.22)] bg-[rgba(79,184,178,0.14)] px-3 py-1 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:bg-[rgba(79,184,178,0.22)]"
                onClick={() => setStepIndex(firstDefectSnapshotIndex)}
              >
                Jump to first defect card
              </button>
            </div>
          ) : null}

          <input
            className="mt-4 w-full accent-[var(--lagoon-deep)]"
            type="range"
            min={0}
            max={Math.max(0, scenario.visual.snapshots.length - 1)}
            step={1}
            value={Math.min(stepIndex, Math.max(0, scenario.visual.snapshots.length - 1))}
            onChange={(event) => setStepIndex(Number(event.target.value))}
          />

          <div className="mt-4">
            <SimBoardView snapshot={selectedSnapshot} compact />
          </div>
        </div>
      ) : null}

      <p className="mt-4 text-xs leading-relaxed text-[var(--sea-ink-soft)]">
        Tip: Push blocker and defect rates high, then tune WIP down on the likely bottleneck. You should
        see lead-time confidence improve when the constrained column stays protected from overload.
      </p>
    </section>
  )
}
