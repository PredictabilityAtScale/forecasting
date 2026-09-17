import type { CourseDefinition } from './course-types'
import { DATA_DRIVEN_QUIZZES } from './course-quizzes'
import { DATA_DRIVEN_SURVEYS } from './course-surveys'

const imageRoot = '/images/courses/data-driven-improvement'

export const DATA_DRIVEN_IMPROVEMENT_COURSE: CourseDefinition = {
  slug: 'data-driven-improvement',
  title: 'Data-Driven Improvement and Outcomes',
  shortTitle: 'Data-Driven Improvement',
  description:
    'Learn how to choose balanced measures, audit dashboards, and use evidence to improve team performance and outcomes.',
  image: '/images/FtjU4y5eRke8yzY5nxCD_course%20image.jpg',
  chapters: [
    {
      title: 'Welcome to the Course',
      lessons: [
        {
          archiveId: '38755825',
          slug: 'instructor-introduction',
          order: 1,
          title: 'A Message from the Instructor',
          chapter: 'Welcome to the Course',
          summary:
            'Welcome to the course and preview its structure and intended outcomes.',
          paragraphs: [
            'Before we start, I want to extend a warm welcome and give you a quick outline of how this course is structured and the outcomes it will deliver.',
          ],
          resources: [],
        },
        {
          archiveId: '38755827',
          slug: 'before-we-begin',
          order: 3,
          title: 'Before We Begin',
          chapter: 'Welcome to the Course',
          summary:
            'Capture your goals, current experience, and biggest roadblock before starting the course.',
          hasVideo: false,
          surveyQuestions: DATA_DRIVEN_SURVEYS.before,
          resources: [],
        },
      ],
    },
    {
      title: 'Measurement and Data Basics',
      lessons: [
        {
          archiveId: '38873197',
          slug: 'why-do-we-measure',
          order: 6,
          title: 'Theory: Why Do We Measure?',
          chapter: 'Measurement and Data Basics',
          summary:
            'Clarify what measurement is for and how data can support better decisions instead of becoming the goal itself.',
          images: [
            {
              src: `${imageRoot}/why-measure.png`,
              alt: 'Course diagram explaining why teams measure performance',
            },
          ],
          resources: [],
        },
        {
          archiveId: '40870587',
          slug: 'inventory-your-current-charts',
          order: 7,
          title: 'Exercise: Inventory Your Current Charts and Dashboards',
          chapter: 'Measurement and Data Basics',
          summary:
            'Take stock of the charts and dashboards your organization currently uses before judging or redesigning them.',
          images: [
            {
              src: `${imageRoot}/current-charts-exercise.png`,
              alt: 'Exercise prompts for reviewing current charts and dashboards',
            },
          ],
          resources: [],
        },
        {
          archiveId: '38755828',
          slug: 'golden-rules-of-measuring',
          order: 8,
          title: 'Theory: Golden Rules of Measuring People and Performance',
          chapter: 'Measurement and Data Basics',
          summary:
            'Use a small set of guardrails to prevent metrics from creating distorted incentives or misleading conclusions.',
          images: [
            {
              src: `${imageRoot}/golden-rules.png`,
              alt: 'Golden rules for measuring people and performance',
            },
          ],
          resources: [],
        },
        {
          archiveId: '40870648',
          slug: 'audit-your-current-charts',
          order: 9,
          title: 'Exercise: Audit Your Current Charts',
          chapter: 'Measurement and Data Basics',
          summary:
            'Evaluate your current reporting against the measurement rules from the previous lesson.',
          images: [
            {
              src: `${imageRoot}/chart-audit-exercise.png`,
              alt: 'Exercise for auditing charts against the measurement rules',
            },
          ],
          resources: [],
        },
        {
          archiveId: '38844410',
          slug: 'golden-rules-debrief',
          order: 10,
          title: 'Debrief/Key Points: Golden Rules of Measuring',
          chapter: 'Measurement and Data Basics',
          summary:
            'The original draft quiz placeholder preserved from the incremental course release.',
          hasVideo: false,
          quiz: DATA_DRIVEN_QUIZZES.goldenRules,
          resources: [],
        },
      ],
    },
    {
      title: 'Measuring Performance and Flow',
      lessons: [
        {
          archiveId: '38844886',
          slug: 'what-do-you-measure-now',
          order: 11,
          title: 'What Do You Measure Now?',
          chapter: 'Measuring Performance and Flow',
          summary:
            'Record the measures you currently use for process or team performance.',
          hasVideo: false,
          surveyQuestions: DATA_DRIVEN_SURVEYS.currentMeasures,
          resources: [],
        },
        {
          archiveId: '38755829',
          slug: 'opposing-performance-dimensions',
          order: 12,
          title: 'Finding Opposing Performance Dimensions',
          chapter: 'Measuring Performance and Flow',
          summary:
            'Balance measures that pull in different directions so improving one dimension does not quietly damage another.',
          images: [
            {
              src: `${imageRoot}/opposing-dimensions.png`,
              alt: 'Diagram showing opposing dimensions of performance',
            },
          ],
          resources: [],
        },
        {
          archiveId: '38844294',
          slug: 'find-your-opposing-dimensions',
          order: 13,
          title: 'Exercise: Find Your Opposing Dimensions',
          chapter: 'Measuring Performance and Flow',
          summary:
            'Identify the measures your team needs to hold in productive tension for balanced performance.',
          images: [
            {
              src: `${imageRoot}/opposing-dimensions-exercise.png`,
              alt: 'Exercise for identifying opposing performance dimensions',
            },
          ],
          resources: [],
        },
        {
          archiveId: '38844359',
          slug: 'opposing-dimensions-debrief',
          order: 14,
          title: 'Debrief/Key Points: Opposing Dimensions',
          chapter: 'Measuring Performance and Flow',
          summary:
            'The original draft quiz placeholder preserved from the incremental course release.',
          hasVideo: false,
          quiz: DATA_DRIVEN_QUIZZES.opposingDimensions,
          resources: [],
        },
        {
          archiveId: '38844317',
          slug: 'leading-measures-for-each-dimension',
          order: 15,
          title: 'Finding Leading Measures for Each Dimension',
          chapter: 'Measuring Performance and Flow',
          summary:
            'A preserved text-only placeholder from the original incremental course release.',
          hasVideo: false,
          note: 'Coming soon.',
          resources: [],
        },
      ],
    },
    {
      title: 'Before You Go',
      lessons: [
        {
          archiveId: '38755836',
          slug: 'test-your-learning',
          order: 32,
          title: 'Test Your Learning',
          chapter: 'Before You Go',
          summary:
            'The original assessment shell preserved from the incremental course release.',
          hasVideo: false,
          quiz: DATA_DRIVEN_QUIZZES.testYourLearning,
          resources: [],
        },
        {
          archiveId: '38755839',
          slug: 'before-you-go',
          order: 33,
          title: 'Before You Go',
          chapter: 'Before You Go',
          summary:
            'Reflect on your learning outcome, confidence, and likelihood of recommending the course.',
          hasVideo: false,
          surveyQuestions: DATA_DRIVEN_SURVEYS.after,
          resources: [],
        },
      ],
    },
  ],
}
