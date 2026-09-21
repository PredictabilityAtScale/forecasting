import { AGILE_PHYSICS_COURSE } from './agile-physics-course'
import { DATA_DRIVEN_IMPROVEMENT_COURSE } from './data-driven-improvement-course'
import { DEPENDENCY_MANAGEMENT_COURSE } from './dependency-management-course'
import { MONTE_CARLO_SPREADSHEETS_COURSE } from './monte-carlo-spreadsheets-course'
import { TEAM_DASHBOARD_SPREADSHEET_COURSE } from './team-dashboard-spreadsheet-course'
import { COURSE_VIDEO_MAPPINGS } from './course-video-mappings'
import { flattenCourseLessons } from './course-types'
import type { CourseDefinition } from './course-types'

const youtubeIdsByLessonArchiveId = new Map(
  COURSE_VIDEO_MAPPINGS.map(({ lessonArchiveId, youtubeId }) => [
    lessonArchiveId,
    youtubeId,
  ]),
)

function attachYouTubeIds(course: CourseDefinition): CourseDefinition {
  return {
    ...course,
    chapters: course.chapters.map((chapter) => ({
      ...chapter,
      lessons: chapter.lessons.map((lesson) => ({
        ...lesson,
        youtubeId:
          youtubeIdsByLessonArchiveId.get(lesson.archiveId) ?? lesson.youtubeId,
      })),
    })),
  }
}

export const COURSE_CATALOG = [
  AGILE_PHYSICS_COURSE,
  DEPENDENCY_MANAGEMENT_COURSE,
  DATA_DRIVEN_IMPROVEMENT_COURSE,
  MONTE_CARLO_SPREADSHEETS_COURSE,
  TEAM_DASHBOARD_SPREADSHEET_COURSE,
].map(attachYouTubeIds)

export function getCourse(courseSlug: string) {
  return COURSE_CATALOG.find((course) => course.slug === courseSlug)
}

export function getCourseLesson(courseSlug: string, lessonSlug: string) {
  const course = getCourse(courseSlug)
  if (!course) return undefined

  const lesson = flattenCourseLessons(course).find(
    (candidate) => candidate.slug === lessonSlug,
  )
  if (!lesson) return undefined

  return { course, lesson }
}
