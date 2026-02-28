import { useEffect, useRef, useState } from 'react'

export type ColorTheme = 'lagoon' | 'blue' | 'indigo' | 'amber' | 'rose'

const COLOR_THEMES: { id: ColorTheme; label: string; swatch: string }[] = [
  { id: 'lagoon', label: 'Lagoon', swatch: '#4fb8b2' },
  { id: 'blue', label: 'Blue', swatch: '#3b82f6' },
  { id: 'indigo', label: 'Indigo', swatch: '#8b5cf6' },
  { id: 'amber', label: 'Amber', swatch: '#f59e0b' },
  { id: 'rose', label: 'Rose', swatch: '#f43f5e' },
]

function getStoredColor(): ColorTheme {
  if (typeof window === 'undefined') return 'lagoon'
  const stored = window.localStorage.getItem('color-theme')
  if (COLOR_THEMES.some((t) => t.id === stored)) return stored as ColorTheme
  return 'lagoon'
}

export function applyColorTheme(theme: ColorTheme) {
  const root = document.documentElement
  if (theme === 'lagoon') {
    root.removeAttribute('data-color')
  } else {
    root.setAttribute('data-color', theme)
  }
}

export default function ColorThemePicker() {
  const [color, setColor] = useState<ColorTheme>('lagoon')
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const initial = getStoredColor()
    setColor(initial)
    applyColorTheme(initial)
  }, [])

  // Close on click outside
  useEffect(() => {
    if (!open) return
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [open])

  function pick(theme: ColorTheme) {
    setColor(theme)
    applyColorTheme(theme)
    window.localStorage.setItem('color-theme', theme)
    setOpen(false)
  }

  const current = COLOR_THEMES.find((t) => t.id === color)!

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen(!open)}
        aria-label={`Color theme: ${current.label}. Click to change.`}
        title={`Color theme: ${current.label}`}
        className="flex items-center gap-1.5 rounded-full border border-[var(--chip-line)] bg-[var(--chip-bg)] px-2.5 py-1.5 text-xs font-semibold text-[var(--sea-ink)] shadow-[0_4px_12px_rgba(0,0,0,0.06)] transition hover:-translate-y-0.5"
      >
        <span
          className="h-3 w-3 rounded-full ring-1 ring-black/10"
          style={{ backgroundColor: current.swatch }}
        />
        <svg
          className={`h-3 w-3 transition-transform ${open ? 'rotate-180' : ''}`}
          viewBox="0 0 12 12"
          fill="currentColor"
        >
          <path d="M2.22 4.22a.75.75 0 0 1 1.06 0L6 6.94l2.72-2.72a.75.75 0 1 1 1.06 1.06l-3.25 3.25a.75.75 0 0 1-1.06 0L2.22 5.28a.75.75 0 0 1 0-1.06Z" />
        </svg>
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 min-w-[140px] rounded-xl border border-[var(--line)] bg-[var(--header-bg)] p-1.5 shadow-lg backdrop-blur-lg">
          {COLOR_THEMES.map((theme) => (
            <button
              key={theme.id}
              type="button"
              onClick={() => pick(theme.id)}
              className={`flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-left text-sm transition
                ${
                  color === theme.id
                    ? 'bg-[var(--link-bg-hover)] font-semibold text-[var(--sea-ink)]'
                    : 'text-[var(--sea-ink-soft)] hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]'
                }`}
            >
              <span
                className="h-3.5 w-3.5 rounded-full ring-1 ring-black/10"
                style={{ backgroundColor: theme.swatch }}
              />
              {theme.label}
              {color === theme.id && (
                <svg className="ml-auto h-4 w-4" viewBox="0 0 16 16" fill="currentColor">
                  <path d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.75.75 0 0 1 1.06-1.06L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z" />
                </svg>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
