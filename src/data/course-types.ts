export type CourseResource = {
  label: string
  href: string
  embedUrl?: string
  embedHeight?: number
}

export type CourseImage = {
  src: string
  alt: string
  caption?: string
  width?: 'full' | 'compact'
}

export type CourseSurveyQuestion = {
  id: string
  type: 'free-text' | 'single-choice' | 'scale' | 'rating'
  prompt: string
  optional: boolean
  choices?: string[]
}

export type CourseQuizChoice = {
  text: string
  correct: boolean
}

export type CourseQuizQuestion = {
  id: string
  type: 'single-choice' | 'multiple-choice'
  prompt: string
  choices: CourseQuizChoice[]
  explanation?: string
  image?: CourseImage
}

export type CourseQuizLesson = {
  sourceLessonId: string
  draft?: boolean
  questions: CourseQuizQuestion[]
}

export type CourseLesson = {
  archiveId: string
  slug: string
  order: number
  title: string
  chapter: string
  summary: string
  hasVideo?: boolean
  youtubeId?: string
  note?: string
  paragraphs?: string[]
  bullets?: string[]
  images?: CourseImage[]
  surveyQuestions?: CourseSurveyQuestion[]
  quiz?: CourseQuizLesson
  resources: CourseResource[]
}

export type CourseChapter = {
  title: string
  lessons: CourseLesson[]
}

export type CourseDefinition = {
  slug: string
  title: string
  shortTitle: string
  description: string
  image: string
  chapters: CourseChapter[]
}

export function flattenCourseLessons(course: CourseDefinition) {
  return course.chapters.flatMap((chapter) => chapter.lessons)
}
