import { Link } from '@tanstack/react-router'
import { useState } from 'react'
import ColorThemePicker from './ColorThemePicker'
import ThemeToggle from './ThemeToggle'

const forecastingLinks = [
  { label: 'Throughput Forecaster', to: '/throughput' },
  { label: 'Multiple Feature Cut-Line Forecaster', to: '/multi-feature' },
  { label: 'Story Count Forecaster', to: '/story-count' },
  { label: 'Latent Defects', to: '/latent-defect' },
  { label: 'KanbanSim', to: '/kanban-scrum-sim' },
  { label: 'ScrumSim', to: '/kanban-scrum-sim' },
]

const forecastingResourceLinks = [
  {
    label: 'Adding More Teams or People',
    href: 'https://observablehq.com/@troymagennis/how-much-improvement-do-i-get-by-adding-more-teams-or-people?collection=@troymagennis/agile-software-development',
  },
  {
    label: 'Velocity vs Throughput Forecasting',
    href: 'https://observablehq.com/@troymagennis/story-point-velocity-or-throughput-forecasting-does-it-mat?collection=@troymagennis/agile-software-development',
  },
  {
    label: 'Understanding Optimal Batch Size',
    href: 'https://observablehq.com/@troymagennis/understanding-optimal-batch-size?collection=@troymagennis/agile-software-development',
  },
  {
    label: 'Throughput Forecaster Spreadsheet',
    href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Throughput%20Forecaster.xlsx',
  },
  {
    label: 'Multiple Feature Cut Line Spreadsheet',
    href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Multiple%20Feature%20Cut%20Line%20Forecaster.xlsx',
  },
  {
    label: 'Time Series Forecasting for Demand',
    href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Demand%20Forecasting.xlsx',
  },
  {
    label: 'KanbanSim and ScrumSim App',
    href: 'https://llmasaservice.io/wp-content/uploads/2025/01/KanbanSimv4.zip',
  },
  {
    label: 'More Agile Data Notebooks',
    href: 'https://observablehq.com/collection/@troymagennis/agile-software-development',
  },
]

const metricLinks = [
  { label: 'Team Dashboard', to: '/team-dashboard' },
  { label: 'Wrong Order-O-Meter', to: '/wrong-order' },
  { label: 'Voting Scorer', to: '/voting' },
  { label: 'Capability Matrix', to: '/capability-matrix' },
]

const metricResourceLinks = [
  {
    label: 'Utilization Impact on Lead Time',
    href: 'https://observablehq.com/@troymagennis/how-does-utilization-impact-lead-time-of-work?collection=@troymagennis/agile-software-development',
  },
  {
    label: 'Multiple Team Dependencies',
    href: 'https://observablehq.com/@troymagennis/impact-of-multiple-team-dependencies-in-software-developm?collection=@troymagennis/agile-software-development',
  },
  {
    label: 'Team Dashboard Spreadsheet',
    href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Team%20Dashboard.xlsx',
  },
  {
    label: 'Skill and Capability Survey',
    href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Capability%20Matrix%20v2.xlsx',
  },
  {
    label: 'More Agile Data Notebooks',
    href: 'https://observablehq.com/collection/@troymagennis/agile-software-development',
  },
  {
    label: 'More Spreadsheet Tools',
    href: 'https://bit.ly/SimResources',
  },
]

const desktopMenuLinkClassName =
  'block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]'

const mobileMenuLinkClassName =
  'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]'

const mobileActiveClassName =
  'block rounded-lg px-3 py-2.5 text-sm font-semibold text-[var(--sea-ink)] no-underline bg-[var(--link-bg-hover)]'

type InternalMenuLink = {
  label: string
  to: string
}

type ExternalMenuLink = {
  label: string
  href: string
}

function DesktopInternalLinks({ links }: { links: InternalMenuLink[] }) {
  return links.map((link) => (
    <Link key={link.label} to={link.to} className={desktopMenuLinkClassName}>
      {link.label}
    </Link>
  ))
}

function DesktopResourceSubmenu({
  label,
  links,
}: {
  label: string
  links: ExternalMenuLink[]
}) {
  return (
    <div className="group/submenu relative mt-2 border-t border-[var(--line)] pt-2">
      <button
        type="button"
        className="flex w-full items-center justify-between rounded-lg px-3 py-2 text-left text-sm font-semibold text-[var(--sea-ink-soft)] transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
        aria-haspopup="menu"
      >
        {label}
        <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
          <path d="M5.97 12.78a.75.75 0 0 1 0-1.06L9.69 8 5.97 4.28a.75.75 0 1 1 1.06-1.06l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0Z" />
        </svg>
      </button>
      <div className="invisible absolute left-full top-0 z-50 ml-1 max-h-[26rem] w-80 overflow-y-auto rounded-xl border border-[var(--line)] bg-[var(--surface)] p-2 opacity-0 shadow-sm transition group-hover/submenu:visible group-hover/submenu:opacity-100 group-focus-within/submenu:visible group-focus-within/submenu:opacity-100">
        {links.map((link) => (
          <a
            key={link.label}
            href={link.href}
            target="_blank"
            rel="noopener noreferrer"
            className={desktopMenuLinkClassName}
          >
            {link.label} ↗
          </a>
        ))}
      </div>
    </div>
  )
}

function MobileInternalLinks({
  links,
  onNavigate,
}: {
  links: InternalMenuLink[]
  onNavigate: () => void
}) {
  return links.map((link) => (
    <Link
      key={link.label}
      to={link.to}
      className={mobileMenuLinkClassName}
      activeProps={{ className: mobileActiveClassName }}
      onClick={onNavigate}
    >
      {link.label}
    </Link>
  ))
}

