import { createFileRoute, Link, notFound } from '@tanstack/react-router'
import SimpleMarkdown from '#/components/SimpleMarkdown'
import { getEntryBySlug } from '#/data/articles'

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

  return (
    <main className="mx-auto max-w-4xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <Link to="/articles" className="text-sm font-semibold text-[var(--lagoon-deep)] no-underline">
          ← Back to archive
        </Link>
        <p className="mt-4 text-xs font-semibold uppercase tracking-widest text-[var(--sea-ink-soft)]">
          {entry.type} · {formatDate(entry.publishedAt)}
        </p>
        <div className="mt-4">
          <SimpleMarkdown content={entry.markdown} />
        </div>
      </section>
    </main>
  )
}
