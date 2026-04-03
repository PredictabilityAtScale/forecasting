import type { BoardSnapshot } from '#/lib/kanban-scrum-sim'
import { cn } from '#/lib/utils'

interface SimBoardViewProps {
  snapshot: BoardSnapshot
  compact?: boolean
  maxCardsPerColumn?: number
}

export function SimBoardView({
  snapshot,
  compact = false,
  maxCardsPerColumn = compact ? 10 : 18,
}: SimBoardViewProps) {
  return (
    <div className="overflow-x-auto">
      <div
        className={cn('grid gap-4', compact && 'gap-3')}
        style={{
          gridTemplateColumns: `repeat(${snapshot.columns.length}, minmax(${compact ? 150 : 200}px, 1fr))`,
          minWidth: `${snapshot.columns.length * (compact ? 170 : 220)}px`,
        }}
      >
        {snapshot.columns.map((column) => {
          const orderedCards = [...column.cards].sort(compareBoardCards)

          return (
          <section key={column.id} className={cn('sim-board-column', compact && 'rounded-[1.1rem] p-3')}>
            <div className="flex items-center justify-between gap-2">
              <div>
                <h3 className={cn('font-semibold tracking-[0.01em] text-[var(--sea-ink)]', compact ? 'text-xs' : 'text-sm')}>
                  {column.label}
                </h3>
                <p className={cn('font-medium text-[var(--sea-ink-soft)]', compact ? 'text-[10px]' : 'text-[11px]')}>
                  {column.cards.length} items{column.wipLimit ? ` / WIP ${column.wipLimit}` : ''}
                </p>
              </div>
            </div>
            <div className={cn('mt-3 space-y-2', compact ? 'min-h-[80px]' : 'min-h-[100px]')}>
              {orderedCards.length > 0 ? (
                orderedCards.slice(0, maxCardsPerColumn).map((card) => (
                  <article
                    key={card.id}
                    className={cn(
                      `sim-board-card sim-board-card-${card.kind} ${card.status === 'queued' ? 'sim-board-card-queued' : ''} ${card.isBlocked ? 'sim-board-card-blocked' : ''}`,
                      compact && 'px-2.5 py-2',
                    )}
                  >
                    <div className={cn('sim-board-card-pin', compact && 'top-1')} />
                    {card.isBlocked ? (
                      <div
                        className={cn('sim-board-card-blocker', compact && 'max-w-14 min-h-4 min-w-4')}
                        title={card.blockerLabel ? `Blocked by ${card.blockerLabel}` : 'Blocked'}
                        aria-label={card.blockerLabel ? `Blocked by ${card.blockerLabel}` : 'Blocked'}
                      >
                        {card.blockerLabel ? <span>{card.blockerLabel}</span> : null}
                      </div>
                    ) : null}
                    <p className={cn('sim-board-card-kind', compact && 'text-[0.5rem]')}>
                      {card.kind === 'addedScope' ? 'Scope' : card.kind === 'defect' ? 'Defect' : 'Story'}
                    </p>
                    <p className={cn('sim-board-card-title', compact && 'mt-1 text-[0.72rem]')}>
                      {card.label}
                    </p>
                    {card.deliverable ? (
                      <p className={cn('sim-board-card-meta', compact && 'text-[0.55rem]')}>
                        {card.deliverable}
                      </p>
                    ) : null}
                  </article>
                ))
              ) : (
                <div className="col-span-full rounded-2xl border border-dashed border-[var(--line)] bg-[var(--surface)] px-3 py-10 text-center text-xs font-medium text-[var(--sea-ink-soft)]">
                  Empty
                </div>
              )}
              {orderedCards.length > maxCardsPerColumn ? (
                <p className={cn('col-span-full text-center font-semibold text-[var(--sea-ink-soft)]', compact ? 'text-[10px]' : 'text-[11px]')}>
                  +{orderedCards.length - maxCardsPerColumn} more
                </p>
              ) : null}
            </div>
          </section>
        )})}
      </div>
    </div>
  )
}

function compareBoardCards(left: BoardSnapshot['columns'][number]['cards'][number], right: BoardSnapshot['columns'][number]['cards'][number]) {
  return boardCardPriority(left) - boardCardPriority(right) || left.label.localeCompare(right.label)
}

function boardCardPriority(card: BoardSnapshot['columns'][number]['cards'][number]) {
  if (card.isBlocked) return 0
  if (card.kind === 'defect') return 1
  if (card.kind === 'addedScope') return 2
  return 3
}

export function BoardLegendChip({
  label,
  kind,
}: {
  label: string
  kind: 'story' | 'defect' | 'addedScope'
}) {
  return <span className={`sim-board-legend sim-board-legend-${kind}`}>{label}</span>
}