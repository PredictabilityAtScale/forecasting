import { AGILE_PHYSICS_COURSE } from './agile-physics-course'
import { DATA_DRIVEN_IMPROVEMENT_COURSE } from './data-driven-improvement-course'
import { DEPENDENCY_MANAGEMENT_COURSE } from './dependency-management-course'
import { MONTE_CARLO_SPREADSHEETS_COURSE } from './monte-carlo-spreadsheets-course'
import { TEAM_DASHBOARD_SPREADSHEET_COURSE } from './team-dashboard-spreadsheet-course'
import { flattenCourseLessons } from './course-types'

export const COURSE_CATALOG = [
  AGILE_PHYSICS_COURSE,
  DEPENDENCY_MANAGEMENT_COURSE,
  DATA_DRIVEN_IMPROVEMENT_COURSE,
  MONTE_CARLO_SPREADSHEETS_COURSE,
  TEAM_DASHBOARD_SPREADSHEET_COURSE,
]

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
