import { createFileRoute, Link } from '@tanstack/react-router'
import { ARTICLE_ENTRIES } from '#/data/articles'

export const Route = createFileRoute('/articles')({
  component: ArticlesPage,
})

const formatDate = (date: string) =>
  new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(`${date}T12:00:00Z`))

function ArticlesPage() {
  const weeklyArticles = ARTICLE_ENTRIES.filter(
    (entry) => entry.type === 'article' && entry.status === 'published',
  )
  const newsletterArchive = ARTICLE_ENTRIES.filter(
    (entry) => entry.type === 'newsletter' && entry.status === 'published',
  )

  return (
    <main className="mx-auto max-w-7xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <p className="island-kicker mb-3">Articles & Newsletter Archive</p>
        <h1 className="display-title mb-3 text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Weekly forecasting insights
        </h1>
        <p className="m-0 max-w-3xl text-base leading-relaxed text-[var(--sea-ink-soft)]">
          New posts ship on a weekly cadence. Each issue includes a practical article and a newsletter
          summary you can share with your team.
        </p>
      </section>

      <section className="mt-8 grid gap-6 lg:grid-cols-2">
        <div className="island-shell rounded-2xl p-6 sm:p-7">
          <h2 className="text-2xl font-semibold text-[var(--sea-ink)]">Articles</h2>
          <ul className="mt-4 space-y-4">
            {weeklyArticles.map((entry) => (
              <li key={entry.slug} className="rounded-xl border border-[var(--line)] p-4">
                <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                  {formatDate(entry.publishedAt)}
                </p>
                <Link
                  to="/articles/$slug"
                  params={{ slug: entry.slug }}
                  className="text-lg font-semibold text-[var(--lagoon-deep)] no-underline"
                >
                  {entry.title}
                </Link>
                <p className="mt-2 text-sm leading-relaxed text-[var(--sea-ink-soft)]">{entry.summary}</p>
              </li>
            ))}
          </ul>
        </div>

        <div className="island-shell rounded-2xl p-6 sm:p-7">
          <h2 className="text-2xl font-semibold text-[var(--sea-ink)]">Newsletter archive</h2>
          <ul className="mt-4 space-y-4">
            {newsletterArchive.map((entry) => (
              <li key={entry.slug} className="rounded-xl border border-[var(--line)] p-4">
                <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)]">
                  {formatDate(entry.publishedAt)}
                </p>
                <Link
                  to="/articles/$slug"
                  params={{ slug: entry.slug }}
                  className="text-lg font-semibold text-[var(--lagoon-deep)] no-underline"
                >
                  {entry.title}
                </Link>
                <p className="mt-2 text-sm leading-relaxed text-[var(--sea-ink-soft)]">{entry.summary}</p>
              </li>
            ))}
          </ul>
        </div>
      </section>
    </main>
  )
}
