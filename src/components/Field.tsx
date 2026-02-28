/**
 * Reusable form field wrapper used across all forecaster pages.
 */
export default function Field({
  label,
  inline,
  children,
}: {
  label: string
  inline?: boolean
  children: React.ReactNode
}) {
  return (
    <label
      className={
        inline
          ? 'inline-flex flex-col gap-1'
          : 'flex flex-col gap-1'
      }
    >
      <span className="text-xs font-medium text-[var(--sea-ink-soft)]">
        {label}
      </span>
      {children}
    </label>
  )
}
