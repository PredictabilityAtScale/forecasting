import type {
  CourseImage,
  CourseQuizLesson,
  CourseQuizQuestion,
} from './course-types'

type QuizQuestionInput = Omit<CourseQuizQuestion, 'id' | 'choices'> & {
  choices: string[]
  correct: number | number[]
}

function question(
  sourceLessonId: string,
  index: number,
  input: QuizQuestionInput,
): CourseQuizQuestion {
  const correctIndexes = new Set(
    Array.isArray(input.correct) ? input.correct : [input.correct],
  )

  return {
    id: `${sourceLessonId}-${index}`,
    type: input.type,
    prompt: input.prompt,
    choices: input.choices.map((text, choiceIndex) => ({
      text,
      correct: correctIndexes.has(choiceIndex),
    })),
    explanation: input.explanation,
    image: input.image,
  }
}

function quiz(
  sourceLessonId: string,
  inputs: QuizQuestionInput[],
  draft = false,
): CourseQuizLesson {
  return {
    sourceLessonId,
    draft,
    questions: inputs.map((input, index) =>
      question(sourceLessonId, index + 1, input),
    ),
  }
}

const quizImage = (
  src: string,
  alt: string,
  caption?: string,
): CourseImage => ({ src, alt, caption, width: 'full' })

export const AGILE_PHYSICS_QUIZZES = {
  systemUtilization: quiz('32314085', [
    {
      type: 'single-choice',
      prompt: 'What variability impacts the queueing time most?',
      choices: [
        'Cycle time variability (service time of each work item)',
        'Arrival time variability (the time between each new work item arriving into our queue)',
        'Both are equally impactful',
      ],
      correct: 2,
      explanation:
        'Cycle time variability and arrival time variability are EQUAL in impact. They BOTH need to be managed effectively in high-utilization systems.',
    },
    {
      type: 'single-choice',
      prompt:
        'Is 80% utilization always the point where queuing time exponentially grows?',
      choices: ['Yes', 'No'],
      correct: 1,
      explanation:
        '80% utilization is for moderate variability, but it could be even LOWER for high variability arrival-rate or cycle-time systems.',
    },
    {
      type: 'single-choice',
      prompt:
        'If there is NO variability of cycle time or arrival rate, can I make my developers work at 100% utilization?',
      choices: ['Yes', 'No'],
      correct: 0,
      explanation:
        "Yes, but don't. Systems are NEVER free of variability. You wouldn't run your servers at 100% CPU utilization and expect no problems!",
    },
  ]),
  addingTeams: quiz('33132408', [
    {
      type: 'single-choice',
      prompt:
        'Do 4 teams deliver 4x faster than 1 team in software development?',
      choices: ['Yes', 'No', "It Depends (I'm a consultant)"],
      correct: 2,
      explanation:
        'The answer is MOSTLY "NO," but if those 4 teams can work independently and deliver without ANY synchronization or dependent steps, then yes. To get to yes, eliminate all sources of dependency and synchronization between those people and teams.',
    },
    {
      type: 'single-choice',
      prompt: 'What do I have to do to benefit from more teams and people?',
      choices: [
        'Make them work harder',
        'Hire better people',
        'Maximize team independence by increasing the parallelizable proportion of their work',
      ],
      correct: 2,
      explanation:
        "Amdahl's Law says that we pay a high price for any part of a task that cannot be performed independently. We need to increase the independently parallelizable proportion.",
    },
  ]),
}

