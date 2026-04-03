import { createFileRoute, Link } from '@tanstack/react-router'
import KanbanFlowPlayground from '#/components/kanban-flow-playground'
import { loadSimulationExamples } from '#/lib/kanban-scrum-sim'

export const Route = createFileRoute('/kanban-flow-learning')({
  component: KanbanFlowLearningPage,
})

const EXAMPLES = loadSimulationExamples()
const BASE_EXAMPLE =
  EXAMPLES.find((example) => example.path.includes('Lean or Kanban Template.simml')) ?? EXAMPLES[0]

function KanbanFlowLearningPage() {
  return (
    <main className="mx-auto max-w-5xl px-4 pb-16 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-3xl px-6 py-8 sm:px-10 sm:py-10">
        <p className="island-kicker m-0">KanbanSim Learning Lab</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          WIP Limits, Constraints, and Flow Stability
        </h1>
        <p className="mt-3 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)] sm:text-base">
          This is the first example page in a planned series of Kanban education articles. It keeps the
          simulation model inline with the lesson, so teams can read a concept and immediately test policy
          changes in-place (similar to a notebook workflow).
        </p>

        <div className="mt-6 rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5">
          <h2 className="m-0 text-xl font-semibold text-[var(--sea-ink)]">Why this concept matters</h2>
          <ul className="mb-0 mt-3 space-y-2 pl-5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
            <li>
              WIP limits reduce queue growth and protect the likely bottleneck from overload.
            </li>
            <li>
              The true constraint may shift between columns based on blockers, defects, and pull behavior.
            </li>
            <li>
              Better flow policy usually improves both average delivery time and confidence ranges.
            </li>
          </ul>
        </div>

        <div className="mt-6">
          <KanbanFlowPlayground source={BASE_EXAMPLE.source} />
        </div>

        <article className="prose prose-sm mt-8 max-w-none text-[var(--sea-ink-soft)]">
          <h2 className="text-[var(--sea-ink)]">How to use this lesson</h2>
          <ol>
            <li>
              Start with the default settings and note the baseline average and 85% completion steps.
            </li>
            <li>
              Pick a likely constraint column and lower WIP one step at a time.
            </li>
            <li>
              Increase blocker and defect rates to simulate poor operating conditions.
            </li>
            <li>
              Switch pull policy to compare predictable flow (FIFO) versus noisy pull policies.
            </li>
          </ol>
          <p>
            As we add more pages, this same inline simulation component can be reused for dedicated lessons
            on blocker strategy, defect containment, and sizing/aging behavior without sending users away to
            a separate simulator tab.
          </p>
        </article>

        <div className="mt-8 flex flex-wrap gap-3">
          <Link
            to="/kanban-scrum-sim"
            className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-5 py-2 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:bg-[rgba(79,184,178,0.24)]"
          >
            Open full KanbanSim
          </Link>
          <Link
            to="/simml-reference"
            className="rounded-full border border-[var(--line)] bg-[var(--surface)] px-5 py-2 text-sm font-semibold text-[var(--sea-ink)] no-underline transition hover:bg-[var(--surface-strong)]"
          >
            Review SimML reference
          </Link>
        </div>
      </section>
    </main>
  )
}
