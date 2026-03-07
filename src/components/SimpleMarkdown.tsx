import { Fragment, type ReactNode } from 'react'

interface SimpleMarkdownProps {
  content: string
}

function renderInline(text: string): ReactNode {
  const linkRegex = /\[([^\]]+)\]\(([^)]+)\)/g
  const nodes: ReactNode[] = []
  let lastIndex = 0
  let match: RegExpExecArray | null

  match = linkRegex.exec(text)
  while (match) {
    if (match.index > lastIndex) {
      nodes.push(text.slice(lastIndex, match.index))
    }

    nodes.push(
      <a
        key={`${match[2]}-${match.index}`}
        href={match[2]}
        target="_blank"
        rel="noopener noreferrer"
        className="text-[var(--lagoon-deep)] underline"
      >
        {match[1]}
      </a>,
    )

    lastIndex = match.index + match[0].length
    match = linkRegex.exec(text)
  }

  if (lastIndex < text.length) {
    nodes.push(text.slice(lastIndex))
  }

  if (nodes.length === 0) {
    return text
  }

  return nodes.map((node, index) => <Fragment key={`inline-${index}`}>{node}</Fragment>)
}

export default function SimpleMarkdown({ content }: SimpleMarkdownProps) {
  const lines = content.split('\n')
  const blocks: ReactNode[] = []

  for (let i = 0; i < lines.length; i += 1) {
    const line = lines[i]?.trim() ?? ''

    if (!line) {
      continue
    }

    if (line.startsWith('# ')) {
      blocks.push(
        <h1 key={`h1-${i}`} className="mt-2 text-3xl font-semibold text-[var(--sea-ink)] sm:text-4xl">
          {line.slice(2)}
        </h1>,
      )
      continue
    }

    if (line.startsWith('## ')) {
      blocks.push(
        <h2 key={`h2-${i}`} className="mt-8 text-2xl font-semibold text-[var(--sea-ink)]">
          {line.slice(3)}
        </h2>,
      )
      continue
    }

    if (line.startsWith('> ')) {
      blocks.push(
        <blockquote
          key={`quote-${i}`}
          className="my-5 border-l-4 border-[var(--line)] pl-4 italic text-[var(--sea-ink-soft)]"
        >
          {renderInline(line.slice(2))}
        </blockquote>,
      )
      continue
    }

    if (line.startsWith('- ')) {
      const items: ReactNode[] = []
      let current = i
      while (current < lines.length && (lines[current]?.trim() ?? '').startsWith('- ')) {
        items.push(<li key={`li-${current}`}>{renderInline((lines[current] ?? '').trim().slice(2))}</li>)
        current += 1
      }
      blocks.push(
        <ul key={`ul-${i}`} className="my-4 list-disc space-y-1 pl-6 text-[var(--sea-ink-soft)]">
          {items}
        </ul>,
      )
      i = current - 1
      continue
    }

    if (/^\d+\.\s/.test(line)) {
      const items: ReactNode[] = []
      let current = i
      while (current < lines.length && /^\d+\.\s/.test((lines[current] ?? '').trim())) {
        items.push(
          <li key={`ol-${current}`}>{renderInline((lines[current] ?? '').trim().replace(/^\d+\.\s/, ''))}</li>,
        )
        current += 1
      }
      blocks.push(
        <ol key={`ol-wrap-${i}`} className="my-4 list-decimal space-y-1 pl-6 text-[var(--sea-ink-soft)]">
          {items}
        </ol>,
      )
      i = current - 1
      continue
    }

    blocks.push(
      <p key={`p-${i}`} className="my-4 leading-relaxed text-[var(--sea-ink-soft)]">
        {renderInline(line)}
      </p>,
    )
  }

  return <article>{blocks}</article>
}