export const DEPENDENCY_MANAGEMENT_QUIZZES = {
  whatAreDependencies: quiz('32106766', [
    {
      type: 'single-choice',
      prompt: 'What is a “Dependency?”',
      choices: [
        'Unable to start or finish something in the past',
        'Unable to start or finish something in the future',
      ],
      correct: 1,
      explanation:
        'The difference between a blocker and a dependency is when it happens. Work was blocked from starting or finishing in the past; a dependency is now or possibly in the future.',
    },
    {
      type: 'single-choice',
      prompt: 'Which of the following are examples of dependencies?',
      choices: [
        'Inability to start or finish something until you learn something',
        'Inability to start or finish something until you get something',
        'Inability to start or finish something until a skill, resource, or person does something',
        'All of the above',
      ],
      correct: 3,
      explanation:
        'All of these block work from being started or finished. Needing to learn, needing a skill we do not have, or needing material or other work are all dependencies that need managing.',
    },
    {
      type: 'single-choice',
      prompt: 'What is the goal of Dependency Management?',
      choices: [
        'To eliminate ALL dependencies',
        'To be aware of dependencies and do nothing',
        'To be aware of dependencies and consider actions to eliminate or cope with them',
      ],
      correct: 2,
      explanation:
        'Resist the temptation to eliminate all dependencies. Even if that were possible, the cost could exceed the time and cost saved. Wisely choose which dependencies to address.',
    },
  ]),
  prioritizing: quiz('32110504', [
    {
      type: 'single-choice',
      prompt: 'Why do we need to prioritize dependencies?',
      choices: [
        'Because all of the cool kids are doing it',
        'Because removing dependencies can be expensive',
        'Because some dependencies only impact the past and not the future',
        'All of the above',
      ],
      correct: 3,
      explanation:
        'Many dependencies require changes to how people work and can be expensive in time, change stress, and cost. We need to make sure we fix the dependencies that matter most.',
    },
    {
      type: 'multiple-choice',
      prompt:
        'Choose all the factors that need to be considered when prioritizing dependencies for “fixing.”',
      choices: [
        'How often they occurred in the recent past (frequency)',
        "How much they affect the CEO's pet feature or product",
        'How many days are lost due to being blocked by the dependency',
        'How likely a similar dependency is to occur in the future',
        'How easy they are to fix',
      ],
      correct: [0, 2, 3],
      explanation:
        'Recency, frequency, and impact are the primary prioritization drivers. In product development, whether future work is likely to encounter similar dependencies is also important.',
    },
    {
      type: 'single-choice',
      prompt:
        'Are all dependencies in the high-frequency and high-impact quadrant equal? Do we always start at the top right?',
      choices: ['Yes, always', 'No, sometimes'],
      correct: 1,
      explanation:
        'Also consider the urgency of the work being blocked. Prioritize dependencies in the top-right quadrant by the urgency of the work they commonly block.',
    },
  ]),
  solving: quiz('32110740', [
    {
      type: 'single-choice',
      prompt: 'Should we solve ALL dependencies?',
      choices: ['Yes', 'No'],
      correct: 1,
      explanation:
        'Solving every dependency is expensive and unlikely to be achievable. Prioritize dependencies likely to remain a problem, that affect urgent work, and that occur frequently or have unacceptable impact.',
    },
    {
      type: 'single-choice',
      prompt:
        'Do we solve dependencies by reducing frequency of occurrence OR impact?',
      choices: [
        'Frequency—we never want these dependencies to occur',
        'Impact—we want zero time to unblock when they occur',
        'Either frequency or impact, whichever is easier',
      ],
      correct: 2,
      explanation:
        'Some dependencies are solved faster by reducing frequency; others by reducing impact. Brainstorm from both directions and choose achievable experiments that affect one or both.',
    },
    {
      type: 'single-choice',
      prompt: 'How do you know your experiments worked?',
      choices: [
        'Frequency of occurrence decreases',
        'Days lost due to impact decreases',
        'The INTENDED frequency OR impact decreases!',
      ],
      correct: 2,
      explanation:
        'When you designed the process change, you expected it to affect one measure or the other. Learning why things change is important for choosing the right experiment next time.',
    },
  ]),
  chapterFiveDraft: quiz(
    '32110822',
    [
      {
        type: 'single-choice',
        prompt: 'What is your question?',
        choices: ['Yes', 'No'],
        correct: 0,
      },
    ],
    true,
  ),
  keyPoints: quiz('32110863', [
    {
      type: 'single-choice',
      prompt:
        'Are dependencies solved by reorganizing teams with all the skills they need?',
      choices: ['Yes', 'No'],
      correct: 1,
      explanation:
        'Many dependency types are not solved by a reorganization. Some are solved by learning something or getting something such as images, content, or servers. Dependencies are anything that will inhibit starting or finishing something we want in the future.',
    },
    {
      type: 'single-choice',
      prompt: 'Should we solve and eliminate all dependencies?',
      choices: ["Yes, that's the goal.", 'No…'],
      correct: 1,
      explanation:
        'Some dependencies will exist no matter how much effort we expend. Capture how frequently dependencies occur and how long they block work, then prioritize those that are frequently impactful and likely to recur.',
    },
  ]),
}

