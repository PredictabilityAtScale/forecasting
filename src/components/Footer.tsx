import { Link } from '@tanstack/react-router'

export default function Footer() {
  const year = new Date().getFullYear()

  return (
    <footer className="border-t border-[var(--line)] bg-[var(--header-bg)] px-4 pb-8 pt-8 backdrop-blur-sm">
      <div className="mx-auto flex max-w-7xl flex-col items-center gap-6 sm:flex-row sm:justify-between">
        {/* Brand + copyright */}
        <div className="flex flex-col items-center gap-1 sm:items-start">
          <div className="flex items-center gap-2">
            <img
              src="/fo_transparent.png"
              alt="Focused Objective"
              className="h-6 w-6 rounded object-contain"
            />
            <span className="text-sm font-semibold text-[var(--sea-ink)]">
              Focused Objective LLC
            </span>
          </div>
          <p className="m-0 text-xs text-[var(--sea-ink-soft)]">
            &copy; {year} Focused Objective LLC. All rights reserved.
          </p>
        </div>

        {/* Quick links */}
        <div className="flex items-center gap-4 text-xs font-medium text-[var(--sea-ink-soft)]">
          <Link to="/throughput" className="transition hover:text-[var(--sea-ink)]">
            Throughput
          </Link>
          <Link to="/multi-feature" className="transition hover:text-[var(--sea-ink)]">
            Multi-Feature
          </Link>
          <Link to="/story-count" className="transition hover:text-[var(--sea-ink)]">
            Story Count
          </Link>
          <Link to="/wrong-order" className="transition hover:text-[var(--sea-ink)]">
            Wrong Order
          </Link>
          <Link to="/latent-defect" className="transition hover:text-[var(--sea-ink)]">
            Latent Defect
          </Link>
          <Link to="/capability-matrix" className="transition hover:text-[var(--sea-ink)]">
            Capability
          </Link>
          <Link to="/team-dashboard" className="transition hover:text-[var(--sea-ink)]">
            Dashboard
          </Link>
        </div>
      </div>
    </footer>
  )
}
