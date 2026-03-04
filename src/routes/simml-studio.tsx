import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useMemo, useRef, useState, type CSSProperties, type UIEvent } from 'react'
import { loadSimulationExamples, parseSimMl } from '#/lib/kanban-scrum-sim'
import {
  getCursorContext,
  resolveAttributeHelp,
  resolveTagHelp,
  validateSimMlSource,
} from '#/lib/simml-editor'

export const Route = createFileRoute('/simml-studio')({
  component: SimmlStudioPage,
})

const EXAMPLES = loadSimulationExamples()
const DEFAULT_EXAMPLE =
  EXAMPLES.find((example) => example.path.includes('1 - Simplest Board')) ?? EXAMPLES[0]
const EDITOR_LINE_HEIGHT_REM = 1.5

function clampRange(value: number, lower: number, upper: number) {
  return Math.max(lower, Math.min(upper, value))
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;')
}

function buildHighlightedMarkup(source: string, diagnostics: ReturnType<typeof validateSimMlSource>) {
  if (!source) return '<br />'

  const decorations = diagnostics
    .map((diagnostic) => ({
      from: clampRange(Math.min(diagnostic.from, diagnostic.to), 0, source.length),
      to: clampRange(Math.max(diagnostic.from, diagnostic.to), 0, source.length),
      severity: diagnostic.severity,
    }))
    .filter((diagnostic) => diagnostic.to > diagnostic.from)
    .sort((left, right) => {
      if (left.from !== right.from) return left.from - right.from
      const leftWeight = left.severity === 'error' ? 0 : 1
      const rightWeight = right.severity === 'error' ? 0 : 1
      return leftWeight - rightWeight
    })

  let cursor = 0
  let markup = ''

  for (const decoration of decorations) {
    if (decoration.from < cursor) continue
    markup += escapeHtml(source.slice(cursor, decoration.from))
    const style =
      decoration.severity === 'error'
        ? 'background:rgba(239,68,68,0.22);color:#ffd9d9;text-decoration:underline wavy rgba(248,113,113,0.95);'
        : 'background:rgba(245,158,11,0.18);color:#ffe8b0;text-decoration:underline wavy rgba(251,191,36,0.9);'
    markup += `<mark style="${style}">${escapeHtml(source.slice(decoration.from, decoration.to))}</mark>`
    cursor = decoration.to
  }

  markup += escapeHtml(source.slice(cursor))
  return markup
}

