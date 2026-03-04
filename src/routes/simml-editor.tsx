import { createFileRoute } from '@tanstack/react-router'
import { useMemo, useState } from 'react'
import { flattenSchema, SIMML_SCHEMA, type SimMLSchemaElement } from '#/data/simml-schema'

export const Route = createFileRoute('/simml-editor')({
  component: SimMLEditorPage,
})

function generateExample(element: SimMLSchemaElement, indent = 0): string {
  const pad = '  '.repeat(indent)
  const attrs = element.attributes
    .map((a) => {
      if (a.defaultValue != null) return `${a.name}="${a.defaultValue}"`
      if (a.validValues?.[0]) return `${a.name}="${a.validValues[0]}"`
      return `${a.name}="..."`
    })
    .join(' ')

  const open = attrs ? `<${element.tag} ${attrs}>` : `<${element.tag}>`

  if (!element.children.length) {
    return `${pad}${open}${element.sampleValue ?? ''}</${element.tag}>`
  }

  const children = element.children.map((child) => generateExample(child, indent + 1)).join('\n')
  return `${pad}${open}\n${children}\n${pad}</${element.tag}>`
}

function SimMLEditorPage() {
  const [source, setSource] = useState('<simulation name="New Model" locale="en-US">\n  <execute />\n  <setup />\n</simulation>')
  const [query, setQuery] = useState('')
  const [selectedTag, setSelectedTag] = useState('simulation')

  const flat = useMemo(() => flattenSchema(SIMML_SCHEMA), [])
  const selectedEntry = flat.find((entry) => entry.element.tag === selectedTag) ?? flat[0]
  const snippet = useMemo(() => generateExample(selectedEntry.element), [selectedEntry])

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return flat
    return flat.filter(({ element, path }) => {
      const text = `${element.tag} ${element.displayName} ${element.description} ${path.join(' ')}`.toLowerCase()
      return text.includes(q)
    })
  }, [flat, query])

  return (
    <main className="mx-auto max-w-[1500px] px-4 pb-14 pt-8 sm:px-6 lg:px-8">
      <div className="space-y-5">
        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <p className="island-kicker">SimML Editor</p>
          <h1 className="display-title text-3xl font-semibold text-[var(--sea-ink)]">Snippet Insertion Workspace</h1>
          <p className="mt-3 text-sm text-[var(--sea-ink-soft)]">
            Browse SimML elements and insert generated snippets at the cursor position in the editor.
          </p>
        </section>

        <section className="island-shell rounded-[2rem] p-5 sm:p-6">
          <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4">
              <input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search snippets..."
                className="w-full rounded-lg border border-[var(--line)] bg-transparent px-3 py-2 text-sm"
              />
              <div className="mt-3 max-h-[520px] space-y-2 overflow-y-auto">
                {filtered.map(({ element, path }) => (
                  <button
                    key={element.tag + path.join('-')}
                    type="button"
                    onClick={() => setSelectedTag(element.tag)}
                    className={`w-full rounded-lg border p-2 text-left text-xs ${
                      selectedTag === element.tag
                        ? 'border-[rgba(79,184,178,0.45)] bg-[rgba(79,184,178,0.14)]'
                        : 'border-[var(--line)] bg-[var(--surface-strong)]'
                    }`}
                  >
                    <p className="font-mono text-[var(--lagoon-deep)]">&lt;{element.tag}&gt;</p>
                    <p className="mt-1 text-[var(--sea-ink-soft)]">{path.slice(0, -1).join(' / ') || 'root'}</p>
                  </button>
                ))}
              </div>
            </div>

            <div className="rounded-[1.5rem] border border-[var(--line)] bg-[var(--surface)] p-4">
              <p className="text-xs uppercase tracking-[0.12em] text-[var(--kicker)]">Selected snippet</p>
              <p className="mt-1 font-mono text-sm text-[var(--sea-ink)]">&lt;{selectedEntry.element.tag}&gt;</p>
              <pre className="mt-3 overflow-x-auto rounded-xl border border-[var(--line)] bg-[rgba(0,0,0,0.03)] p-4 text-xs leading-5 text-[var(--sea-ink)]">
                {snippet}
              </pre>
              <button
                type="button"
                className="mt-3 rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)]"
                onClick={() => {
                  const textarea = document.getElementById('simml-editor-area') as HTMLTextAreaElement | null
                  if (!textarea) return
                  const start = textarea.selectionStart ?? source.length
                  const end = textarea.selectionEnd ?? source.length
                  const nextValue = `${source.slice(0, start)}${snippet}${source.slice(end)}`
                  setSource(nextValue)
                  requestAnimationFrame(() => {
                    const pos = start + snippet.length
                    textarea.focus()
                    textarea.setSelectionRange(pos, pos)
                  })
                }}
              >
                Insert at cursor
              </button>

              <textarea
                id="simml-editor-area"
                className="mt-4 min-h-[380px] w-full rounded-[1.5rem] border border-[rgba(141,229,219,0.18)] bg-[#0d2034] p-4 font-mono text-[12px] leading-6 text-[#e6f3ff] outline-none"
                value={source}
                onChange={(event) => setSource(event.target.value)}
                spellCheck={false}
              />
            </div>
          </div>
        </section>
      </div>
    </main>
  )
}