function MobileResourceSubmenu({
  label,
  links,
  open,
  onToggle,
  onNavigate,
}: {
  label: string
  links: ExternalMenuLink[]
  open: boolean
  onToggle: () => void
  onNavigate: () => void
}) {
  return (
    <div className="mt-2 border-t border-[var(--line)] pt-2">
      <button
        type="button"
        onClick={onToggle}
        className="flex w-full items-center justify-between rounded-lg px-3 py-2.5 text-left text-sm font-semibold text-[var(--sea-ink-soft)] transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
        aria-expanded={open}
      >
        {label}
        <svg
          className={`h-3.5 w-3.5 transition ${open ? 'rotate-180' : ''}`}
          viewBox="0 0 16 16"
          fill="currentColor"
          aria-hidden="true"
        >
          <path d="M3.22 5.97a.75.75 0 0 1 1.06 0L8 9.69l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L3.22 7.03a.75.75 0 0 1 0-1.06Z" />
        </svg>
      </button>
      {open && (
        <div className="mt-1 space-y-1 px-2 pb-1">
          {links.map((link) => (
            <a
              key={link.label}
              href={link.href}
              target="_blank"
              rel="noopener noreferrer"
              className="block rounded-lg px-3 py-2 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
              onClick={onNavigate}
            >
              {link.label} ↗
            </a>
          ))}
        </div>
      )}
    </div>
  )
}

export default function Header() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const [mobileForecastingResourcesOpen, setMobileForecastingResourcesOpen] = useState(false)
  const [mobileMetricResourcesOpen, setMobileMetricResourcesOpen] = useState(false)

  function closeMobileMenu() {
    setMobileOpen(false)
    setMobileForecastingResourcesOpen(false)
    setMobileMetricResourcesOpen(false)
  }

  function toggleMobileMenu() {
    if (mobileOpen) {
      closeMobileMenu()
      return
    }

    setMobileOpen(true)
  }

  return (
    <header className="sticky top-0 z-50 border-b border-[var(--line)] bg-[var(--header-bg)] backdrop-blur-lg">
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        {/* Logo + brand */}
        <Link
          to="/"
          className="flex items-center gap-2.5 no-underline"
          onClick={closeMobileMenu}
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
              <DesktopInternalLinks links={forecastingLinks} />
              <DesktopResourceSubmenu
                label="Forecasting Spreadsheets & Articles"
                links={forecastingResourceLinks}
              />
            </div>
          </div>

          <div className="group relative">
            <button
              type="button"
              className="nav-link inline-flex items-center gap-1 rounded-lg px-3 py-2 text-sm font-semibold"
              aria-haspopup="menu"
            >
              Metric Tools
              <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M3.22 5.97a.75.75 0 0 1 1.06 0L8 9.69l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L3.22 7.03a.75.75 0 0 1 0-1.06Z" />
              </svg>
            </button>
            <div className="invisible absolute left-0 top-full z-50 mt-1 min-w-56 rounded-xl border border-[var(--line)] bg-[var(--surface)] p-2 opacity-0 shadow-sm transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
              <DesktopInternalLinks links={metricLinks} />
              <DesktopResourceSubmenu
                label="Metrics Spreadsheets & Articles"
                links={metricResourceLinks}
              />
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
          <Link
            to="/articles"
            className="nav-link rounded-lg px-3 py-2 text-sm font-semibold no-underline"
          >
            Articles
          </Link>
          <Link
            to="/contact"
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
          >
            Contact me
          </Link>
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
            onClick={closeMobileMenu}
          >
            Training ↗
          </a>
          <ColorThemePicker />
          <ThemeToggle />
          <button
            onClick={toggleMobileMenu}
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
          <MobileInternalLinks links={forecastingLinks} onNavigate={closeMobileMenu} />
          <MobileResourceSubmenu
            label="Forecasting Spreadsheets & Articles"
            links={forecastingResourceLinks}
            open={mobileForecastingResourcesOpen}
            onToggle={() => setMobileForecastingResourcesOpen((current) => !current)}
            onNavigate={closeMobileMenu}
          />

          <p className="px-3 pb-1 pt-3 text-[10px] font-bold uppercase tracking-[0.16em] text-[var(--sea-ink-soft)]">
            Metric Tools
          </p>
          <MobileInternalLinks links={metricLinks} onNavigate={closeMobileMenu} />
          <MobileResourceSubmenu
            label="Metrics Spreadsheets & Articles"
            links={metricResourceLinks}
            open={mobileMetricResourcesOpen}
            onToggle={() => setMobileMetricResourcesOpen((current) => !current)}
            onNavigate={closeMobileMenu}
          />
          <a
            href="https://learn.focusedobjective.com"
            target="_blank"
            rel="noopener noreferrer"
            className={mobileMenuLinkClassName}
            onClick={closeMobileMenu}
          >
            Training ↗
          </a>
          <Link
            to="/articles"
            className={mobileMenuLinkClassName}
            activeProps={{ className: mobileActiveClassName }}
            onClick={closeMobileMenu}
          >
            Articles
          </Link>
          <Link
            to="/contact"
            className="mt-3 block rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2.5 text-center text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:bg-[rgba(79,184,178,0.24)]"
            onClick={closeMobileMenu}
          >
            Contact me
          </Link>
        </div>
      )}
    </header>
  )
}
