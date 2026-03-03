import { createFileRoute } from '@tanstack/react-router'
import { useState, useMemo, useCallback } from 'react'
import {
  SIMML_SCHEMA,
  flattenSchema,
  schemaStats,
  type SimMLSchemaElement,
  type SimMLSchemaAttribute,
} from '#/data/simml-schema'

export const Route = createFileRoute('/simml-reference')({
  component: SimMLReferencePage,
})

/* ─── Helpers ────────────────────────────────────────────────────────────── */

function typeBadge(type?: string) {
  if (!type) return null
  const colors: Record<string, string> = {
    string: 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
    integer: 'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300',
    number: 'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300',
    boolean: 'bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-300',
    enum: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300',
    date: 'bg-rose-100 text-rose-800 dark:bg-rose-900/40 dark:text-rose-300',
    currency: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
  }
  const cls = colors[type] ?? 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'
  return (
    <span className={`inline-block rounded px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wider ${cls}`}>
      {type}
    </span>
  )
}

function simTypeBadge(simType?: 'kanban' | 'scrum') {
  if (!simType) return null
  const cls =
    simType === 'kanban'
      ? 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300'
      : 'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300'
  return (
    <span className={`ml-1.5 inline-block rounded px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wider ${cls}`}>
      {simType} only
    </span>
  )
}

function mandatoryDot(mandatory: boolean) {
  return mandatory ? (
    <span className="ml-1 inline-block h-1.5 w-1.5 rounded-full bg-red-500" title="Required" />
  ) : null
}

/* ─── Attribute table ────────────────────────────────────────────────────── */

