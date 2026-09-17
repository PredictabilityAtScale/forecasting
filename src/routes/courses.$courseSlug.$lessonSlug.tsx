import { createFileRoute, notFound } from '@tanstack/react-router'
import CoursePlayer from '#/components/CoursePlayer'
import { getCourseLesson } from '#/data/course-catalog'
import { SITE_URL } from '#/lib/site'

export const Route = createFileRoute('/courses/$courseSlug/$lessonSlug')({
  loader: ({ params }) => {
    const result = getCourseLesson(params.courseSlug, params.lessonSlug)
    if (!result) throw notFound()
    return result
  },
  head: ({ loaderData }) => {
    if (!loaderData)
      return { meta: [{ title: 'Free Training | Focused Objective' }] }

    const { course, lesson } = loaderData
    const title = `${lesson.title} | ${course.shortTitle} | Focused Objective`
    const canonical = `${SITE_URL}/courses/${course.slug}/${lesson.slug}`

    return {
      meta: [
        { title },
        { name: 'robots', content: 'noindex, nofollow' },
        { name: 'description', content: lesson.summary },
        { property: 'og:title', content: title },
        { property: 'og:description', content: lesson.summary },
        { property: 'og:url', content: canonical },
        { property: 'og:image', content: `${SITE_URL}${course.image}` },
        { name: 'twitter:card', content: 'summary_large_image' },
      ],
      links: [{ rel: 'canonical', href: canonical }],
    }
  },
  component: CourseLessonPage,
})

function CourseLessonPage() {
  const { course, lesson } = Route.useLoaderData()
  return <CoursePlayer course={course} lessonSlug={lesson.slug} />
}
