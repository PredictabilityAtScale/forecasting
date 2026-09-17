import type { CourseDefinition } from './course-types'
import { DEPENDENCY_MANAGEMENT_QUIZZES } from './course-quizzes'
import { DEPENDENCY_SURVEYS } from './course-surveys'

const imageRoot = '/images/courses/dependency-management'

export const DEPENDENCY_MANAGEMENT_COURSE: CourseDefinition = {
  slug: 'dependency-management',
  title: 'Dependency Management — Capture, Fix, and Avoid',
  shortTitle: 'Dependency Management',
  description:
    'A practical workshop for finding the dependencies that matter, solving their root causes, and improving the policies that create them.',
  image: '/images/71RnNqTuQByHazE9WxXl_gotdependencies_fo.png',
  chapters: [
    {
      title: 'Introduction',
      lessons: [
        {
          archiveId: '32103762',
          slug: 'course-overview-and-welcome',
          order: 1,
          title: 'Course Overview and Welcome',
          chapter: 'Introduction',
          summary:
            'Start with the learning outcomes, course structure, and a few tips for getting the most from the exercises.',
          bullets: [
            'Welcome you to the course.',
            'List the learning outcomes.',
            'Share tips to maximize your success with the material.',
          ],
          resources: [
            {
              label: 'Email Troy Magennis',
              href: 'mailto:troy.magennis@focusedobjective.com',
            },
          ],
        },
        {
          archiveId: '32104243',
          slug: 'dependency-issues-and-goals',
          order: 3,
          title: 'Your Dependency Issues and Goals',
          chapter: 'Introduction',
          summary:
            'Reflect on the dependency problems you want to solve and the outcome you want from the course.',
          hasVideo: false,
          surveyQuestions: DEPENDENCY_SURVEYS.goals,
          resources: [],
        },
      ],
    },
    {
      title: 'What are Dependencies?',
      lessons: [
        {
          archiveId: '32104629',
          slug: 'your-definition-of-dependency',
          order: 11,
          title: 'Your Current Definition of Dependency',
          chapter: 'What are Dependencies?',
          summary:
            'Record your starting definition before comparing it with the course model.',
          hasVideo: false,
          surveyQuestions: DEPENDENCY_SURVEYS.definition,
          resources: [],
        },
        {
          archiveId: '32106615',
          slug: 'what-are-dependencies',
          order: 12,
          title: 'Theory: What Are Dependencies?',
          chapter: 'What are Dependencies?',
          summary:
            'Establish a shared definition of dependencies, blockers, and dependency management.',
          bullets: [
            'Definitions of dependencies and blockers.',
            'A definition of dependency management.',
            'The four basic types of dependencies.',
          ],
          resources: [],
        },
        {
          archiveId: '36829241',
          slug: 'create-your-dependency-examples',
          order: 13,
          title: 'Exercise: Create Your Dependency Examples',
          chapter: 'What are Dependencies?',
          summary:
            'Create examples of each dependency type from your own experience for use in later exercises.',
          paragraphs: [
            'Create a few examples for each dependency type that you have experienced. You will use these examples later when prioritizing dependencies and finding fixes.',
          ],
          bullets: [
            'Create physical sticky notes, or',
            'Use a digital whiteboard such as Miro or Mural.',
          ],
          resources: [],
        },
        {
          archiveId: '32106766',
          slug: 'what-are-dependencies-quiz',
          order: 14,
          title: 'Debrief and Quiz: What Are Dependencies?',
          chapter: 'What are Dependencies?',
          summary:
            'Review the definition, common types, and purpose of dependency management.',
          hasVideo: false,
          quiz: DEPENDENCY_MANAGEMENT_QUIZZES.whatAreDependencies,
          resources: [],
        },
      ],
    },
    {
      title: 'Finding the Most Impactful Dependencies',
      lessons: [
        {
          archiveId: '36828854',
          slug: 'what-makes-dependencies-urgent',
          order: 16,
          title: 'What Makes Some Dependencies More Urgent?',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Capture your current thinking about which dependencies deserve attention first.',
          hasVideo: false,
          surveyQuestions: DEPENDENCY_SURVEYS.urgency,
          resources: [],
        },
        {
          archiveId: '32110157',
          slug: 'capturing-and-prioritizing-dependencies',
          order: 17,
          title: 'Theory: Capturing and Prioritizing Dependencies',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Use urgency, impact, and frequency to identify the dependencies most worth resolving.',
          bullets: [
            'How to measure dependency urgency.',
            'How to use the Dependency Prioritization Canvas.',
            'How to identify the top candidates for resolution.',
          ],
          resources: [],
        },
        {
          archiveId: '36829678',
          slug: 'dependency-prioritization-canvas',
          order: 18,
          title: 'Exercise: Dependency Prioritization Canvas',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Map dependency examples against impact and frequency to identify the most urgent candidates.',
          paragraphs: [
            'Using the examples from the prior exercise, map each dependency against impact and frequency. Draw the canvas on a physical or digital whiteboard and position the sticky notes relative to one another.',
          ],
          images: [
            {
              src: `${imageRoot}/dependency-prioritization-canvas.png`,
              alt: 'Dependency Prioritization Canvas plotting frequency against impact',
              caption:
                'Dependency Prioritization Canvas from the archived lesson.',
            },
          ],
          resources: [],
        },
        {
          archiveId: '32110477',
          slug: 'facilitating-dependency-prioritization',
          order: 19,
          title: 'Theory: Facilitating Dependency Prioritization',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Learn how to facilitate a group through discovering and prioritizing dependencies.',
          bullets: [
            'Questions that uncover dependencies from a group.',
            'How product and organization context suggests where to look.',
            'How to know when to stop looking for more dependencies.',
          ],
          resources: [],
        },
        {
          archiveId: '36829790',
          slug: 'facilitation-context-and-questions',
          order: 20,
          title: 'Exercise: Facilitation Context and Questions',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Design prompts that help teams surface dependencies earlier during planning.',
          paragraphs: [
            'Think about the context of your organization. What questions would you ask teams to help them understand dependencies earlier when planning?',
            'The goal is to prompt deeper thinking about what might block future work. Consider all four dependency types and create useful questions for each.',
          ],
          resources: [],
        },
        {
          archiveId: '32110504',
          slug: 'prioritizing-dependencies-quiz',
          order: 21,
          title: 'Debrief and Quiz: Prioritizing Dependencies',
          chapter: 'Finding the Most Impactful Dependencies',
          summary:
            'Review the factors used to identify the dependencies most worth addressing.',
          hasVideo: false,
          quiz: DEPENDENCY_MANAGEMENT_QUIZZES.prioritizing,
          resources: [],
        },
      ],
    },
    {
      title: 'Solving the Most Impactful Dependencies',
      lessons: [
        {
          archiveId: '36830148',
          slug: 'dependency-root-causes-reflection',
          order: 22,
          title: 'Root Causes of Dependencies in Your Organization',
          chapter: 'Solving the Most Impactful Dependencies',
          summary:
            'Identify the organizational or team conditions that create recurring dependencies.',
          hasVideo: false,
          surveyQuestions: DEPENDENCY_SURVEYS.rootCauses,
          resources: [],
        },
        {
          archiveId: '32110703',
          slug: 'solving-root-causes',
          order: 23,
          title: 'Theory: Solving the Root Causes of Dependencies',
          chapter: 'Solving the Most Impactful Dependencies',
          summary:
            'Move beyond coordination workarounds and design experiments that address root causes.',
          bullets: [
            'Facilitate understanding the root causes of dependencies.',
            'Define experiments that resolve those root causes.',
            'Measure the experiments to confirm whether they worked.',
          ],
          resources: [],
        },
        {
          archiveId: '38783972',
          slug: 'solve-a-dependency-canvas',
          order: 24,
          title: 'Exercise: Solve a Dependency Canvas',
          chapter: 'Solving the Most Impactful Dependencies',
          summary:
            'Use the canvas to propose both immediate communication improvements and longer-term process changes.',
          paragraphs: [
            'Use this canvas to solve a dependency you identified earlier in the workshop.',
          ],
          bullets: [
            'Create at least one experiment that improves communication.',
            'Create at least one longer-term process experiment that reduces dependency frequency or impact.',
          ],
          images: [
            {
              src: `${imageRoot}/solve-a-dependency-canvas.png`,
              alt: 'Solve a Dependency Canvas from the course exercise',
              caption: 'Solve a Dependency Canvas from the archived lesson.',
            },
          ],
          resources: [],
        },
        {
          archiveId: '32110740',
          slug: 'solving-dependencies-quiz',
          order: 25,
          title: 'Debrief and Quiz: Solving the Most Impactful Dependencies',
          chapter: 'Solving the Most Impactful Dependencies',
          summary:
            'Review how to choose dependency experiments and measure whether they worked.',
          hasVideo: false,
          quiz: DEPENDENCY_MANAGEMENT_QUIZZES.solving,
          resources: [],
        },
      ],
    },
    {
      title: 'Avoiding Dependencies with Better Management',
      lessons: [
        {
          archiveId: '32110774',
          slug: 'dependency-management-quick-wins',
          order: 26,
          title: 'Quick Wins for Dependency Management',
          chapter: 'Avoiding Dependencies with Better Management',
          summary:
            'Apply practical changes that make dependencies easier to see, coordinate, and reduce.',
          resources: [],
        },
        {
          archiveId: '32110786',
          slug: 'three-decisions-at-multiple-levels',
          order: 27,
          title: 'Theory: Three Decisions at Multiple Levels',
          chapter: 'Avoiding Dependencies with Better Management',
          summary:
            'Examine dependency-management decisions across portfolio and cross-team levels.',
          resources: [],
        },
        {
          archiveId: '38786552',
          slug: 'document-current-policies',
          order: 28,
          title: 'Exercise: Document Current Portfolio and Cross-Team Policies',
          chapter: 'Avoiding Dependencies with Better Management',
          summary:
            'Make current decision policies explicit before refining how portfolio and cross-team coordination works.',
          paragraphs: [
            'Document the current policies for the three decisions or headings at two levels: portfolio and cross-team coordination.',
            'Start by recording how these decisions are made today, then add and refine. Simply making the current process visible creates value.',
          ],
          images: [
            {
              src: `${imageRoot}/portfolio-policy-example.png`,
              alt: 'Example portfolio policy from the dependency-management exercise',
            },
            {
              src: `${imageRoot}/portfolio-policy-canvas.png`,
              alt: 'Portfolio policy canvas from the dependency-management exercise',
            },
            {
              src: `${imageRoot}/cross-team-policy-example.png`,
              alt: 'Example cross-team policy from the dependency-management exercise',
            },
            {
              src: `${imageRoot}/cross-team-policy-canvas.png`,
              alt: 'Cross-team coordination policy canvas from the dependency-management exercise',
            },
          ],
          resources: [],
        },
        {
          archiveId: '32110822',
          slug: 'chapter-five-debrief-quiz',
          order: 29,
          title: 'Chapter 5 Debrief and Quiz',
          chapter: 'Avoiding Dependencies with Better Management',
          summary:
            'The original draft quiz placeholder preserved exactly as it appeared in Thinkific.',
          hasVideo: false,
          quiz: DEPENDENCY_MANAGEMENT_QUIZZES.chapterFiveDraft,
          resources: [],
        },
      ],
    },
    {
      title: 'Quantifying the Impact of Dependencies',
      lessons: [
        {
          archiveId: '32110839',
          slug: 'cost-of-dependencies',
          order: 30,
          title: 'The Cost of Dependencies on Capacity and Predictability',
          chapter: 'Quantifying the Impact of Dependencies',
          summary:
            'See how dependencies consume capacity and make delivery outcomes less predictable.',
          resources: [],
        },
      ],
    },
    {
      title: 'Before You Go',
      lessons: [
        {
          archiveId: '32110863',
          slug: 'dependency-management-key-points-quiz',
          order: 33,
          title: 'Quiz on the Key Points',
          chapter: 'Before You Go',
          summary:
            'Check your understanding of the course’s core dependency-management ideas.',
          hasVideo: false,
          quiz: DEPENDENCY_MANAGEMENT_QUIZZES.keyPoints,
          resources: [],
        },
        {
          archiveId: '32110875',
          slug: 'course-feedback',
          order: 36,
          title: 'How Did We Do?',
          chapter: 'Before You Go',
          summary:
            'Reflect on your learning outcome and provide private feedback about the course.',
          hasVideo: false,
          surveyQuestions: DEPENDENCY_SURVEYS.feedback,
          resources: [],
        },
      ],
    },
    {
      title: 'Additional Video',
      lessons: [
        {
          archiveId: '33229791',
          slug: 'exercise-2-1',
          order: 37,
          title: 'Exercise 2.1',
          chapter: 'Additional Video',
          summary:
            'A preserved supplemental exercise from the original course.',
          resources: [],
        },
      ],
    },
  ],
}
