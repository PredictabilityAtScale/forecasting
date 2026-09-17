import type { CourseSurveyQuestion } from './course-types'

const outcomeChoices = [
  'Yes, I achieved my desired outcome.',
  "No, but I'm happy with my outcome in this course anyway.",
  "No, I didn't achieve my desired outcome.",
]

const scaleFive = ['1', '2', '3', '4', '5']
const ratingTen = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10']

function closingSurvey(
  ids: [string, string, string, string, string],
  recommendationPrompt = 'How likely are you to recommend this course to a friend or colleague?',
): CourseSurveyQuestion[] {
  return [
    {
      id: ids[0],
      type: 'single-choice',
      optional: false,
      prompt:
        'This is a quick survey to help me understand if this course has helped you reach your goals. Think back to when you first started this course and completed this statement: “After taking this course, I will be able to ________________________.” Is that statement still true now that you have completed the course?',
      choices: outcomeChoices,
    },
    {
      id: ids[1],
      type: 'scale',
      optional: true,
      prompt:
        "Now having completed this course, how experienced do you feel with this course's subject matter?",
      choices: scaleFive,
    },
    {
      id: ids[2],
      type: 'free-text',
      optional: true,
      prompt:
        'Please elaborate on how you feel about your outcome from learning with this online course.',
    },
    {
      id: ids[3],
      type: 'rating',
      optional: false,
      prompt: recommendationPrompt,
      choices: ratingTen,
    },
    {
      id: ids[4],
      type: 'free-text',
      optional: true,
      prompt:
        'If you have ANY suggestions or feedback on how to make this training better, let me know.',
    },
  ]
}

export const DEPENDENCY_SURVEYS = {
  goals: [
    {
      id: '4035958',
      type: 'free-text',
      optional: false,
      prompt:
        'When you registered for this workshop, what did you hope to learn? Make a few notes here describing your current issues and the goals you want from this workshop in solving these issues.',
    },
  ],
  definition: [
    {
      id: '4035979',
      type: 'free-text',
      optional: false,
      prompt: 'What is your current definition of “Dependency?”',
    },
  ],
  urgency: [
    {
      id: '4500585',
      type: 'free-text',
      optional: false,
      prompt: 'What makes some dependencies more urgent to fix than others?',
    },
  ],
  rootCauses: [
    {
      id: '4500690',
      type: 'free-text',
      optional: false,
      prompt:
        'What are the root causes of dependencies in your organization or team?',
    },
  ],
  feedback: closingSurvey([
    '4036752',
    '4036753',
    '4036754',
    '4036755',
    '4036756',
  ]),
} satisfies Record<string, CourseSurveyQuestion[]>

export const DATA_DRIVEN_SURVEYS = {
  before: [
    {
      id: '4700280',
      type: 'free-text',
      optional: false,
      prompt:
        'This is a quick survey to help me understand your goals so I can help you reach them. Fill in the blank: After taking this course, I will be able to ________________________.',
    },
    {
      id: '4700281',
      type: 'scale',
      optional: false,
      prompt: "How experienced do you feel in this course's subject matter?",
      choices: ['1 (very low)', '2', '3', '4', '5 (very high)'],
    },
    {
      id: '4700282',
      type: 'free-text',
      optional: false,
      prompt:
        "What's the biggest roadblock you have with this course's subject matter right now?",
    },
  ],
  currentMeasures: [
    {
      id: '4710217',
      type: 'free-text',
      optional: false,
      prompt:
        'What do you measure now for process or team performance purposes?',
    },
  ],
  after: [
    {
      id: '4700283',
      type: 'free-text',
      optional: false,
      prompt:
        'This is a quick survey to help me understand if this course has helped you reach your goals. Think back to the statement you completed when starting: “After taking this course, I will be able to ________________________.” Is that statement true now that you have completed the course?',
    },
    {
      id: '4700284',
      type: 'scale',
      optional: false,
      prompt:
        "Now having completed this course, how experienced do you feel with this course's subject matter?",
      choices: scaleFive,
    },
    {
      id: '4700285',
      type: 'free-text',
      optional: true,
      prompt:
        'Please elaborate on how you feel about your outcome from learning with this online course.',
    },
    {
      id: '4700286',
      type: 'rating',
      optional: true,
      prompt:
        'How likely are you to recommend this course to a friend, partner, or colleague?',
      choices: ratingTen,
    },
  ],
} satisfies Record<string, CourseSurveyQuestion[]>

export const MONTE_CARLO_SURVEYS = {
  before: [
    {
      id: '4012633',
      type: 'free-text',
      optional: false,
      prompt:
        'This is a quick survey to help me understand your goals so I can help you reach them. Fill in the blank: After taking this course, I will be able to ________________________.',
    },
    {
      id: '4012632',
      type: 'free-text',
      optional: true,
      prompt:
        "What's the biggest roadblock you have in forecasting right now? How do you forecast now? What works well, and what doesn't?",
    },
  ],
  feedback: closingSurvey([
    '4017429',
    '4024249',
    '4024250',
    '4024251',
    '4017430',
  ]),
} satisfies Record<string, CourseSurveyQuestion[]>

export const TEAM_DASHBOARD_SURVEYS = {
  before: [
    {
      id: '4024255',
      type: 'free-text',
      optional: false,
      prompt:
        'This is a quick survey to help me understand your goals so I can help you reach them. Fill in the blank: After taking this course, I will be able to ________________________.',
    },
    {
      id: '4024256',
      type: 'free-text',
      optional: true,
      prompt:
        "What's the biggest roadblock you have in gathering and showing team data right now? How do you show team data now? What works well, and what doesn't?",
    },
  ],
  feedback: closingSurvey([
    '4024297',
    '4024298',
    '4024299',
    '4024300',
    '4024301',
  ]),
} satisfies Record<string, CourseSurveyQuestion[]>
