import { createFileRoute, Link, notFound } from '@tanstack/react-router'
import SimpleMarkdown from '#/components/SimpleMarkdown'
import { getEntryBySlug } from '#/data/articles'
import { SITE_URL } from '#/lib/site'

export const Route = createFileRoute('/articles/$slug')({
  loader: ({ params }) => {
    const entry = getEntryBySlug(params.slug)

    if (!entry) {
      throw notFound()
    }

    // Never show drafts in production builds.
    if (entry.status === 'draft' && !import.meta.env.DEV) {
      throw notFound()
    }

    return entry
  },
  head: ({ loaderData }) => {
    const entry = loaderData
    const title = `${entry.title} | Focused Objective`
    const description = buildMetaDescription(entry.summary, entry.markdown)
    const canonical = `${SITE_URL}/articles/${entry.slug}`
    const publishedTime = `${entry.publishedAt}T12:00:00Z`
    const heroImage = entry.heroImage?.trim() ? `${SITE_URL}${entry.heroImage}` : `${SITE_URL}/fo.jpg`

    return {
      meta: [
        { title },
        { name: 'description', content: description },
        { name: 'author', content: 'Troy Magennis' },
        { name: 'robots', content: 'index,follow,max-image-preview:large' },
        { property: 'og:type', content: 'article' },
        { property: 'og:site_name', content: 'Focused Objective' },
        { property: 'og:title', content: title },
        { property: 'og:description', content: description },
        { property: 'og:url', content: canonical },
        { property: 'og:image', content: heroImage },
        { property: 'article:published_time', content: publishedTime },
        { name: 'twitter:card', content: 'summary_large_image' },
        { name: 'twitter:title', content: title },
        { name: 'twitter:description', content: description },
        { name: 'twitter:image', content: heroImage },
      ],
      links: [{ rel: 'canonical', href: canonical }],
    }
  },
  component: ArticleDetailPage,
})

const formatDate = (date: string) =>
  new Intl.DateTimeFormat('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(`${date}T12:00:00Z`))

function ArticleDetailPage() {
  const entry = Route.useLoaderData()
  const articleContent = stripLeadingTitleHeading(entry.markdown, entry.title)

  return (
    <main className="mx-auto max-w-4xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <Link to="/articles" className="text-sm font-semibold text-[var(--lagoon-deep)] no-underline">
          ← Back to archive
        </Link>
        <p className="mt-4 text-xs font-semibold uppercase tracking-widest text-[var(--sea-ink-soft)]">
          {entry.type} · {formatDate(entry.publishedAt)}
        </p>
        <h1 className="mt-4 text-3xl font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          {entry.title}
        </h1>
        {entry.summary ? (
          <p className="mt-3 max-w-3xl text-lg leading-relaxed text-[var(--sea-ink-soft)]">{entry.summary}</p>
        ) : null}
        {entry.heroImage ? (
          <div className="mt-6 overflow-hidden rounded-2xl border border-[var(--line)] bg-[var(--sea-foam)]">
            <img
              src={entry.heroImage}
              alt=""
              loading="lazy"
              className="h-auto w-full"
              onError={(event) => {
                const img = event.currentTarget
                img.style.display = 'none'
              }}
            />
          </div>
        ) : null}
        <div className="mt-6">
          <SimpleMarkdown content={articleContent} />
        </div>
      </section>
    </main>
  )
}

function stripLeadingTitleHeading(markdown: string, title: string) {
  const lines = markdown.split(/\r?\n/)
  const firstLine = lines[0]?.trim() ?? ''
  if (!firstLine.startsWith('# ')) {
    return markdown
  }

  const heading = firstLine.slice(2).trim()
  if (normalizeTitle(heading) !== normalizeTitle(title)) {
    return markdown
  }

  let start = 1
  while (start < lines.length && !lines[start]?.trim()) {
    start += 1
  }
  return lines.slice(start).join('\n')
}

function normalizeTitle(value: string) {
  return value
    .toLowerCase()
    .replace(/['"“”‘’`]/g, '')
    .replace(/\s+/g, ' ')
    .trim()
}

function buildMetaDescription(summary: string, markdown: string) {
  if (summary.trim()) {
    return summary.trim()
  }

  const plainText = markdown
    .replace(/^#.*$/gm, '')
    .replace(/^[-*]\s+/gm, '')
    .replace(/\[([^\]]+)\]\([^)]+\)/g, '$1')
    .replace(/\s+/g, ' ')
    .trim()
  return plainText.slice(0, 180) || 'Forecasting insights from Focused Objective.'
}
