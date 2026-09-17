import type { CourseDefinition } from './course-types'
import { flattenCourseLessons } from './course-types'
import { AGILE_PHYSICS_QUIZZES } from './course-quizzes'

export const AGILE_PHYSICS_COURSE: CourseDefinition = {
  slug: 'agile-physics',
  title: 'Agile Physics — The Math of Flow',
  shortTitle: 'Agile Physics',
  description:
    'Short, practical explanations of the mathematics behind flow, queues, utilization, parallel work, and cycle-time distributions.',
  image: '/images/iFKveizWTQKaD8If0uAm_Scientific-Formulas.jpg',
  chapters: [
    {
      title: 'Before we begin',
      lessons: [
        {
          archiveId: '32278706',
          slug: 'welcome-and-about-this-course',
          order: 1,
          title: 'Welcome and about this course',
          chapter: 'Before we begin',
          summary:
            'Meet the course and see how a little probability, statistics, and queueing theory can explain the behavior of software delivery systems.',
          resources: [],
        },
      ],
    },
    {
      title: 'System Utilization',
      lessons: [
        {
          archiveId: '32314048',
          slug: 'system-utilization-and-queuing-time',
          order: 3,
          title: 'System Utilization and Queuing Time',
          chapter: 'System Utilization',
          summary:
            'Explore why queueing time rises rapidly as a delivery system approaches full utilization.',
          note: "The interactive notebook below explains Kingman's estimation and lets you explore how utilization changes lead time.",
          resources: [
            {
              label: "Explore Kingman's Estimation notebook",
              href: 'https://observablehq.com/@troymagennis/how-does-utilization-impact-lead-time-of-work',
              embedUrl:
                'https://observablehq.com/embed/@troymagennis/how-does-utilization-impact-lead-time-of-work?cell=*',
            },
          ],
        },
        {
          archiveId: '32314085',
          slug: 'system-utilization-pop-quiz',
          order: 4,
          title: 'System Utilization Key Points Pop Quiz',
          chapter: 'System Utilization',
          summary:
            'Check your understanding of variability, utilization, and queueing time.',
          hasVideo: false,
          quiz: AGILE_PHYSICS_QUIZZES.systemUtilization,
          resources: [],
        },
        {
          archiveId: '32314064',
          slug: 'financial-impact-of-high-utilization',
          order: 5,
          title: 'Calculating the Financial Impact of High System Utilization',
          chapter: 'System Utilization',
          summary:
            'Connect high utilization and longer queues to the economic cost of delayed delivery.',
          note: 'Use the full interactive notebook below to explore the economic impact of high system utilization.',
          resources: [
            {
              label: 'Open the economic impact notebook',
              href: 'https://observablehq.com/@troymagennis/the-economic-impact-of-high-system-utilization',
              embedUrl:
                'https://observablehq.com/embed/@troymagennis/the-economic-impact-of-high-system-utilization?cell=*',
            },
          ],
        },
        {
          archiveId: '32355403',
          slug: 'understanding-kingmans-formula',
          order: 6,
          title: 'Understanding the Formula (be brave)',
          chapter: 'System Utilization',
          summary:
            "Look beneath the intuition and examine the variables in Kingman's formula.",
          note: 'These references provide the mathematical background for the formula discussed in the lesson.',
          resources: [
            {
              label: "Kingman's formula on Wikipedia",
              href: 'https://en.wikipedia.org/wiki/Kingman%27s_formula',
            },
            {
              label: "More explanations of Kingman's formula",
              href: 'https://www.google.com/search?q=kingman%27s+formula',
            },
          ],
        },
      ],
    },
    {
      title: 'Likely Improvement by Adding Teams or People',
      lessons: [
        {
          archiveId: '33130007',
          slug: 'amdahls-law',
          order: 8,
          title: "Amdahl's Law — Speedup in Parallel Systems",
          chapter: 'Likely Improvement by Adding Teams or People',
          summary:
            'Understand the hard limit on improvement when only part of a system can be parallelized.',
          note: "The reference article below gives the formal definition and derivation of Amdahl's law.",
          resources: [
            {
              label: "Amdahl's law on Wikipedia",
              href: 'https://en.wikipedia.org/wiki/Amdahl%27s_law',
            },
          ],
        },
        {
          archiveId: '33132408',
          slug: 'adding-teams-pop-quiz',
          order: 10,
          title: 'Scaling by Adding Teams or People Pop Quiz',
          chapter: 'Likely Improvement by Adding Teams or People',
          summary:
            "Check your understanding of team independence and Amdahl's Law.",
          hasVideo: false,
          quiz: AGILE_PHYSICS_QUIZZES.addingTeams,
          resources: [],
        },
      ],
    },
    {
      title: 'Cycle Time (or Lead Time) Distribution',
      lessons: [
        {
          archiveId: '33226104',
          slug: 'understanding-cycle-time-distributions',
          order: 11,
          title: 'Understanding Cycle Time Distributions',
          chapter: 'Cycle Time (or Lead Time) Distribution',
          summary:
            'An introduction to the history and shape of cycle-time distributions and why their variability matters.',
          resources: [],
        },
      ],
    },
  ],
}

export const AGILE_PHYSICS_LESSONS = flattenCourseLessons(AGILE_PHYSICS_COURSE)

export const FIRST_AGILE_PHYSICS_LESSON = AGILE_PHYSICS_LESSONS[0]

export function getAgilePhysicsLesson(slug: string) {
  return AGILE_PHYSICS_LESSONS.find((lesson) => lesson.slug === slug)
}
