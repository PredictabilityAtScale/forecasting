import { createFileRoute, Link } from '@tanstack/react-router'
import { TRAINING_URL } from '#/lib/site'

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
              to="/throughput"
              className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-6 py-2.5 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
            >
              Get Started
            </Link>
            <a
              href={TRAINING_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="rounded-full border border-[var(--line)] bg-[var(--surface)] px-6 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline transition hover:-translate-y-0.5 hover:bg-[var(--surface-strong)]"
            >
              Training on Forecasting &amp; Metrics ↗
            </a>
          </div>
        </div>
      </section>

      {/* Tool cards */}
      <section className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        <ToolCard
          title="Throughput Forecaster"
          description="Given historical throughput data, forecast how many items a team can deliver in a time window — with percentile-based confidence levels."
          href="/throughput"
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
          href="/multi-feature"
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
          href="/story-count"
          delay={180}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
              <rect x="9" y="3" width="6" height="4" rx="1" />
              <path d="M9 14l2 2 4-4" />
            </svg>
          }
        />
        <ToolCard
          title="Wrong Order-O-Meter"
          description="Measure how far actual delivery order diverged from the plan. Accounts for re-ordering, unplanned work, and undelivered items."
          href="/wrong-order"
          delay={270}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10" />
              <path d="M12 6v6l4 2" />
            </svg>
          }
        />
        <ToolCard
          title="Latent Defect Estimation"
          description="Estimate how many defects remain undiscovered using the Lincoln-Petersen capture-recapture method with two independent test groups."
          href="/latent-defect"
          delay={360}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2" />
              <path d="M12 8v4" />
              <circle cx="12" cy="16" r="0.5" fill="currentColor" />
            </svg>
          }
        />
        <ToolCard
          title="Capability Matrix"
          description="Assess your team's skill levels across technologies, identify capability gaps, risk areas, and plan training investments with urgency matrices."
          href="/capability-matrix"
          delay={450}
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
          title="Team Dashboard"
          description="Visualize flow metrics across six dimensions: throughput, cycle time, WIP, defect rate, cumulative flow, and predictability."
          href="/team-dashboard"
          delay={540}
          icon={
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2" />
              <path d="M3 9h18" />
              <path d="M9 21V9" />
              <path d="M13 15l2-2 2 2" />
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

      {/* Training CTA */}
      <section className="island-shell mt-10 rounded-2xl p-6 sm:p-8">
        <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-start sm:gap-8">
          <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-xl border border-[var(--line)] bg-[var(--surface-strong)]">
            <svg className="h-7 w-7 text-[var(--lagoon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M22 10v6M2 10l10-5 10 5-10 5z" />
              <path d="M6 12v5c0 1.66 2.69 3 6 3s6-1.34 6-3v-5" />
            </svg>
          </div>
          <div className="flex-1 text-center sm:text-left">
            <p className="island-kicker mb-2">Level Up Your Team</p>
            <h2 className="mb-2 text-xl font-bold text-[var(--sea-ink)] sm:text-2xl">
              Training on Forecasting &amp; Metrics
            </h2>
            <p className="mb-4 max-w-xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Learn how to apply Monte Carlo simulation, flow metrics, and
              probabilistic forecasting in your organisation. Workshops and
              coaching from the team behind these tools.
            </p>
            <a
              href={TRAINING_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1.5 rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-6 py-2.5 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
            >
              Visit focusedobjective.com
              <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor">
                <path d="M4.5 2a.5.5 0 0 0 0 1h6.793L2.146 12.146a.5.5 0 0 0 .708.708L12 3.707V10.5a.5.5 0 0 0 1 0v-9a.5.5 0 0 0-.5-.5h-8Z" />
              </svg>
            </a>
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