const placeholderQuiz = (sourceLessonId: string) =>
  quiz(
    sourceLessonId,
    [
      {
        type: 'single-choice',
        prompt: 'What is your question?',
        choices: ['Yes', 'No'],
        correct: 0,
      },
    ],
    true,
  )

export const DATA_DRIVEN_QUIZZES = {
  goldenRules: placeholderQuiz('38844410'),
  opposingDimensions: placeholderQuiz('38844359'),
  testYourLearning: quiz(
    '38755836',
    [
      {
        type: 'single-choice',
        prompt:
          'This quiz is a short assessment to help solidify the learning you just did. Question #1',
        choices: ['Answer #1', 'Answer #2'],
        correct: 0,
        explanation: 'Explanation',
      },
    ],
    true,
  ),
}

const monteCarloImageRoot = '/images/courses/quiz-assets'

export const MONTE_CARLO_QUIZZES = {
  exerciseTwo: quiz('31925174', [
    {
      type: 'single-choice',
      prompt:
        'How many weeks at 85% certainty would it take to deliver 25 well-understood stories, with no splitting, at 5 stories per week, when the team is 100% focused on this work?',
      choices: ['10', '8', '5'],
      correct: 2,
      explanation:
        'Although you could have done this in your head, 5 weeks is the correct answer.',
      image: quizImage(
        `${monteCarloImageRoot}/monte-carlo-q1.jpg`,
        'Throughput Forecaster showing a five-week result',
        'The spreadsheet setup and result preserved from the original explanation.',
      ),
    },
    {
      type: 'single-choice',
      prompt:
        'How many weeks at 85% certainty would it take to deliver 25–30 stories, with work splitting between 1–2x, throughput of 3–10 stories per week (most likely 5), and the team 50% focused on this work?',
      choices: ['9 to 10', '17 to 18', '24 to 25'],
      correct: 1,
      explanation: '17 to 18 weeks is the correct answer.',
      image: quizImage(
        `${monteCarloImageRoot}/monte-carlo-q2.jpg`,
        'Throughput Forecaster showing a 17-to-18-week result',
        'The spreadsheet setup and result preserved from the original explanation.',
      ),
    },
    {
      type: 'single-choice',
      prompt:
        'Can the same work be delivered with 85% certainty by April 1, 2023, if it starts January 1, 2023?',
      choices: [
        'Yes, we can deliver by April 1, 2023.',
        'No, we cannot deliver with 85% certainty by that date.',
      ],
      correct: 1,
      explanation:
        'No. The 85th-percentile delivery date is approximately May 5.',
      image: quizImage(
        `${monteCarloImageRoot}/monte-carlo-q3.jpg`,
        'Throughput Forecaster showing an 85th-percentile date in May',
      ),
    },
    {
      type: 'single-choice',
      prompt:
        'If the team were 100% dedicated to this work, could it be delivered with 85% certainty by April 1, 2023, starting January 1, 2023?',
      choices: [
        'Yes, we can deliver by April 1, 2023.',
        'No, we cannot deliver with 85% certainty by that date.',
      ],
      correct: 0,
      explanation:
        'Yes. The 85th-percentile delivery date is approximately March 5.',
      image: quizImage(
        `${monteCarloImageRoot}/monte-carlo-q4.jpg`,
        'Throughput Forecaster showing an 85th-percentile date in March',
      ),
    },
  ]),
  keyPoints: quiz('31926282', [
    {
      type: 'single-choice',
      prompt:
        'If I want to understand the likely delivery date of a single feature or backlog, which spreadsheet should I use?',
      choices: [
        'Throughput Forecaster',
        'Multiple Feature Cut-Line Forecaster',
      ],
      correct: 0,
      explanation:
        'The Throughput Forecaster works with a single low and high guess of backlog size. It is the right tool for a team focused on a single batch of scope.',
    },
    {
      type: 'single-choice',
      prompt:
        'I want to work with stakeholders to hit a specific target delivery date. Which spreadsheet should I use?',
      choices: [
        'Throughput Forecaster',
        'Multiple Feature Cut-Line Forecaster',
      ],
      correct: 1,
      explanation:
        'The Multiple Feature Cut-Line Forecaster shows how much scope will likely be delivered by a target date and supports prioritization discussions.',
    },
    {
      type: 'single-choice',
      prompt: 'What measure of development pace can I use?',
      choices: [
        'Throughput (stories delivered per week or sprint)',
        'Velocity (sum of story points delivered per week or sprint)',
        'Either',
      ],
      correct: 2,
      explanation:
        'Either measure works, but be consistent. With velocity, scope must be in points; with throughput, scope must be a number of stories.',
    },
    {
      type: 'single-choice',
      prompt:
        'When using historical team data, how many weeks or sprints do I need before I can rely on the answer?',
      choices: ['100 or more', '1000 or more', '10 or more'],
      correct: 2,
      explanation:
        'The sweet spot is approximately 7–15 samples. Aim for 10% or less data instability and ensure the samples remain representative of the work ahead.',
    },
    {
      type: 'single-choice',
      prompt: 'Do ALL stories have to be the same size?',
      choices: ['Yes', 'No'],
      correct: 1,
      explanation:
        'The stories can be different sizes. What matters is that the distribution of sizes remains comparable to the historical distribution used for the forecast.',
    },
  ]),
}

