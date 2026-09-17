import type { CourseDefinition } from './course-types'
import { TEAM_DASHBOARD_QUIZZES } from './course-quizzes'
import { TEAM_DASHBOARD_SURVEYS } from './course-surveys'

const imageRoot = '/images/courses/team-dashboard-spreadsheet'

export const TEAM_DASHBOARD_SPREADSHEET_COURSE: CourseDefinition = {
  slug: 'team-dashboard-spreadsheet',
  title: 'Using the Team Dashboard Spreadsheet',
  shortTitle: 'Team Dashboard Spreadsheet',
  description:
    'Use the free Team Dashboard spreadsheet to visualize flow, balance performance measures, and support better coaching conversations.',
  image: '/images/vyrZfbZSaqpLjLCwN85b_course%20image.png',
  chapters: [
    {
      title: 'Welcome and Introduction',
      lessons: [
        {
          archiveId: '31991757',
          slug: 'about-this-course',
          order: 1,
          title: 'About This Course',
          chapter: 'Welcome and Introduction',
          summary:
            'Understand what the Team Dashboard provides and how the lessons are organized.',
          resources: [],
        },
        {
          archiveId: '31991760',
          slug: 'before-we-begin',
          order: 2,
          title: 'Before We Begin',
          chapter: 'Welcome and Introduction',
          summary:
            'Capture your dashboard goals and the biggest roadblock in your current reporting approach.',
          hasVideo: false,
          surveyQuestions: TEAM_DASHBOARD_SURVEYS.before,
          resources: [],
        },
        {
          archiveId: '31991771',
          slug: 'download-and-get-running',
          order: 3,
          title: 'Download the Spreadsheet and Get It Running',
          chapter: 'Welcome and Introduction',
          summary: 'Download the workbook and prepare it for your team’s data.',
          paragraphs: [
            'The original lesson provided the spreadsheet through the link below.',
          ],
          resources: [
            {
              label: 'Download the Team Dashboard spreadsheet',
              href: 'https://bit.ly/FlowDashboard',
            },
          ],
        },
      ],
    },
    {
      title: 'Using the Spreadsheet',
      lessons: [
        {
          archiveId: '31992725',
          slug: 'quick-walkthrough',
          order: 6,
          title: 'Quick Walkthrough',
          chapter: 'Using the Spreadsheet',
          summary:
            'Tour the workbook, its worksheets, and the main path from raw delivery data to useful charts.',
          resources: [],
        },
        {
          archiveId: '31993004',
          slug: 'add-your-data',
          order: 7,
          title: 'How to Add Your Data',
          chapter: 'Using the Spreadsheet',
          summary:
            'Prepare completed, started, and work-type fields so the dashboard can interpret them correctly.',
          bullets: [
            'Date format follows your local machine settings. Unrecognized dates remain left-aligned; recognized Excel dates are right-aligned.',
            'Keep uncompleted work at the bottom by sorting Completed Date from oldest to newest.',
            'For team views, Completed Date is ideally when work is marked Done; Started Date is when the team pulled it into a sprint or workflow.',
            'Choose one work type to represent defects or unplanned work in the Settings worksheet. Other types are treated as non-defects.',
            'Use Paste Values when copying data from another source.',
          ],
          resources: [],
        },
        {
          archiveId: '31993016',
          slug: 'team-dashboard-and-six-dimensions',
          order: 8,
          title: 'Team Dashboard and the Six Dimensions of Performance',
          chapter: 'Using the Spreadsheet',
          summary:
            'Understand the dashboard’s balanced performance model and configure the view for your team.',
          paragraphs: [
            'The Team Dashboard worksheet is based on the Six Dimensions of Performance model, extending the original four dimensions in the Software Development Performance Index by Larry Maccherone.',
          ],
          bullets: [
            'The dashboard supports four of the six dimensions and includes one selected chart from Other Charts; replace charts with your own when useful.',
            'Set the team name on the Settings worksheet.',
            'Use the controls to choose your data, filter by work type, and set the date range.',
            'The default three-month view helps workbook performance and keeps old data from stretching the date axis.',
            'Invent measures for Value and Sustainability that fit your team’s context.',
          ],
          images: [
            {
              src: `${imageRoot}/team-dashboard.png`,
              alt: 'Team Dashboard spreadsheet showing the Six Dimensions of Performance',
              caption:
                'The Team Dashboard view preserved from the original lesson.',
            },
          ],
          resources: [],
        },
        {
          archiveId: '31993031',
          slug: 'work-in-progress-and-age-chart',
          order: 9,
          title: 'Work in Progress and Age Chart',
          chapter: 'Using the Spreadsheet',
          summary:
            'Read daily work in progress by age band and configure warnings for sudden changes.',
          paragraphs: [
            'The chart shows work that has started but is not complete, day by day, color-coded by age.',
          ],
          bullets: [
            'Lowest, Middle, and Largest set the age-band thresholds. For example, 1, 7, and 14 days create light gray, dark gray, yellow, and orange bands.',
            'WIP Warning % marks a large day-over-day change with a red square above the WIP column.',
            'Age Warning % marks a large increase in older yellow and orange items with a red circle.',
          ],
          images: [
            {
              src: `${imageRoot}/wip-age-chart.png`,
              alt: 'Work in Progress and Age chart from the Team Dashboard spreadsheet',
            },
            {
              src: `${imageRoot}/wip-age-settings.png`,
              alt: 'Settings for WIP age bands and warning percentages',
              width: 'compact',
            },
          ],
          resources: [],
        },
        {
          archiveId: '31993068',
          slug: 'charts-and-coaching',
          order: 10,
          title: 'Charts and Coaching with Them',
          chapter: 'Using the Spreadsheet',
          summary:
            'Review the available charts and use them to guide evidence-based coaching conversations.',
          paragraphs: [
            'This lesson describes the available charts and how to coach using them. The video also includes an optional discussion of Little’s Law.',
          ],
          resources: [
            {
              label: "Read about Little's Law",
              href: 'https://en.wikipedia.org/wiki/Little%27s_law',
            },
          ],
        },
      ],
    },
    {
      title: 'Advanced Topics',
      lessons: [
        {
          archiveId: '31993032',
          slug: 'settings-and-options',
          order: 12,
          title: 'Settings and Options',
          chapter: 'Advanced Topics',
          summary:
            'Configure the dashboard’s filters, thresholds, labels, and other workbook options.',
          resources: [],
        },
      ],
    },
    {
      title: 'Before You Go',
      lessons: [
        {
          archiveId: '31991976',
          slug: 'team-dashboard-key-points-quiz',
          order: 13,
          title: 'Quiz on the Key Points',
          chapter: 'Before You Go',
          summary:
            'Check your understanding of date handling, work types, and the WIP and age chart.',
          hasVideo: false,
          quiz: TEAM_DASHBOARD_QUIZZES.keyPoints,
          resources: [],
        },
        {
          archiveId: '31992003',
          slug: 'course-feedback',
          order: 17,
          title: 'How Did We Do?',
          chapter: 'Before You Go',
          summary:
            'Reflect on your learning outcome and provide private feedback about the course.',
          hasVideo: false,
          surveyQuestions: TEAM_DASHBOARD_SURVEYS.feedback,
          resources: [],
        },
      ],
    },
  ],
}