function SimmlStudioPage() {
  const [selectedExampleId, setSelectedExampleId] = useState(DEFAULT_EXAMPLE?.id ?? '')
  const [source, setSource] = useState(DEFAULT_EXAMPLE?.source ?? '')
  const [cursor, setCursor] = useState(0)
  const [isClient, setIsClient] = useState(false)
  const [scrollTop, setScrollTop] = useState(0)
  const editorRef = useRef<HTMLTextAreaElement | null>(null)

  useEffect(() => {
    setIsClient(true)
  }, [])

  const diagnostics = useMemo(() => (isClient ? validateSimMlSource(source) : []), [isClient, source])
  const parserResult = useMemo(() => {
    if (!isClient) return { ok: false as const, message: 'Waiting for browser runtime…' }
    try {
      return { ok: true as const, value: parseSimMl(source) }
    } catch (error) {
      return {
        ok: false as const,
        message: error instanceof Error ? error.message : 'Unable to parse SimML model.',
      }
    }
  }, [isClient, source])

  const context = useMemo(() => (isClient ? getCursorContext(source, cursor) : { activeTag: null, activeAttribute: null, inOpenTag: false, suggestedAttributes: [] }), [isClient, source, cursor])
  const activeTag = resolveTagHelp(context.activeTag)
  const activeAttribute = resolveAttributeHelp(context.activeTag, context.activeAttribute)
  const lineCount = useMemo(() => Math.max(source.split('\n').length, 1), [source])
  const lineNumbers = useMemo(() => Array.from({ length: lineCount }, (_, index) => index + 1), [lineCount])
  const highlightedMarkup = useMemo(() => buildHighlightedMarkup(source, diagnostics), [diagnostics, source])
  const enumAttributes = useMemo(
    () => activeTag?.attributes.filter((attribute) => attribute.validValues?.length) ?? [],
    [activeTag],
  )
  const editorTypography: CSSProperties = {
    fontFamily: 'ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace',
    fontSize: '12px',
    lineHeight: `${EDITOR_LINE_HEIGHT_REM}rem`,
  }

  function handleEditorScroll(event: UIEvent<HTMLTextAreaElement>) {
    setScrollTop(event.currentTarget.scrollTop)
  }

  function insertAttribute(attributeName: string) {
    const editor = editorRef.current
    const selectionStart = editor?.selectionStart ?? cursor
    const selectionEnd = editor?.selectionEnd ?? selectionStart
    const insertion = ` ${attributeName}=""`
    const nextSource = `${source.slice(0, selectionStart)}${insertion}${source.slice(selectionEnd)}`
    const nextCursor = selectionStart + insertion.length - 1
    setSource(nextSource)
    setCursor(nextCursor)

    requestAnimationFrame(() => {
      const currentEditor = editorRef.current
      if (!currentEditor) return
      currentEditor.focus()
      currentEditor.setSelectionRange(nextCursor, nextCursor)
    })
  }

  return (
    <main className="mx-auto max-w-[1600px] px-4 pb-14 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rounded-[2rem] p-5 sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="island-kicker">First-class SimML authoring</p>
            <h1 className="display-title text-3xl font-semibold text-[var(--sea-ink)]">SimML Studio</h1>
            <p className="mt-2 max-w-3xl text-sm text-[var(--sea-ink-soft)]">
              Dedicated workspace for `.simml` files with continuous validation, schema-aware attribute help, and syntax diagnostics.
            </p>
          </div>
          <Link
            to="/kanban-scrum-sim"
            className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] no-underline"
          >
            Back to Simulator
          </Link>
        </div>

        <div className="mt-5 grid gap-4 lg:grid-cols-[1.2fr_1fr_auto]">
          <label className="field-wrap">
            <span className="field-legend">Example library</span>
            <select
              className="field-input"
              value={selectedExampleId}
              onChange={(event) => {
                const id = event.target.value
                const example = EXAMPLES.find((item) => item.id === id)
                setSelectedExampleId(id)
                if (example) setSource(example.source)
              }}
            >
              {EXAMPLES.map((example) => (
                <option key={example.id} value={example.id}>
                  {example.group} / {example.section} / {example.title}
                </option>
              ))}
            </select>
          </label>

          <label className="field-wrap">
            <span className="field-legend">Open local file</span>
            <input
              type="file"
              accept=".simml,.xml"
              className="field-input file:mr-3 file:rounded-full file:border-0 file:bg-[rgba(79,184,178,0.18)] file:px-3 file:py-1 file:text-xs file:font-semibold file:text-[var(--lagoon-deep)]"
              onChange={async (event) => {
                const file = event.target.files?.[0]
                if (!file) return
                setSelectedExampleId('')
                setSource(await file.text())
              }}
            />
          </label>

          <button
            type="button"
            className="self-end rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)]"
            onClick={() => {
              const blob = new Blob([source], { type: 'application/xml' })
              const url = URL.createObjectURL(blob)
              const anchor = document.createElement('a')
              anchor.href = url
              anchor.download = 'model.simml'
              anchor.click()
              URL.revokeObjectURL(url)
            }}
          >
            Download .simml
          </button>
        </div>
      </section>

      <section className="mt-5 grid gap-5 xl:grid-cols-[1.35fr_0.85fr]">
        <article className="rounded-[1.8rem] border border-[var(--line)] bg-[#0b1b2f] p-4 shadow-sm">
          <div className="mb-3 flex items-center justify-between px-1">
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[#8ecbe0]">Editor</p>
            <p className="text-xs text-[#9bb7cc]">{lineCount} lines</p>
          </div>
          <div className="overflow-hidden rounded-[1.2rem] border border-[rgba(141,229,219,0.2)] bg-[#0f223a]">
            <div className="grid min-h-[640px] grid-cols-[auto_1fr]">
              <div className="border-r border-[rgba(141,229,219,0.12)] bg-[#0b1c31] px-3 py-4 text-right text-[#5f7d93]" style={editorTypography}>
                <div style={{ transform: `translateY(-${scrollTop}px)` }}>
                  {lineNumbers.map((lineNumber) => (
                    <div key={lineNumber} className="select-none">
                      {lineNumber}
                    </div>
                  ))}
                </div>
              </div>
              <div className="relative">
                <pre
                  aria-hidden="true"
                  className="pointer-events-none absolute inset-0 overflow-hidden whitespace-pre-wrap break-words px-4 py-4 text-[#8bb3cf]"
                  style={{ ...editorTypography, transform: `translateY(-${scrollTop}px)` }}
                  dangerouslySetInnerHTML={{ __html: highlightedMarkup }}
                />
                <textarea
                  ref={editorRef}
                  className="relative min-h-[640px] w-full resize-none bg-transparent px-4 py-4 text-transparent caret-[#e6f3ff] outline-none [text-shadow:0_0_0_#e6f3ff] selection:bg-[rgba(79,184,178,0.35)] focus:border-[var(--lagoon)]"
                  style={editorTypography}
                  spellCheck={false}
                  value={source}
                  onScroll={handleEditorScroll}
                  onClick={(event) => setCursor(event.currentTarget.selectionStart)}
                  onKeyUp={(event) => setCursor(event.currentTarget.selectionStart)}
                  onSelect={(event) => setCursor(event.currentTarget.selectionStart)}
                  onChange={(event) => {
                    setSource(event.target.value)
                    setCursor(event.target.selectionStart)
                  }}
                />
              </div>
            </div>
          </div>
        </article>

        <article className="space-y-4">
          <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4">
            <p className="island-kicker">Validation</p>
            <div className="mt-2 flex items-center gap-2 text-sm">
              <span className={`inline-block h-2.5 w-2.5 rounded-full ${diagnostics.some((item) => item.severity === 'error') ? 'bg-red-500' : 'bg-emerald-500'}`} />
              {diagnostics.length
                ? `${diagnostics.filter((item) => item.severity === 'error').length} errors, ${diagnostics.filter((item) => item.severity === 'warning').length} warnings`
                : 'No schema or syntax diagnostics'}
            </div>
            {!parserResult.ok ? (
              <p className="mt-2 text-xs text-red-700">Runtime parse check: {parserResult.message}</p>
            ) : (
              <p className="mt-2 text-xs text-emerald-700">Runtime parse check passed for simulation: {parserResult.value.name || 'Unnamed model'}.</p>
            )}
          </div>

          <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4">
            <p className="island-kicker">Context-aware help</p>
            {activeTag ? (
              <>
                <p className="mt-2 text-sm">
                  Tag: <code className="rounded bg-[var(--surface-strong)] px-1.5 py-0.5">&lt;{activeTag.tag}&gt;</code>
                </p>
                <p className="mt-2 text-xs leading-5 text-[var(--sea-ink-soft)]">{activeTag.description}</p>
                <p className="mt-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--kicker)]">Allowed attributes</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {context.suggestedAttributes.map((attribute) => (
                    <button
                      key={attribute.name}
                      type="button"
                      className="rounded-full border border-[var(--line)] bg-[var(--surface-strong)] px-2.5 py-1 text-[11px]"
                      onClick={() => insertAttribute(attribute.name)}
                    >
                      {attribute.name}
                      {attribute.mandatory ? ' *' : ''}
                    </button>
                  ))}
                </div>
                {enumAttributes.length ? (
                  <div className="mt-4 rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.12em] text-[var(--lagoon-deep)]">Enum-backed values</p>
                    <div className="mt-2 space-y-2">
                      {enumAttributes.map((attribute) => (
                        <div key={attribute.name}>
                          <p className="text-xs font-semibold text-[var(--sea-ink)]">{attribute.name}</p>
                          <div className="mt-1 flex flex-wrap gap-1">
                            {attribute.validValues?.map((value) => (
                              <code
                                key={`${attribute.name}-${value}`}
                                className="rounded bg-[var(--surface)] px-1.5 py-0.5 text-[11px] text-[var(--sea-ink)]"
                              >
                                {value}
                              </code>
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : null}
              </>
            ) : (
              <p className="mt-2 text-xs text-[var(--sea-ink-soft)]">Place cursor inside an opening tag to see schema-aware attribute guidance.</p>
            )}

            {activeAttribute ? (
              <div className="mt-4 rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-3">
                <p className="text-xs font-semibold text-[var(--lagoon-deep)]">Attribute: {activeAttribute.name}</p>
                <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">{activeAttribute.description}</p>
                {activeAttribute.validValues?.length ? (
                  <p className="mt-2 text-xs">Values: {activeAttribute.validValues.join(', ')}</p>
                ) : null}
              </div>
            ) : null}
          </div>

          <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4">
            <p className="island-kicker">Diagnostics</p>
            <ul className="mt-2 max-h-[320px] space-y-2 overflow-auto pr-1 text-xs">
              {diagnostics.length === 0 ? (
                <li className="rounded-lg border border-emerald-200 bg-emerald-50 p-2 text-emerald-800">All checks are passing.</li>
              ) : (
                diagnostics.map((diagnostic, index) => (
                  <li
                    key={`${diagnostic.message}-${index}`}
                    className={`rounded-lg border p-2 ${diagnostic.severity === 'error' ? 'border-red-200 bg-red-50 text-red-900' : 'border-amber-200 bg-amber-50 text-amber-900'}`}
                  >
                    <p className="font-semibold">{diagnostic.severity.toUpperCase()} · line {diagnostic.line}, col {diagnostic.column}</p>
                    <p className="mt-0.5">{diagnostic.message}</p>
                  </li>
                ))
              )}
            </ul>
          </div>

          <div className="rounded-[1.5rem] border border-dashed border-[var(--line)] bg-[var(--surface)] p-4">
            <p className="island-kicker">Roadmap</p>
            <p className="mt-2 text-xs text-[var(--sea-ink-soft)]">
              Reserved for AI-assisted authoring, completion, and schema-aware refactors. This page now centralizes model authoring so those capabilities can be layered in next.
            </p>
          </div>
        </article>
      </section>
    </main>
  )
}
