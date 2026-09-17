import { createFileRoute, Link } from '@tanstack/react-router'
import { COURSE_CATALOG } from '#/data/course-catalog'
import { flattenCourseLessons } from '#/data/course-types'
import { SITE_URL } from '#/lib/site'

export const Route = createFileRoute('/courses/')({
  head: () => {
    const title = 'Free Training Courses | Focused Objective'
    const description =
      'Free self-paced courses on flow, dependency management, metrics, Monte Carlo forecasting, and team dashboards.'
    const canonical = `${SITE_URL}/courses`

    return {
      meta: [
        { title },
        { name: 'description', content: description },
        { property: 'og:title', content: title },
        { property: 'og:description', content: description },
        { property: 'og:url', content: canonical },
        { name: 'twitter:card', content: 'summary_large_image' },
      ],
      links: [{ rel: 'canonical', href: canonical }],
    }
  },
  component: CourseLibraryPage,
})

function CourseLibraryPage() {
  return (
    <main className="mx-auto max-w-7xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rise-in relative overflow-hidden rounded-3xl px-6 py-12 sm:px-10 sm:py-16">
        <div className="pointer-events-none absolute -left-24 -top-28 h-64 w-64 rounded-full bg-[radial-gradient(circle,rgba(79,184,178,0.30),transparent_66%)]" />
        <div className="pointer-events-none absolute -bottom-24 -right-24 h-64 w-64 rounded-full bg-[radial-gradient(circle,rgba(47,106,74,0.16),transparent_66%)]" />
        <div className="relative max-w-4xl">
          <p className="island-kicker mb-3">Focused Objective Course Library</p>
          <h1 className="display-title m-0 text-4xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-6xl">
            Practical training, freely available.
          </h1>
          <p className="mb-0 mt-5 max-w-3xl text-base leading-relaxed text-[var(--sea-ink-soft)] sm:text-lg">
            Work through the original Focused Objective self-paced courses at
            your own pace. No account is required, and lesson progress stays
            privately in your browser.
          </p>
        </div>
      </section>

      <section
        className="mt-8 grid gap-5 md:grid-cols-2 lg:grid-cols-3"
        aria-label="Courses"
      >
        {COURSE_CATALOG.map((course) => {
          const lessonCount = flattenCourseLessons(course).length

          return (
            <Link
              key={course.slug}
              to="/courses/$courseSlug"
              params={{ courseSlug: course.slug }}
              className="feature-card group flex h-full flex-col overflow-hidden rounded-2xl border border-[var(--line)] bg-[var(--surface)] no-underline transition hover:-translate-y-0.5"
            >
              <div className="relative overflow-hidden">
                <img
                  src={course.image}
                  alt={`${course.shortTitle} course cover`}
                  className="h-48 w-full object-cover transition duration-300 group-hover:scale-[1.02]"
                  loading="lazy"
                />
                <span className="absolute left-4 top-4 rounded-full bg-[rgba(10,35,40,0.82)] px-3 py-1 text-xs font-bold uppercase tracking-wider text-white backdrop-blur-sm">
                  Free
                </span>
              </div>
              <div className="flex flex-1 flex-col p-5">
                <p className="mb-2 text-xs font-bold uppercase tracking-[0.12em] text-[var(--palm)]">
                  {lessonCount} lessons · {course.chapters.length} topics
                </p>
                <h2 className="m-0 text-2xl font-semibold text-[var(--sea-ink)] group-hover:text-[var(--lagoon-deep)]">
                  {course.title}
                </h2>
                <p className="mb-0 mt-3 flex-1 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
                  {course.description}
                </p>
                <span className="mt-5 inline-flex items-center gap-1 text-sm font-semibold text-[var(--lagoon-deep)]">
                  Start course →
                </span>
              </div>
            </Link>
          )
        })}
      </section>
    </main>
  )
}
