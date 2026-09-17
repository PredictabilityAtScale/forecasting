import type { CourseDefinition } from './course-types'
import { MONTE_CARLO_QUIZZES } from './course-quizzes'
import { MONTE_CARLO_SURVEYS } from './course-surveys'

export const MONTE_CARLO_SPREADSHEETS_COURSE: CourseDefinition = {
  slug: 'monte-carlo-spreadsheets',
  title: 'Using the Monte Carlo Forecasting Spreadsheets',
  shortTitle: 'Monte Carlo Spreadsheets',
  description:
    'Learn how to use the free forecasting spreadsheets for single backlogs, multiple features, risk, changing delivery pace, and historical data.',
  image: '/images/lDj2J2QSqmc4VM8cze9w_six.png',
  chapters: [
    {
      title: 'Welcome and Introduction',
      lessons: [
        {
          archiveId: '31899300',
          slug: 'about-this-course',
          order: 1,
          title: 'About This Course',
          chapter: 'Welcome and Introduction',
          summary:
            'See what the forecasting spreadsheets can do and how the course is organized.',
          resources: [],
        },
        {
          archiveId: '31900564',
          slug: 'before-we-begin',
          order: 2,
          title: 'Before We Begin',
          chapter: 'Welcome and Introduction',
          summary:
            'Capture your forecasting goals and the biggest roadblock in your current approach.',
          hasVideo: false,
          surveyQuestions: MONTE_CARLO_SURVEYS.before,
          resources: [],
        },
        {
          archiveId: '31899364',
          slug: 'download-and-get-running',
          order: 3,
          title: 'Download the Spreadsheets and Get Them Running',
          chapter: 'Welcome and Introduction',
          summary:
            'Download the free workbooks and prepare them for your first probabilistic forecast.',
          resources: [
            {
              label: 'Download the Throughput Forecaster spreadsheet',
              href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Throughput%20Forecaster.xlsx',
            },
            {
              label: 'Download the Multiple Feature Cut Line spreadsheet',
              href: 'https://github.com/FocusedObjective/FocusedObjective.Resources/raw/master/Spreadsheets/Multiple%20Feature%20Cut%20Line%20Forecaster.xlsx',
            },
          ],
        },
      ],
    },
    {
      title: 'Using the Spreadsheets',
      lessons: [
        {
          archiveId: '31900429',
          slug: 'single-feature-forecasting',
          order: 6,
          title: 'Forecasting a Single Feature or Backlog',
          chapter: 'Using the Spreadsheets',
          summary:
            'Introduce the workflow for forecasting when a single backlog or feature is the unit of planning.',
          resources: [],
        },
        {
          archiveId: '31925174',
          slug: 'exercise-two-basic-single-backlog-forecast',
          order: 7,
          title: 'Exercise 2: Basic Forecasting of a Single Backlog',
          chapter: 'Using the Spreadsheets',
          summary:
            'Use the Throughput Forecaster to answer four increasingly realistic delivery questions.',
          hasVideo: false,
          quiz: MONTE_CARLO_QUIZZES.exerciseTwo,
          resources: [],
        },
        {
          archiveId: '31900385',
          slug: 'multiple-feature-forecasting',
          order: 8,
          title: 'Forecasting Multiple Features',
          chapter: 'Using the Spreadsheets',
          summary:
            'Use the multiple-feature forecaster to model a portfolio and understand likely cut lines.',
          resources: [],
        },
        {
          archiveId: '31900396',
          slug: 'team-throughput-or-velocity-data',
          order: 9,
          title: 'Using Team Throughput or Velocity Data',
          chapter: 'Using the Spreadsheets',
          summary:
            'Prepare historical delivery data for use as the sampling input to a forecast.',
          resources: [],
        },
        {
          archiveId: '31899502',
          slug: 'manage-work-splitting',
          order: 10,
          title: 'Managing the Splitting of Work',
          chapter: 'Using the Spreadsheets',
          summary:
            'Account for work splitting so the forecast and historical data stay comparable.',
          resources: [],
        },
        {
          archiveId: '31899638',
          slug: 'manage-risks',
          order: 11,
          title: 'Managing Risks and Things That Might Go Wrong',
          chapter: 'Using the Spreadsheets',
          summary:
            'Represent uncertain events explicitly instead of hiding them inside a single estimate.',
          resources: [],
        },
        {
          archiveId: '31899621',
          slug: 'delivery-pace-changes',
          order: 12,
          title: 'Handling Changes in Delivery Pace',
          chapter: 'Using the Spreadsheets',
          summary:
            'Adjust a forecast for vacations, staffing changes, disruptions, and other shifts in delivery pace.',
          resources: [],
        },
      ],
    },
    {
      title: 'Advanced Topics',
      lessons: [
        {
          archiveId: '31899637',
          slug: 'what-is-monte-carlo-forecasting',
          order: 13,
          title: 'What Is Monte Carlo Forecasting and Why Do I Care?',
          chapter: 'Advanced Topics',
          summary:
            'Build an intuitive understanding of simulation-based forecasting and why ranges are more useful than single-point promises.',
          note: 'Use the interactive article below to explore Monte Carlo forecasting directly.',
          resources: [
            {
              label: 'Open the Monte Carlo forecasting article',
              href: 'https://observablehq.com/@troymagennis/introduction-to-monte-carlo-forecasting',
              embedUrl:
                'https://observablehq.com/embed/@troymagennis/introduction-to-monte-carlo-forecasting?cell=*',
              embedHeight: 1100,
            },
          ],
        },
        {
          archiveId: '31899541',
          slug: 'how-much-historical-data',
          order: 14,
          title: 'How Much Historical Data Do I Need?',
          chapter: 'Advanced Topics',
          summary:
            'Explore how sample size affects forecast stability and how much delivery history is enough.',
          note: 'The interactive article below explores the tradeoff between sample size and useful forecast accuracy.',
          resources: [
            {
              label: 'Open the historical sample-size article',
              href: 'https://observablehq.com/@troymagennis/how-many-samples-do-i-need',
              embedUrl:
                'https://observablehq.com/embed/@troymagennis/how-many-samples-do-i-need?cell=*',
              embedHeight: 1000,
            },
          ],
        },
        {
          archiveId: '31899645',
          slug: 'spreadsheet-settings-and-options',
          order: 15,
          title: 'Spreadsheet Settings and Options',
          chapter: 'Advanced Topics',
          summary:
            'Configure the workbook settings and optional controls for your forecasting context.',
          resources: [],
        },
      ],
    },
    {
      title: 'Before You Go',
      lessons: [
        {
          archiveId: '31926282',
          slug: 'monte-carlo-key-points-quiz',
          order: 16,
          title: 'Quiz on the Key Points',
          chapter: 'Before You Go',
          summary:
            'Check your understanding of the forecasting workbooks, data requirements, and story-size distributions.',
          hasVideo: false,
          quiz: MONTE_CARLO_QUIZZES.keyPoints,
          resources: [],
        },
        {
          archiveId: '31927336',
          slug: 'course-feedback',
          order: 19,
          title: 'How Did We Do?',
          chapter: 'Before You Go',
          summary:
            'Reflect on your learning outcome and provide private feedback about the course.',
          hasVideo: false,
          surveyQuestions: MONTE_CARLO_SURVEYS.feedback,
          resources: [],
        },
      ],
    },
  ],
}