export const TEAM_DASHBOARD_QUIZZES = {
  keyPoints: quiz('31991976', [
    {
      type: 'single-choice',
      prompt: 'How are dates formatted in this spreadsheet?',
      choices: [
        'The date format of your machine based on where you live',
        'US format MM/DD/YYYY',
      ],
      correct: 0,
      explanation:
        'Although the course screen used US-formatted dates, the spreadsheet follows the local settings of your machine.',
    },
    {
      type: 'single-choice',
      prompt: 'Which date is invalid here? (Choose the cell row number.)',
      choices: ['8 (1/29/2015)', '9 (21/2/2015)', '10 (2/2/2015)'],
      correct: 1,
      explanation:
        'Excel right-aligns recognized dates. A left-aligned date is being treated as text and will not work.',
      image: quizImage(
        '/images/courses/quiz-assets/team-dashboard-invalid-date.jpg',
        'Spreadsheet rows showing one date stored as text',
      ),
    },
    {
      type: 'single-choice',
      prompt: 'How many “Defect” types can I have in my data?',
      choices: ['One', 'Many'],
      correct: 0,
      explanation:
        'You can have unlimited work types, but only one can be designated as the unplanned or defect type in the Settings worksheet.',
      image: quizImage(
        '/images/courses/quiz-assets/team-dashboard-settings.jpg',
        'Team Dashboard setting for the unplanned work type',
      ),
    },
    {
      type: 'single-choice',
      prompt: "Why is the WIP and age chart Troy's favorite?",
      choices: [
        'The colors—he likes color',
        'The age of work in progress is a leading indicator of future cycle time',
        'The Spotify model uses WIP',
      ],
      correct: 1,
      explanation:
        "Throughput and velocity are measured after work is complete. Work in progress can still be fixed. Today's WIP trend will become tomorrow's throughput and cycle-time trend.",
    },
  ]),
}
