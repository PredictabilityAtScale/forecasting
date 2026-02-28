import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/')({ component: App })

function App() {
  return (
    <main className="mx-auto max-w-7xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      {/* Hero */}
      <section className="island-shell rise-in relative overflow-hidden rounded-3xl px-6 py-12 sm:px-12 sm:py-16 lg:py-20">
        <div className="pointer-events-none absolute -left-24 -top-28 h-64 w-64 rounded-full bg-[radial-gradient(circle,rgba(79,184,178,0.30),transparent_66%)]" />
        <div className="pointer-events-none absolute -bottom-24 -right-24 h-64 w-64 rounded-full bg-[radial-gradient(circle,rgba(47,106,74,0.16),transparent_66%)]" />

        <div className="relative flex flex-col items-center text-center">
          <img
            src="/fo_transparent.png"
            alt="Focused Objective"
            className="mb-6 h-20 w-20 rounded-2xl object-contain sm:h-24 sm:w-24"
          />
          <p className="island-kicker mb-3">Monte Carlo Forecasting</p>
          <h1 className="display-title mb-5 max-w-3xl text-3xl leading-tight font-bold tracking-tight text-[var(--sea-ink)] sm:text-5xl lg:text-6xl">
            Predict delivery with confidence, not guesswork.
          </h1>
          <p className="mb-8 max-w-2xl text-base text-[var(--sea-ink-soft)] sm:text-lg">
            Focused Objective's probabilistic forecasting tools help agile teams
            answer "when will it be done?" using Monte Carlo simulation —
            no story points required.
          </p>
          <div className="flex flex-wrap justify-center gap-3">
            <Link
              to="/forecaster/throughput"
              className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-6 py-2.5 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
            >
              Get Started
            </Link>
          </div>
        </div>
      </section>

      {/* Tool cards */}
      <section className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        <ToolCard
          title="Throughput Forecaster"
          description="Given historical throughput data, forecast how many items a team can deliver in a time window — with percentile-based confidence levels."
          href="/forecaster/throughput"
          delay={0}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M3 3v18h18" />
              <path d="m7 16 4-8 4 5 5-9" />
            </svg>
          }
        />
        <ToolCard
          title="Multi-Feature Cut Line"
          description="Prioritize a backlog of features and see which ones are likely to ship by a target date, with a clear probabilistic cut line."
          href="/forecaster/multi-feature"
          delay={90}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="3" width="7" height="7" rx="1" />
              <rect x="14" y="3" width="7" height="7" rx="1" />
              <rect x="3" y="14" width="7" height="7" rx="1" />
              <rect x="14" y="14" width="7" height="7" rx="1" />
            </svg>
          }
        />
        <ToolCard
          title="Story Count Forecaster"
          description="Estimate total story count from a partially-estimated backlog using reference-class forecasting and Monte Carlo simulation."
          href="/forecaster/story-count"
          delay={180}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
              <rect x="9" y="3" width="6" height="4" rx="1" />
              <path d="M9 14l2 2 4-4" />
            </svg>
          }
        />
      </section>

      {/* Value prop */}
      <section className="island-shell mt-10 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">Why Monte Carlo?</p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              No Estimation Required
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Use actual throughput data instead of story point estimates.
              Let the math do the heavy lifting.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Probabilistic Answers
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Get confidence intervals (50th, 85th, 95th percentile) instead of
              single-point estimates that always miss.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Runs in Your Browser
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              All simulations run client-side. Your data never leaves
              your machine.
            </p>
          </div>
        </div>
      </section>
    </main>
  )
}

function ToolCard({
  title,
  description,
  href,
  delay,
  icon,
}: {
  title: string
  description: string
  href: string
  delay: number
  icon: React.ReactNode
}) {
  return (
    <Link
      to={href}
      className="island-shell feature-card rise-in group flex flex-col rounded-2xl p-6 no-underline"
      style={{ animationDelay: `${delay + 80}ms` }}
    >
      <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl border border-[var(--line)] bg-[var(--surface-strong)]">
        {icon}
      </div>
      <h2 className="mb-2 text-lg font-semibold text-[var(--sea-ink)] group-hover:text-[var(--lagoon-deep)]">
        {title}
      </h2>
      <p className="m-0 flex-1 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
        {description}
      </p>
      <span className="mt-4 inline-flex items-center gap-1 text-xs font-semibold text-[var(--lagoon-deep)]">
        Open tool
        <svg className="h-3.5 w-3.5 transition group-hover:translate-x-0.5" viewBox="0 0 16 16" fill="currentColor">
          <path d="M6.22 3.22a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.75.75 0 0 1-1.06-1.06L9.94 8 6.22 4.28a.75.75 0 0 1 0-1.06Z" />
        </svg>
      </span>
    </Link>
  )
}
