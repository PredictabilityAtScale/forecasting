import { Link } from '@tanstack/react-router'
import { useState } from 'react'
import ColorThemePicker from './ColorThemePicker'
import ThemeToggle from './ThemeToggle'

const SCHEDULE_CALL_URL = 'https://freebusy.io/data'

export default function Header() {
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-50 border-b border-[var(--line)] bg-[var(--header-bg)] backdrop-blur-lg">
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        {/* Logo + brand */}
        <Link
          to="/"
          className="flex items-center gap-2.5 no-underline"
          onClick={() => setMobileOpen(false)}
        >
          <img
            src="/fo_transparent.png"
            alt="Focused Objective"
            className="h-8 w-8 rounded-lg object-contain"
          />
          <span className="text-base font-bold tracking-tight text-[var(--sea-ink)] sm:text-lg">
            Focused Objective
          </span>
        </Link>

        {/* Desktop nav */}
        <div className="hidden items-center gap-2 md:flex">
          <div className="group relative">
            <button
              type="button"
              className="nav-link inline-flex items-center gap-1 rounded-lg px-3 py-2 text-sm font-semibold"
              aria-haspopup="menu"
            >
              Forecasting Tools
              <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M3.22 5.97a.75.75 0 0 1 1.06 0L8 9.69l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L3.22 7.03a.75.75 0 0 1 0-1.06Z" />
              </svg>
            </button>
            <div className="invisible absolute left-0 top-full z-50 mt-1 min-w-64 rounded-xl border border-[var(--line)] bg-[var(--surface)] p-2 opacity-0 shadow-sm transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
              <Link
                to="/throughput"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Throughput Forecaster
              </Link>
              <Link
                to="/multi-feature"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Multiple Feature Cut-Line Forecaster
              </Link>
              <Link
                to="/story-count"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Story Count Forecaster
              </Link>
              <Link
                to="/latent-defect"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Latent Defects
              </Link>
            </div>
          </div>

          <div className="group relative">
            <button
              type="button"
              className="nav-link inline-flex items-center gap-1 rounded-lg px-3 py-2 text-sm font-semibold"
              aria-haspopup="menu"
            >
              Metrics
              <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M3.22 5.97a.75.75 0 0 1 1.06 0L8 9.69l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L3.22 7.03a.75.75 0 0 1 0-1.06Z" />
              </svg>
            </button>
            <div className="invisible absolute left-0 top-full z-50 mt-1 min-w-56 rounded-xl border border-[var(--line)] bg-[var(--surface)] p-2 opacity-0 shadow-sm transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
              <Link
                to="/team-dashboard"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Team Dashboard
              </Link>
            </div>
          </div>

          <div className="group relative">
            <button
              type="button"
              className="nav-link inline-flex items-center gap-1 rounded-lg px-3 py-2 text-sm font-semibold"
              aria-haspopup="menu"
            >
              Others
              <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M3.22 5.97a.75.75 0 0 1 1.06 0L8 9.69l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L3.22 7.03a.75.75 0 0 1 0-1.06Z" />
              </svg>
            </button>
            <div className="invisible absolute left-0 top-full z-50 mt-1 min-w-56 rounded-xl border border-[var(--line)] bg-[var(--surface)] p-2 opacity-0 shadow-sm transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
              <Link
                to="/wrong-order"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Wrong Order-O-Meter
              </Link>
              <Link
                to="/capability-matrix"
                className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              >
                Capability Matrix
              </Link>
            </div>
          </div>

          <a
            href="https://learn.focusedobjective.com"
            target="_blank"
            rel="noopener noreferrer"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold no-underline"
          >
            Training ↗
          </a>

          <a
            href={SCHEDULE_CALL_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
          >
            Schedule free call
          </a>
          <div className="mx-2 h-5 w-px bg-[var(--line)]" />
          <ColorThemePicker />
          <ThemeToggle />
        </div>

        {/* Mobile hamburger */}
        <div className="flex items-center gap-2 md:hidden">

          <a
            href="https://learn.focusedobjective.com"
            target="_blank"
            rel="noopener noreferrer"
            className="mt-2 block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            onClick={() => setMobileOpen(false)}
          >
            Training ↗
          </a>
          <ColorThemePicker />
          <ThemeToggle />
          <button
            onClick={() => setMobileOpen(!mobileOpen)}
            className="rounded-lg p-2 text-[var(--sea-ink-soft)] transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            aria-label="Toggle menu"
          >
            {mobileOpen ? (
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            ) : (
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                <line x1="3" y1="6" x2="21" y2="6" />
                <line x1="3" y1="12" x2="21" y2="12" />
                <line x1="3" y1="18" x2="21" y2="18" />
              </svg>
            )}
          </button>
        </div>
      </nav>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="border-t border-[var(--line)] bg-[var(--header-bg)] px-4 pb-4 pt-2 md:hidden">
          <p className="px-3 pb-1 pt-2 text-[10px] font-bold uppercase tracking-[0.16em] text-[var(--sea-ink-soft)]">
            Forecasting Tools
          </p>
          <Link
            to="/throughput"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Throughput Forecaster
          </Link>
          <Link
            to="/multi-feature"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Multiple Feature Cut-Line Forecaster
          </Link>
          <Link
            to="/story-count"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Story Count Forecaster
          </Link>
          <Link
            to="/latent-defect"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Latent Defects
          </Link>

          <p className="px-3 pb-1 pt-3 text-[10px] font-bold uppercase tracking-[0.16em] text-[var(--sea-ink-soft)]">
            Metrics
          </p>
          <Link
            to="/team-dashboard"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Team Dashboard
          </Link>

          <p className="px-3 pb-1 pt-3 text-[10px] font-bold uppercase tracking-[0.16em] text-[var(--sea-ink-soft)]">
            Others
          </p>
          <Link
            to="/wrong-order"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Wrong Order-O-Meter
          </Link>
          <Link
            to="/capability-matrix"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Capability Matrix
          </Link>
          <a
            href="https://learn.focusedobjective.com"
            target="_blank"
            rel="noopener noreferrer"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            onClick={() => setMobileOpen(false)}
          >
            Training ↗
          </a>

          <a
            href={SCHEDULE_CALL_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="mt-3 block rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2.5 text-center text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:bg-[rgba(79,184,178,0.24)]"
            onClick={() => setMobileOpen(false)}
          >
            Schedule free call
          </a>
        </div>
      )}
    </header>
  )
}
