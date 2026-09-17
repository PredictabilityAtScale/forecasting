import { createFileRoute, notFound } from '@tanstack/react-router'
import CoursePlayer from '#/components/CoursePlayer'
import { getCourse } from '#/data/course-catalog'
import { flattenCourseLessons } from '#/data/course-types'
import { SITE_URL } from '#/lib/site'

export const Route = createFileRoute('/courses/$courseSlug/')({
  loader: ({ params }) => {
    const course = getCourse(params.courseSlug)
    if (!course) throw notFound()

    const firstLesson = flattenCourseLessons(course).at(0)
    if (!firstLesson) throw notFound()

    return { course, firstLesson }
  },
  head: ({ loaderData }) => {
    if (!loaderData)
      return { meta: [{ title: 'Free Training | Focused Objective' }] }

    const { course, firstLesson } = loaderData
    const title = `${course.title} | Focused Objective`
    const canonical = `${SITE_URL}/courses/${course.slug}`

    return {
      meta: [
        { title },
        { name: 'robots', content: 'noindex, nofollow' },
        { name: 'description', content: course.description },
        { property: 'og:title', content: title },
        { property: 'og:description', content: course.description },
        { property: 'og:url', content: canonical },
        { property: 'og:image', content: `${SITE_URL}${course.image}` },
        { name: 'twitter:card', content: 'summary_large_image' },
      ],
      links: [{ rel: 'canonical', href: canonical }],
      scripts: [
        {
          type: 'application/ld+json',
          children: JSON.stringify({
            '@context': 'https://schema.org',
            '@type': 'Course',
            name: course.title,
            description: course.description,
            provider: {
              '@type': 'Organization',
              name: 'Focused Objective',
              url: SITE_URL,
            },
            hasCourseInstance: {
              '@type': 'CourseInstance',
              courseMode: 'online',
              courseWorkload: `${flattenCourseLessons(course).length} lessons`,
            },
            firstLesson: firstLesson.title,
          }),
        },
      ],
    }
  },
  component: CourseIndexPage,
})

function CourseIndexPage() {
  const { course, firstLesson } = Route.useLoaderData()
  return <CoursePlayer course={course} lessonSlug={firstLesson.slug} />
}