function AttributeTable({ attributes }: { attributes: SimMLSchemaAttribute[] }) {
  if (attributes.length === 0)
    return (
      <p className="mt-2 text-xs italic text-[var(--sea-ink-soft)]">
        No attributes — presence of the element alone is meaningful.
      </p>
    )

  return (
    <div className="mt-3 overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-[var(--line)] text-[10px] font-bold uppercase tracking-wider text-[var(--sea-ink-soft)]">
            <th className="pb-2 pr-3">Attribute</th>
            <th className="pb-2 pr-3">Type</th>
            <th className="pb-2 pr-3">Description</th>
            <th className="pb-2 pr-3">Default</th>
            <th className="pb-2">Values</th>
          </tr>
        </thead>
        <tbody>
          {attributes.map((attr) => (
            <tr key={attr.name} className="border-b border-[var(--line)]/50">
              <td className="whitespace-nowrap py-2 pr-3 font-mono text-xs font-semibold text-[var(--lagoon-deep)]">
                {attr.name}
                {mandatoryDot(attr.mandatory)}
                {simTypeBadge(attr.simType)}
              </td>
              <td className="py-2 pr-3">{typeBadge(attr.type)}</td>
              <td className="py-2 pr-3 text-xs leading-relaxed text-[var(--sea-ink)]">
                {attr.description}
              </td>
              <td className="whitespace-nowrap py-2 pr-3 font-mono text-xs text-[var(--sea-ink-soft)]">
                {attr.defaultValue ?? '—'}
              </td>
              <td className="py-2">
                {attr.validValues ? (
                  <div className="flex flex-wrap gap-1">
                    {attr.validValues.map((v) => (
                      <code
                        key={v}
                        className="rounded bg-[var(--surface)] px-1.5 py-0.5 text-[11px] text-[var(--sea-ink)]"
                      >
                        {v}
                      </code>
                    ))}
                  </div>
                ) : (
                  <span className="text-xs text-[var(--sea-ink-soft)]">—</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/* ─── Tree node sidebar ──────────────────────────────────────────────────── */

function TreeNode({
  element,
  depth,
  selectedTag,
  onSelect,
  expandedSet,
  onToggle,
}: {
  element: SimMLSchemaElement
  depth: number
  selectedTag: string
  onSelect: (tag: string) => void
  expandedSet: Set<string>
  onToggle: (tag: string) => void
}) {
  const hasChildren = element.children.length > 0
  const isExpanded = expandedSet.has(element.tag)
  const isSelected = selectedTag === element.tag

  return (
    <div>
      <button
        onClick={() => {
          onSelect(element.tag)
          if (hasChildren && !isExpanded) onToggle(element.tag)
        }}
        className={`flex w-full items-center gap-1 rounded-lg px-2 py-1.5 text-left text-sm transition ${
          isSelected
            ? 'bg-[rgba(79,184,178,0.18)] font-semibold text-[var(--lagoon-deep)]'
            : 'text-[var(--sea-ink-soft)] hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]'
        }`}
        style={{ paddingLeft: `${depth * 16 + 8}px` }}
      >
        {hasChildren ? (
          <span
            className="mr-0.5 inline-flex h-4 w-4 flex-shrink-0 items-center justify-center text-[10px]"
            onClick={(e) => {
              e.stopPropagation()
              onToggle(element.tag)
            }}
          >
            {isExpanded ? '▾' : '▸'}
          </span>
        ) : (
          <span className="mr-0.5 inline-block h-4 w-4 flex-shrink-0" />
        )}
        <code className="text-xs">&lt;{element.tag}&gt;</code>
        {element.mandatory && (
          <span className="ml-auto inline-block h-1.5 w-1.5 flex-shrink-0 rounded-full bg-red-400" title="Required" />
        )}
      </button>

      {hasChildren && isExpanded && (
        <div>
          {element.children.map((child) => (
            <TreeNode
              key={child.tag}
              element={child}
              depth={depth + 1}
              selectedTag={selectedTag}
              onSelect={onSelect}
              expandedSet={expandedSet}
              onToggle={onToggle}
            />
          ))}
        </div>
      )}
    </div>
  )
}

/* ─── Element detail panel ───────────────────────────────────────────────── */

function ElementDetail({ element, path }: { element: SimMLSchemaElement; path: string[] }) {
  return (
    <div>
      {/* Breadcrumb */}
      <div className="mb-3 flex flex-wrap items-center gap-1 text-xs text-[var(--sea-ink-soft)]">
        {path.map((p, i) => (
          <span key={i}>
            {i > 0 && <span className="mx-1 text-[var(--line)]">/</span>}
            <code className={i === path.length - 1 ? 'font-semibold text-[var(--lagoon-deep)]' : ''}>
              &lt;{p}&gt;
            </code>
          </span>
        ))}
      </div>

      {/* Title */}
      <h2 className="text-2xl font-bold text-[var(--sea-ink)]">
        <code>&lt;{element.tag}&gt;</code>
      </h2>
      <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
        {element.displayName}
        {element.mandatory && (
          <span className="ml-2 rounded bg-red-100 px-1.5 py-0.5 text-[10px] font-bold uppercase text-red-700 dark:bg-red-900/30 dark:text-red-400">
            required
          </span>
        )}
      </p>

      {/* Description */}
      <p className="mt-4 text-sm leading-relaxed text-[var(--sea-ink)]">{element.description}</p>

      {/* Notes */}
      {element.notes && (
        <div className="mt-4 rounded-xl border border-[var(--line)] bg-[rgba(79,184,178,0.06)] p-4 text-xs leading-relaxed text-[var(--sea-ink)]">
          <span className="font-bold text-[var(--lagoon-deep)]">Note:</span> {element.notes}
        </div>
      )}

      {/* Attributes */}
      <div className="mt-6">
        <h3 className="text-sm font-bold text-[var(--sea-ink)]">
          Attributes
          <span className="ml-2 text-xs font-normal text-[var(--sea-ink-soft)]">
            ({element.attributes.length})
            {element.attributes.some((a) => a.mandatory) && (
              <span className="ml-1">
                <span className="inline-block h-1.5 w-1.5 rounded-full bg-red-500" /> = required
              </span>
            )}
          </span>
        </h3>
        <AttributeTable attributes={element.attributes} />
      </div>

      {/* Children */}
      {element.children.length > 0 && (
        <div className="mt-6">
          <h3 className="text-sm font-bold text-[var(--sea-ink)]">
            Child Elements ({element.children.length})
          </h3>
          <div className="mt-2 grid gap-2 sm:grid-cols-2">
            {element.children.map((child) => (
              <div
                key={child.tag}
                className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3"
              >
                <code className="text-sm font-semibold text-[var(--lagoon-deep)]">
                  &lt;{child.tag}&gt;
                </code>
                {child.mandatory && mandatoryDot(true)}
                <p className="mt-1 text-xs leading-relaxed text-[var(--sea-ink-soft)]">
                  {child.description.slice(0, 120)}
                  {child.description.length > 120 ? '...' : ''}
                </p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* C# source */}
      <div className="mt-6 border-t border-[var(--line)] pt-4">
        <p className="text-[10px] font-bold uppercase tracking-wider text-[var(--sea-ink-soft)]">
          Implementation
        </p>
        <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
          C# class: <code className="font-semibold text-[var(--sea-ink)]">{element.csharpClass}</code>
        </p>
        <p className="text-xs text-[var(--sea-ink-soft)]">
          File: <code className="text-[var(--sea-ink)]">{element.csharpFile}</code>
        </p>
      </div>
    </div>
  )
}

/* ─── Search results ─────────────────────────────────────────────────────── */

function SearchResults({
  results,
  onSelect,
}: {
  results: Array<{ element: SimMLSchemaElement; path: string[]; matchType: string }>
  onSelect: (tag: string) => void
}) {
  if (results.length === 0)
    return <p className="p-4 text-sm text-[var(--sea-ink-soft)]">No results found.</p>

  return (
    <div className="space-y-2">
      {results.map(({ element, path, matchType }) => (
        <button
          key={element.tag + path.join('/')}
          className="w-full rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3 text-left transition hover:border-[var(--lagoon)]"
          onClick={() => onSelect(element.tag)}
        >
          <div className="flex items-baseline gap-2">
            <code className="text-sm font-semibold text-[var(--lagoon-deep)]">
              &lt;{element.tag}&gt;
            </code>
            <span className="rounded bg-[var(--link-bg-hover)] px-1.5 py-0.5 text-[10px] text-[var(--sea-ink-soft)]">
              {matchType}
            </span>
          </div>
          <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
            {path.map((p) => `<${p}>`).join(' → ')}
          </p>
        </button>
      ))}
    </div>
  )
}

/* ─── Example XML snippet generator ──────────────────────────────────────── */

function generateExample(element: SimMLSchemaElement, indent = 0): string {
  const pad = '  '.repeat(indent)
  const tag = element.tag.startsWith('?') ? element.tag : element.tag

  // Processing instruction
  if (element.tag.startsWith('?')) {
    const attrs = element.attributes
      .filter((a) => a.mandatory)
      .map((a) => `${a.name}="${a.defaultValue ?? '...'}"`)
      .join(' ')
    return `${pad}<?${element.tag.slice(1)} ${attrs}?>`
  }

  const mandatoryAttrs = element.attributes.filter((a) => a.mandatory)
  const optionalAttrs = element.attributes.filter((a) => !a.mandatory).slice(0, 3)

  let attrStr = mandatoryAttrs.map((a) => `${a.name}="${a.defaultValue ?? '...'}"`)
  attrStr = attrStr.concat(optionalAttrs.map((a) => `${a.name}="${a.defaultValue ?? '...'}">`))

  // Format the opening tag
  const attrString = attrStr.length > 0 ? ' ' + attrStr.join(' ') : ''

  if (element.children.length === 0 && !element.notes?.includes('text content')) {
    return `${pad}<${tag}${attrString} />`
  }

  const lines = [`${pad}<${tag}${attrString.replace(/\>$/, '')}>`]
  for (const child of element.children.slice(0, 3)) {
    lines.push(generateExample(child, indent + 1))
  }
  if (element.children.length > 3) {
    lines.push(`${'  '.repeat(indent + 1)}<!-- ... -->`)
  }
  lines.push(`${pad}</${tag}>`)
  return lines.join('\n')
}

/* ─── Main page ──────────────────────────────────────────────────────────── */

function SimMLReferencePage() {
  const [selectedTag, setSelectedTag] = useState('simulation')
  const [searchQuery, setSearchQuery] = useState('')
  const [expandedSet, setExpandedSet] = useState<Set<string>>(
    () => new Set(['simulation', 'execute', 'setup']),
  )
  const [showExample, setShowExample] = useState(false)

  const flat = useMemo(() => flattenSchema(SIMML_SCHEMA), [])
  const stats = useMemo(() => schemaStats(SIMML_SCHEMA), [])

  const selectedEntry = useMemo(
    () => flat.find((e) => e.element.tag === selectedTag) ?? flat[0],
    [flat, selectedTag],
  )

  const searchResults = useMemo(() => {
    if (!searchQuery.trim()) return []
    const q = searchQuery.toLowerCase()
    return flat
      .flatMap(({ element, path }) => {
        const matches: Array<{
          element: SimMLSchemaElement
          path: string[]
          matchType: string
        }> = []
        if (element.tag.toLowerCase().includes(q))
          matches.push({ element, path, matchType: 'element' })
        else if (element.description.toLowerCase().includes(q))
          matches.push({ element, path, matchType: 'description' })
        else if (element.attributes.some((a) => a.name.toLowerCase().includes(q)))
          matches.push({ element, path, matchType: 'attribute' })
        else if (
          element.attributes.some((a) => a.description.toLowerCase().includes(q))
        )
          matches.push({ element, path, matchType: 'attr description' })
        return matches
      })
      .slice(0, 20)
  }, [flat, searchQuery])

  const onToggle = useCallback((tag: string) => {
    setExpandedSet((prev) => {
      const next = new Set(prev)
      if (next.has(tag)) next.delete(tag)
      else next.add(tag)
      return next
    })
  }, [])

  const onSelect = useCallback(
    (tag: string) => {
      setSelectedTag(tag)
      setSearchQuery('')
      // expand parent path
      const entry = flat.find((e) => e.element.tag === tag)
      if (entry) {
        setExpandedSet((prev) => {
          const next = new Set(prev)
          entry.path.forEach((p) => next.add(p))
          return next
        })
      }
    },
    [flat],
  )

  const example = useMemo(
    () => generateExample(selectedEntry.element),
    [selectedEntry],
  )

  return (
    <main className="mx-auto max-w-[1500px] px-4 pb-14 pt-8 sm:px-6 lg:px-8">
      <div className="space-y-5">
        {/* Hero */}
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-5 sm:p-6">
            <p className="island-kicker mb-2">Format Documentation</p>
            <h1 className="display-title text-3xl font-semibold text-[var(--sea-ink)]">
              SimML Reference
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-[var(--sea-ink-soft)]">
              Complete XML schema reference for the{' '}
              <code className="rounded bg-[var(--link-bg-hover)] px-1 py-0.5 text-xs">.simml</code>{' '}
              simulation model format. Reverse-engineered from the C# desktop engine's{' '}
              <code className="rounded bg-[var(--link-bg-hover)] px-1 py-0.5 text-xs">
                [SimMLElement]
              </code>{' '}
              and{' '}
              <code className="rounded bg-[var(--link-bg-hover)] px-1 py-0.5 text-xs">
                [SimMLAttribute]
              </code>{' '}
              contract decorators.
            </p>
            <div className="mt-4 flex flex-wrap gap-4 text-xs text-[var(--sea-ink-soft)]">
              <span>
                <strong className="text-[var(--sea-ink)]">{stats.elements}</strong> elements
              </span>
              <span>
                <strong className="text-[var(--sea-ink)]">{stats.attributes}</strong> attributes
              </span>
              <span>
                <strong className="text-[var(--sea-ink)]">2</strong> sim types (Kanban &amp; Scrum)
              </span>
            </div>
          </div>
        </section>

        {/* Document tree */}
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <p className="island-kicker mb-3">Document Structure</p>
          <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4 sm:p-5">
            <pre className="overflow-x-auto text-xs leading-6 text-[var(--sea-ink)]">
{`<simulation name="..." locale="...">
  <?parameter name="@var" value="..." ?>
  <execute type="kanban|scrum" limitIntervalsTo="..." ...>
    <visual />
    <monteCarlo cycles="..." />
    <sensitivity cycles="..." estimateMultiplier="..." />
    <addStaff cycles="..." />
    <ballot type="borda|schulze" />
  </execute>
  <setup>
    <backlog type="simple|custom" simpleCount="..." shuffle="true|false">
      <deliverable name="..." order="..." preRequisiteDeliverables="...">
        <custom name="..." count="..." estimateLowBound="..." estimateHighBound="...">
          <column id="..." estimateLowBound="..." estimateHighBound="..." />
        </custom>
      </deliverable>
      <custom name="..." count="..." ... />
    </backlog>
    <columns>
      <column id="..." wipLimit="..." estimateLowBound="..." estimateHighBound="..." buffer="false" />
    </columns>
    <iteration storyPointsPerIterationLowBound="..." storyPointsPerIterationHighBound="..." />
    <defects>
      <defect columnId="..." occurrenceLowBound="..." occurrenceHighBound="..." count="...">
        <column columnId="..." estimateLowBound="..." estimateHighBound="..." />
      </defect>
    </defects>
    <blockingEvents>
      <blockingEvent columnId="..." occurrenceLowBound="..." occurrenceHighBound="..."
                     estimateLowBound="..." estimateHighBound="..." />
    </blockingEvents>
    <addedScopes>
      <addedScope occurrenceLowBound="..." occurrenceHighBound="..." count="..." />
    </addedScopes>
    <forecastDate startDate="..." costPerDay="..." targetDate="...">
      <excludes><exclude date="..." /></excludes>
      <actuals><actual date="..." count="..." annotation="..." /></actuals>
    </forecastDate>
    <distributions>
      <distribution name="..." shape="..." parameters="..." />
    </distributions>
    <phases unit="percentage|interval|iteration">
      <phase start="..." end="..." estimateMultiplier="..." costPerDay="...">
        <column id="..." wipLimit="..." />
      </phase>
    </phases>
    <classOfServices>
      <classOfService order="..." default="false" violateWIP="false" skipPercentage="0">
        <column id="..." estimateLowBound="..." estimateHighBound="..." />
      </classOfService>
    </classOfServices>
  </setup>
</simulation>`}
            </pre>
          </div>
        </section>

        {/* Explorer */}
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <p className="island-kicker mb-3">Element Explorer</p>
          <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
            {/* Sidebar */}
            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-3">
              {/* Search */}
              <div className="mb-3">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search elements, attributes..."
                  className="w-full rounded-lg border border-[var(--line)] bg-transparent px-3 py-2 text-sm text-[var(--sea-ink)] placeholder:text-[var(--sea-ink-soft)] focus:border-[var(--lagoon)] focus:outline-none"
                />
              </div>

              {searchQuery.trim() ? (
                <SearchResults results={searchResults} onSelect={onSelect} />
              ) : (
                <div className="max-h-[600px] overflow-y-auto">
                  <TreeNode
                    element={SIMML_SCHEMA}
                    depth={0}
                    selectedTag={selectedTag}
                    onSelect={onSelect}
                    expandedSet={expandedSet}
                    onToggle={onToggle}
                  />
                </div>
              )}
            </div>

            {/* Detail panel */}
            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-5 sm:p-6">
              <ElementDetail
                element={selectedEntry.element}
                path={selectedEntry.path}
              />

              {/* Toggle example snippet */}
              <div className="mt-6 border-t border-[var(--line)] pt-4">
                <button
                  onClick={() => setShowExample((v) => !v)}
                  className="text-xs font-semibold text-[var(--lagoon-deep)] transition hover:underline"
                >
                  {showExample ? 'Hide' : 'Show'} XML example snippet
                </button>
                {showExample && (
                  <pre className="mt-3 overflow-x-auto rounded-xl border border-[var(--line)] bg-[rgba(0,0,0,0.03)] p-4 text-xs leading-5 text-[var(--sea-ink)] dark:bg-[rgba(255,255,255,0.03)]">
                    {example}
                  </pre>
                )}
              </div>
            </div>
          </div>
        </section>

        {/* Quick ref: all attributes flat table */}
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <p className="island-kicker mb-3">Attribute Quick Reference</p>
          <p className="mb-4 text-xs text-[var(--sea-ink-soft)]">
            Every attribute across all elements, searchable. Click an element name to jump to the explorer.
          </p>
          <QuickRefTable flat={flat} onSelect={onSelect} />
        </section>

        {/* Enums reference */}
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <p className="island-kicker mb-3">Enumeration Reference</p>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <EnumCard
              name="SimulationType"
              values={['kanban', 'scrum']}
              description="Determines whether the simulation uses Kanban flow or Scrum iteration mechanics."
            />
            <EnumCard
              name="BacklogType"
              values={['simple', 'custom']}
              description="Simple creates N identical items. Custom uses deliverables and typed entries."
            />
            <EnumCard
              name="OccurrenceType"
              values={['count', 'cards', 'stories', 'size', 'points', 'percentage']}
              description="Unit of measure for event occurrence rates. 'count'/'cards'/'stories' are equivalent count-based triggers. 'size'/'points' use story points (Scrum). 'percentage' is a per-interval probability."
            />
            <EnumCard
              name="PullOrder"
              values={['afterOrdering', 'random', 'index', 'fifo', 'fifoStrict']}
              description="Controls how items are selected for processing. 'afterOrdering' randomizes items that share the same priority. 'fifoStrict' prevents out-of-order completion."
            />
            <EnumCard
              name="AggregationValue"
              values={['average', 'median', 'fifth', 'twentyfifth', 'seventyfifth', 'ninetyfifth']}
              description="Statistical method for summarizing Monte Carlo results into a single forecast."
            />
            <EnumCard
              name="PhaseUnit"
              values={['percentage', 'interval', 'iteration']}
              description="Unit system for phase start/end trigger values."
            />
            <EnumCard
              name="ShufflePositions"
              values={['afterOrdering', 'true', 'false', 'FIFO', 'FIFOStrict']}
              description="Controls backlog randomization. 'afterOrdering' shuffles within same-priority groups."
            />
            <EnumCard
              name="BoundProcessing"
              values={['clip', 'stretch']}
              description="How distribution values outside bounds are handled. 'clip' truncates, 'stretch' rescales."
            />
            <EnumCard
              name="ZeroHandling"
              values={['keep', 'remove', 'value']}
              description="How zero values in sample data are processed. 'value' replaces them with zeroValue."
            />
            <EnumCard
              name="RevenueUnit"
              values={['day', 'week', 'month', 'year']}
              description="Time period for the revenue attribute in cost-of-delay calculations."
            />
          </div>
        </section>
      </div>
    </main>
  )
}

/* ─── Enum card ──────────────────────────────────────────────────────────── */

function EnumCard({
  name,
  values,
  description,
}: {
  name: string
  values: string[]
  description: string
}) {
  return (
    <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-4">
      <h4 className="text-sm font-bold text-[var(--sea-ink)]">{name}</h4>
      <p className="mt-1 text-xs leading-relaxed text-[var(--sea-ink-soft)]">{description}</p>
      <div className="mt-2 flex flex-wrap gap-1">
        {values.map((v) => (
          <code
            key={v}
            className="rounded bg-[var(--link-bg-hover)] px-1.5 py-0.5 text-[11px] text-[var(--sea-ink)]"
          >
            {v}
          </code>
        ))}
      </div>
    </div>
  )
}

/* ─── Quick-ref flat table ───────────────────────────────────────────────── */

function QuickRefTable({
  flat,
  onSelect,
}: {
  flat: Array<{ element: SimMLSchemaElement; path: string[] }>
  onSelect: (tag: string) => void
}) {
  const [filter, setFilter] = useState('')

  const rows = useMemo(() => {
    const q = filter.toLowerCase()
    return flat.flatMap(({ element }) =>
      element.attributes
        .filter(
          (a) =>
            !q ||
            a.name.toLowerCase().includes(q) ||
            a.description.toLowerCase().includes(q) ||
            element.tag.toLowerCase().includes(q),
        )
        .map((attr) => ({ element, attr })),
    )
  }, [flat, filter])

  return (
    <div>
      <input
        type="text"
        value={filter}
        onChange={(e) => setFilter(e.target.value)}
        placeholder="Filter attributes..."
        className="mb-3 w-full max-w-sm rounded-lg border border-[var(--line)] bg-transparent px-3 py-2 text-sm text-[var(--sea-ink)] placeholder:text-[var(--sea-ink-soft)] focus:border-[var(--lagoon)] focus:outline-none"
      />
      <div className="max-h-[500px] overflow-y-auto rounded-xl border border-[var(--line)]">
        <table className="w-full text-left text-xs">
          <thead className="sticky top-0 bg-[var(--surface)]">
            <tr className="border-b border-[var(--line)] text-[10px] font-bold uppercase tracking-wider text-[var(--sea-ink-soft)]">
              <th className="px-3 py-2">Element</th>
              <th className="px-3 py-2">Attribute</th>
              <th className="px-3 py-2">Type</th>
              <th className="px-3 py-2">Req</th>
              <th className="px-3 py-2">Description</th>
              <th className="px-3 py-2">Default</th>
            </tr>
          </thead>
          <tbody>
            {rows.slice(0, 200).map(({ element, attr }, i) => (
              <tr
                key={`${element.tag}-${attr.name}-${i}`}
                className="border-b border-[var(--line)]/30 hover:bg-[var(--link-bg-hover)]/50"
              >
                <td className="px-3 py-1.5">
                  <button
                    onClick={() => onSelect(element.tag)}
                    className="font-mono text-[var(--lagoon-deep)] hover:underline"
                  >
                    &lt;{element.tag}&gt;
                  </button>
                </td>
                <td className="px-3 py-1.5 font-mono font-semibold text-[var(--sea-ink)]">
                  {attr.name}
                  {attr.mandatory && mandatoryDot(true)}
                </td>
                <td className="px-3 py-1.5">{typeBadge(attr.type)}</td>
                <td className="px-3 py-1.5">{attr.mandatory ? 'Yes' : ''}</td>
                <td className="max-w-xs px-3 py-1.5 text-[var(--sea-ink-soft)]">
                  {attr.description.slice(0, 100)}
                  {attr.description.length > 100 ? '...' : ''}
                </td>
                <td className="whitespace-nowrap px-3 py-1.5 font-mono text-[var(--sea-ink-soft)]">
                  {attr.defaultValue ?? '—'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {rows.length > 200 && (
          <p className="p-3 text-center text-xs text-[var(--sea-ink-soft)]">
            Showing 200 of {rows.length} results. Refine your filter.
          </p>
        )}
      </div>
    </div>
  )
}
