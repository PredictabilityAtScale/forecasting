import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useMemo, useState } from 'react'
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

function SimmlStudioPage() {
  const [selectedExampleId, setSelectedExampleId] = useState(DEFAULT_EXAMPLE?.id ?? '')
  const [source, setSource] = useState(DEFAULT_EXAMPLE?.source ?? '')
  const [cursor, setCursor] = useState(0)
  const [isClient, setIsClient] = useState(false)

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
            <p className="text-xs text-[#9bb7cc]">{source.split('\n').length} lines</p>
          </div>
          <textarea
            className="min-h-[640px] w-full rounded-[1.2rem] border border-[rgba(141,229,219,0.2)] bg-[#0f223a] p-4 font-mono text-[12px] leading-6 text-[#e6f3ff] outline-none focus:border-[var(--lagoon)]"
            spellCheck={false}
            value={source}
            onClick={(event) => setCursor(event.currentTarget.selectionStart)}
            onKeyUp={(event) => setCursor(event.currentTarget.selectionStart)}
            onChange={(event) => {
              setSource(event.target.value)
              setCursor(event.target.selectionStart)
            }}
          />
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
                      onClick={() => {
                        setSource((current) => `${current.slice(0, cursor)} ${attribute.name}=""${current.slice(cursor)}`)
                      }}
                    >
                      {attribute.name}
                      {attribute.mandatory ? ' *' : ''}
                    </button>
                  ))}
                </div>
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
