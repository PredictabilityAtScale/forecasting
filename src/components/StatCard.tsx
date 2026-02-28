/**
 * Reusable stat card used across multiple tool pages.
 */
export default function StatCard({
  label,
  value,
}: {
  label: string
  value: number
}) {
  return (
    <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3 text-center">
      <p className="m-0 text-lg font-bold tabular-nums text-[var(--sea-ink)]">
        {value}
      </p>
      <p className="m-0 text-[10px] font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
        {label}
      </p>
    </div>
  )
}
