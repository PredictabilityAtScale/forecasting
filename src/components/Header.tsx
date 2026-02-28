import { Link } from '@tanstack/react-router'
import { useState } from 'react'
import ColorThemePicker from './ColorThemePicker'
import ThemeToggle from './ThemeToggle'

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
        <div className="hidden items-center gap-1 md:flex">
          <Link
            to="/throughput"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Throughput
          </Link>
          <Link
            to="/multi-feature"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Multi-Feature Cut Line
          </Link>
          <Link
            to="/story-count"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Story Count
          </Link>
          <Link
            to="/wrong-order"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Wrong Order
          </Link>
          <Link
            to="/latent-defect"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Latent Defect
          </Link>
          <Link
            to="/capability-matrix"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Capability
          </Link>
          <Link
            to="/team-dashboard"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold"
            activeProps={{ className: 'nav-link is-active rounded-lg px-3 py-2 text-sm font-semibold' }}
          >
            Dashboard
          </Link>
          <div className="mx-2 h-5 w-px bg-[var(--line)]" />
          <ColorThemePicker />
          <ThemeToggle />
        </div>

        {/* Mobile hamburger */}
        <div className="flex items-center gap-2 md:hidden">
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
            Multi-Feature Cut Line
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
            to="/wrong-order"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Wrong Order-O-Meter
          </Link>
          <Link
            to="/latent-defect"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Latent Defect Estimation
          </Link>
          <Link
            to="/capability-matrix"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Capability Matrix
          </Link>
          <Link
            to="/team-dashboard"
            className="block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
            activeProps={{ className: 'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]' }}
            onClick={() => setMobileOpen(false)}
          >
            Team Dashboard
          </Link>
        </div>
      )}
    </header>
  )
}
