import { Link } from '@tanstack/react-router'
import { useEffect, useMemo, useState } from 'react'
import type {
  CourseDefinition,
  CourseLesson,
  CourseQuizLesson,
  CourseSurveyQuestion,
} from '#/data/course-types'
import { flattenCourseLessons } from '#/data/course-types'

export default function CoursePlayer({
  course,
  lessonSlug,
}: {
  course: CourseDefinition
  lessonSlug: string
}) {
  const lessons = useMemo(() => flattenCourseLessons(course), [course])
  const lesson =
    lessons.find((candidate) => candidate.slug === lessonSlug) ?? lessons.at(0)

  if (!lesson) return null

  return (
    <CoursePlayerContent course={course} lesson={lesson} lessons={lessons} />
  )
}

function CoursePlayerContent({
  course,
  lesson,
  lessons,
}: {
  course: CourseDefinition
  lesson: CourseLesson
  lessons: CourseLesson[]
}) {
  const progressStorageKey = `focused-objective:course-progress:${course.slug}`
  const lessonIndex = lessons.findIndex(
    (candidate) => candidate.slug === lesson.slug,
  )
  const previousLesson =
    lessonIndex > 0 ? lessons.at(lessonIndex - 1) : undefined
  const nextLesson =
    lessonIndex < lessons.length - 1 ? lessons.at(lessonIndex + 1) : undefined
  const [completedLessons, setCompletedLessons] = useState<string[]>([])
  const [progressLoaded, setProgressLoaded] = useState(false)

  useEffect(() => {
    try {
      const stored = window.localStorage.getItem(progressStorageKey)
      const parsed = stored ? JSON.parse(stored) : []
      if (Array.isArray(parsed)) {
        const validSlugs = new Set(lessons.map((item) => item.slug))
        setCompletedLessons(
          parsed.filter(
            (value): value is string =>
              typeof value === 'string' && validSlugs.has(value),
          ),
        )
      }
    } catch {
      setCompletedLessons([])
    } finally {
      setProgressLoaded(true)
    }
  }, [lessons, progressStorageKey])

  const completedSet = useMemo(
    () => new Set(completedLessons),
    [completedLessons],
  )
  const completionPercent = Math.round(
    (completedLessons.length / lessons.length) * 100,
  )
  const isComplete = completedSet.has(lesson.slug)
  const hasNotes = Boolean(
    lesson.note ||
    lesson.paragraphs?.length ||
    lesson.bullets?.length ||
    lesson.images?.length,
  )

  function toggleLessonComplete() {
    const next = isComplete
      ? completedLessons.filter((slug) => slug !== lesson.slug)
      : [...completedLessons, lesson.slug]

    setCompletedLessons(next)
    window.localStorage.setItem(progressStorageKey, JSON.stringify(next))
  }

  return (
    <main className="mx-auto max-w-[1440px] px-4 pb-16 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in relative overflow-hidden rounded-3xl">
        <div className="grid min-h-64 lg:grid-cols-[minmax(0,1fr)_420px]">
          <div className="relative z-10 flex flex-col justify-center px-6 py-10 sm:px-10 lg:py-12">
            <div className="mb-4 flex flex-wrap items-center gap-2">
              <Link
                to="/courses"
                className="rounded-full border border-[var(--chip-line)] bg-[var(--chip-bg)] px-3 py-1 text-xs font-bold uppercase tracking-[0.12em] text-[var(--palm)] no-underline transition hover:bg-[var(--link-bg-hover)]"
              >
                Free course library
              </Link>
              <span className="text-sm font-semibold text-[var(--sea-ink-soft)]">
                {lessons.length} lessons · {course.chapters.length} topics
              </span>
            </div>
            <h1 className="display-title m-0 max-w-4xl text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-5xl">
              {course.title}
            </h1>
            <p className="mb-0 mt-4 max-w-3xl text-base leading-relaxed text-[var(--sea-ink-soft)] sm:text-lg">
              {course.description}
            </p>
          </div>
          <div className="relative min-h-56 overflow-hidden border-t border-[var(--line)] lg:border-l lg:border-t-0">
            <img
              src={course.image}
              alt={`${course.shortTitle} course cover`}
              className="absolute inset-0 h-full w-full object-cover"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-[rgba(10,35,40,0.68)] via-transparent to-transparent" />
            <p className="absolute bottom-5 left-5 m-0 rounded-full bg-[rgba(10,35,40,0.78)] px-3 py-1.5 text-xs font-semibold text-white backdrop-blur-sm">
              By Troy Magennis
            </p>
          </div>
        </div>
      </section>

      <div className="mt-6 grid items-start gap-6 lg:grid-cols-[320px_minmax(0,1fr)]">
        <aside
          className="island-shell rounded-2xl p-4 lg:sticky lg:top-24"
          aria-label="Course topics"
        >
          <div className="rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] p-4">
            <div className="flex items-center justify-between gap-3 text-xs font-bold uppercase tracking-[0.12em] text-[var(--sea-ink-soft)]">
              <span>Your progress</span>
              <span>{progressLoaded ? `${completionPercent}%` : '—'}</span>
            </div>
            <div
              className="mt-3 h-2 overflow-hidden rounded-full bg-[var(--sand)]"
              role="progressbar"
              aria-valuemin={0}
              aria-valuemax={100}
              aria-valuenow={progressLoaded ? completionPercent : 0}
              aria-label="Course completion"
            >
              <div
                className="h-full rounded-full bg-[var(--lagoon)] transition-[width]"
                style={{ width: `${progressLoaded ? completionPercent : 0}%` }}
              />
            </div>
            <p className="mb-0 mt-2 text-xs text-[var(--sea-ink-soft)]">
              {completedLessons.length} of {lessons.length} lessons complete on
              this browser
            </p>
          </div>

          <nav className="mt-5 space-y-5">
            {course.chapters.map((chapter) => (
              <div key={chapter.title}>
                <p className="mb-2 px-2 text-[11px] font-bold uppercase tracking-[0.12em] text-[var(--sea-ink-soft)]">
                  {chapter.title}
                </p>
                <div className="space-y-1">
                  {chapter.lessons.map((item) => {
                    const active = item.slug === lesson.slug
                    const complete = completedSet.has(item.slug)
                    const itemNumber =
                      lessons.findIndex(
                        (candidate) => candidate.slug === item.slug,
                      ) + 1

                    return (
                      <Link
                        key={item.slug}
                        to="/courses/$courseSlug/$lessonSlug"
                        params={{
                          courseSlug: course.slug,
                          lessonSlug: item.slug,
                        }}
                        aria-current={active ? 'page' : undefined}
                        className={`group flex items-start gap-3 rounded-xl px-3 py-3 no-underline transition ${
                          active
                            ? 'bg-[rgba(79,184,178,0.18)] text-[var(--sea-ink)]'
                            : 'text-[var(--sea-ink-soft)] hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]'
                        }`}
                      >
                        <span
                          className={`mt-0.5 flex h-5 min-w-5 shrink-0 items-center justify-center rounded-full border px-1 text-[10px] font-bold ${
                            complete
                              ? 'border-[var(--lagoon-deep)] bg-[var(--lagoon-deep)] text-white'
                              : active
                                ? 'border-[var(--lagoon-deep)] text-[var(--lagoon-deep)]'
                                : 'border-[var(--line)] text-[var(--sea-ink-soft)]'
                          }`}
                          aria-label={
                            complete ? 'Completed' : `Lesson ${itemNumber}`
                          }
                        >
                          {complete ? '✓' : itemNumber}
                        </span>
                        <span className="text-sm font-semibold leading-snug">
                          {item.title}
                        </span>
                      </Link>
                    )
                  })}
                </div>
              </div>
            ))}
          </nav>
        </aside>

        <article className="island-shell min-w-0 rounded-2xl p-5 sm:p-8 lg:p-10">
          <header>
            <p className="island-kicker mb-2">
              Lesson {lessonIndex + 1} of {lessons.length} · {lesson.chapter}
            </p>
            <h2 className="display-title m-0 text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
              {lesson.title}
            </h2>
            <p className="mb-0 mt-3 max-w-3xl text-base leading-relaxed text-[var(--sea-ink-soft)]">
              {lesson.summary}
            </p>
          </header>

          {lesson.surveyQuestions?.length ? (
            <SurveyLesson
              courseSlug={course.slug}
              lessonSlug={lesson.slug}
              questions={lesson.surveyQuestions}
            />
          ) : lesson.quiz ? (
            <QuizLesson quiz={lesson.quiz} lessonSlug={lesson.slug} />
          ) : lesson.hasVideo === false ? (
            <div className="mt-7 flex min-h-48 flex-col items-center justify-center rounded-2xl border border-[var(--line)] bg-[var(--surface-strong)] px-6 text-center">
              <span className="flex h-14 w-14 items-center justify-center rounded-full border border-[var(--line)] bg-[var(--surface)] text-2xl">
                ◫
              </span>
              <h3 className="mb-0 mt-4 text-xl font-semibold text-[var(--sea-ink)]">
                Written lesson
              </h3>
              <p className="mb-0 mt-2 max-w-lg text-sm leading-relaxed text-[var(--sea-ink-soft)]">
                This lesson did not include a video in the original course
                archive.
              </p>
            </div>
          ) : (
            <div className="mt-7 overflow-hidden rounded-2xl border border-[var(--line)] bg-[linear-gradient(135deg,rgba(23,58,64,0.96),rgba(50,143,151,0.88))]">
              {lesson.youtubeId ? (
                <div className="aspect-video">
                  <iframe
                    className="h-full w-full"
                    src={`https://www.youtube-nocookie.com/embed/${lesson.youtubeId}`}
                    title={lesson.title}
                    loading="lazy"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                    allowFullScreen
                  />
                </div>
              ) : (
                <div className="flex aspect-video min-h-64 flex-col items-center justify-center px-6 text-center text-white">
                  <span className="flex h-16 w-16 items-center justify-center rounded-full border border-white/35 bg-white/10 text-2xl backdrop-blur-sm">
                    ▶
                  </span>
                  <h3 className="mb-0 mt-5 text-xl font-semibold">
                    Video publishing shortly
                  </h3>
                  <p className="mb-0 mt-2 max-w-lg text-sm leading-relaxed text-white/80">
                    This lesson has been preserved and its video is being
                    prepared for free viewing. The lesson notes and resources
                    are available below now.
                  </p>
                </div>
              )}
            </div>
          )}

          {lesson.surveyQuestions?.length || lesson.quiz ? null : (
            <section className="mt-8 border-t border-[var(--line)] pt-7">
              <h3 className="m-0 text-xl font-semibold text-[var(--sea-ink)]">
                Lesson notes
              </h3>
              {hasNotes ? (
                <div className="mt-3 max-w-4xl space-y-4 text-sm leading-7 text-[var(--sea-ink-soft)] sm:text-base">
                  {lesson.note ? <p className="m-0">{lesson.note}</p> : null}
                  {lesson.paragraphs?.map((paragraph) => (
                    <p key={paragraph} className="m-0">
                      {paragraph}
                    </p>
                  ))}
                  {lesson.bullets?.length ? (
                    <ul className="m-0 space-y-2 pl-6 marker:text-[var(--lagoon-deep)]">
                      {lesson.bullets.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  ) : null}
                </div>
              ) : (
                <p className="mb-0 mt-3 text-sm italic text-[var(--sea-ink-soft)]">
                  This archived video lesson did not include additional written
                  notes.
                </p>
              )}

              {lesson.images?.length ? (
                <div className="mt-6 grid gap-4 sm:grid-cols-2">
                  {lesson.images.map((image) => (
                    <figure
                      key={image.src}
                      className={`m-0 overflow-hidden rounded-2xl border border-[var(--line)] bg-white p-3 ${
                        image.width === 'full' ? 'sm:col-span-2' : ''
                      } ${image.width === 'compact' ? 'max-w-lg' : ''}`}
                    >
                      <img
                        src={image.src}
                        alt={image.alt}
                        loading="lazy"
                        className="h-auto w-full"
                      />
                      {image.caption ? (
                        <figcaption className="px-1 pb-1 pt-3 text-xs leading-relaxed text-slate-600">
                          {image.caption}
                        </figcaption>
                      ) : null}
                    </figure>
                  ))}
                </div>
              ) : null}
            </section>
          )}

          {lesson.resources.length > 0 ? (
            <section className="mt-7">
              <h3 className="m-0 text-xl font-semibold text-[var(--sea-ink)]">
                Resources
              </h3>
              <div className="mt-3 grid gap-3">
                {lesson.resources.map((resource) => (
                  <a
                    key={resource.href}
                    href={resource.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="flex items-center justify-between gap-4 rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] px-4 py-3 text-sm font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[var(--surface)]"
                  >
                    <span>{resource.label}</span>
                    <span aria-hidden="true">↗</span>
                  </a>
                ))}
              </div>

              {lesson.resources
                .filter((resource) => resource.embedUrl)
                .map((resource) => (
                  <div
                    key={resource.embedUrl}
                    className="mt-5 overflow-hidden rounded-2xl border border-[var(--line)] bg-white"
                  >
                    <div className="flex items-center justify-between gap-3 border-b border-slate-200 bg-slate-50 px-4 py-3">
                      <p className="m-0 text-sm font-semibold text-slate-700">
                        Interactive notebook
                      </p>
                      <a
                        href={resource.href}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-xs font-semibold text-slate-600 no-underline hover:text-slate-900"
                      >
                        Open full screen ↗
                      </a>
                    </div>
                    <iframe
                      src={resource.embedUrl}
                      title={resource.label}
                      className="w-full"
                      style={{ height: resource.embedHeight ?? 720 }}
                      loading="lazy"
                    />
                  </div>
                ))}
            </section>
          ) : null}

          <div className="mt-9 flex flex-col-reverse gap-3 border-t border-[var(--line)] pt-6 sm:flex-row sm:items-center sm:justify-between">
            <div>
              {previousLesson ? (
                <Link
                  to="/courses/$courseSlug/$lessonSlug"
                  params={{
                    courseSlug: course.slug,
                    lessonSlug: previousLesson.slug,
                  }}
                  className="inline-flex items-center gap-2 rounded-full px-4 py-2.5 text-sm font-semibold text-[var(--sea-ink-soft)] no-underline transition hover:bg-[var(--link-bg-hover)] hover:text-[var(--sea-ink)]"
                >
                  ← Previous lesson
                </Link>
              ) : null}
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <button
                type="button"
                onClick={toggleLessonComplete}
                className={`rounded-full border px-5 py-2.5 text-sm font-semibold transition ${
                  isComplete
                    ? 'border-[var(--lagoon-deep)] bg-[rgba(79,184,178,0.16)] text-[var(--lagoon-deep)]'
                    : 'border-[var(--line)] bg-[var(--surface-strong)] text-[var(--sea-ink)] hover:bg-[var(--surface)]'
                }`}
              >
                {isComplete ? '✓ Lesson complete' : 'Mark lesson complete'}
              </button>
              {nextLesson ? (
                <Link
                  to="/courses/$courseSlug/$lessonSlug"
                  params={{
                    courseSlug: course.slug,
                    lessonSlug: nextLesson.slug,
                  }}
                  className="inline-flex items-center justify-center gap-2 rounded-full bg-[var(--lagoon-deep)] px-5 py-2.5 text-sm font-semibold !text-white no-underline transition hover:-translate-y-0.5 hover:opacity-90"
                >
                  Next lesson →
                </Link>
              ) : (
                <Link
                  to="/courses"
                  className="inline-flex items-center justify-center gap-2 rounded-full bg-[var(--lagoon-deep)] px-5 py-2.5 text-sm font-semibold !text-white no-underline transition hover:-translate-y-0.5 hover:opacity-90"
                >
                  Browse courses →
                </Link>
              )}
            </div>
          </div>
        </article>
      </div>
    </main>
  )
}

function QuizLesson({
  quiz,
  lessonSlug,
}: {
  quiz: CourseQuizLesson
  lessonSlug: string
}) {
  const [answers, setAnswers] = useState<Partial<Record<string, string[]>>>({})
  const [checked, setChecked] = useState(false)

  useEffect(() => {
    setAnswers({})
    setChecked(false)
  }, [quiz.sourceLessonId])

  const answeredCount = quiz.questions.filter(
    (question) => (answers[question.id]?.length ?? 0) > 0,
  ).length
  const correctCount = quiz.questions.filter((question) => {
    const selected = new Set(answers[question.id] ?? [])
    return question.choices.every(
      (choice) => selected.has(choice.text) === choice.correct,
    )
  }).length

  function chooseAnswer(questionId: string, choice: string, multiple: boolean) {
    setAnswers((current) => {
      if (!multiple) return { ...current, [questionId]: [choice] }

      const selected = new Set(current[questionId] ?? [])
      if (selected.has(choice)) selected.delete(choice)
      else selected.add(choice)
      return { ...current, [questionId]: [...selected] }
    })
    setChecked(false)
  }

  return (
    <section className="mt-7 rounded-2xl border border-[var(--line)] bg-[var(--surface-strong)] p-5 sm:p-7">
      <div className="flex flex-col gap-2 border-b border-[var(--line)] pb-5 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="island-kicker m-0">Preserved quiz</p>
          <h3 className="mb-0 mt-2 text-2xl font-semibold text-[var(--sea-ink)]">
            Check your understanding
          </h3>
        </div>
        <span className="w-fit rounded-full border border-[var(--chip-line)] bg-[var(--chip-bg)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink-soft)]">
          {quiz.questions.length}{' '}
          {quiz.questions.length === 1 ? 'question' : 'questions'}
        </span>
      </div>

      {quiz.draft ? (
        <div className="mt-5 rounded-xl border border-amber-300 bg-amber-50 px-4 py-3 text-sm leading-relaxed text-amber-950">
          This lesson was still a draft placeholder in Thinkific. Its original
          placeholder wording is preserved below.
        </div>
      ) : (
        <p className="mb-0 mt-4 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
          These questions, correct answers, and explanations were recovered from
          the original Thinkific lesson. Nothing is submitted externally.
        </p>
      )}

      <div className="mt-7 space-y-7">
        {quiz.questions.map((question, index) => {
          const selected = new Set(answers[question.id] ?? [])
          const multiple = question.type === 'multiple-choice'

          return (
            <fieldset
              key={question.id}
              className="m-0 rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-4 sm:p-5"
            >
              <legend className="px-2 text-sm font-bold text-[var(--sea-ink)]">
                Question {index + 1}
                {multiple ? (
                  <span className="ml-2 font-normal text-[var(--sea-ink-soft)]">
                    Choose all that apply
                  </span>
                ) : null}
              </legend>
              <p className="mb-0 mt-1 whitespace-pre-line text-sm leading-7 text-[var(--sea-ink-soft)] sm:text-base">
                {question.prompt}
              </p>

              {question.image ? (
                <figure className="m-0 mt-4 overflow-hidden rounded-xl border border-[var(--line)] bg-white p-3">
                  <img
                    src={question.image.src}
                    alt={question.image.alt}
                    loading="lazy"
                    className="h-auto w-full"
                  />
                  {question.image.caption ? (
                    <figcaption className="px-1 pb-1 pt-3 text-xs leading-relaxed text-slate-600">
                      {question.image.caption}
                    </figcaption>
                  ) : null}
                </figure>
              ) : null}

              <div className="mt-4 grid gap-2">
                {question.choices.map((choice) => {
                  const isSelected = selected.has(choice.text)
                  const revealCorrect = checked && choice.correct
                  const revealIncorrect =
                    checked && isSelected && !choice.correct

                  return (
                    <label
                      key={choice.text}
                      className={`flex cursor-pointer items-start gap-3 rounded-xl border px-4 py-3 text-sm leading-relaxed transition ${
                        revealCorrect
                          ? 'border-emerald-500 bg-emerald-50 text-emerald-950'
                          : revealIncorrect
                            ? 'border-rose-400 bg-rose-50 text-rose-950'
                            : isSelected
                              ? 'border-[var(--lagoon-deep)] bg-[rgba(79,184,178,0.16)] text-[var(--sea-ink)]'
                              : 'border-[var(--line)] bg-[var(--surface-strong)] text-[var(--sea-ink-soft)] hover:border-[var(--lagoon-deep)]'
                      }`}
                    >
                      <input
                        type={multiple ? 'checkbox' : 'radio'}
                        name={`quiz-${lessonSlug}-${question.id}`}
                        checked={isSelected}
                        onChange={() =>
                          chooseAnswer(question.id, choice.text, multiple)
                        }
                        className="mt-1 accent-[var(--lagoon-deep)]"
                      />
                      <span className="flex-1">{choice.text}</span>
                      {revealCorrect ? (
                        <span className="font-bold" aria-label="Correct answer">
                          ✓
                        </span>
                      ) : revealIncorrect ? (
                        <span
                          className="font-bold"
                          aria-label="Incorrect answer"
                        >
                          ×
                        </span>
                      ) : null}
                    </label>
                  )
                })}
              </div>

              {checked && question.explanation ? (
                <div className="mt-4 rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] px-4 py-3 text-sm leading-7 text-[var(--sea-ink-soft)]">
                  <strong className="text-[var(--sea-ink)]">
                    Explanation:
                  </strong>{' '}
                  {question.explanation}
                </div>
              ) : null}
            </fieldset>
          )
        })}
      </div>

      <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:items-center">
        <button
          type="button"
          disabled={answeredCount !== quiz.questions.length}
          onClick={() => setChecked(true)}
          className="rounded-full bg-[var(--lagoon-deep)] px-5 py-2.5 text-sm font-semibold text-white transition enabled:hover:-translate-y-0.5 enabled:hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-45"
        >
          Check my answers
        </button>
        <p
          className="m-0 text-sm text-[var(--sea-ink-soft)]"
          aria-live="polite"
        >
          {checked
            ? `${correctCount} of ${quiz.questions.length} correct`
            : `${answeredCount} of ${quiz.questions.length} answered`}
        </p>
      </div>
    </section>
  )
}

function SurveyLesson({
  courseSlug,
  lessonSlug,
  questions,
}: {
  courseSlug: string
  lessonSlug: string
  questions: CourseSurveyQuestion[]
}) {
  const storageKey = `focused-objective:survey:${courseSlug}:${lessonSlug}`
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [loaded, setLoaded] = useState(false)

  useEffect(() => {
    setLoaded(false)
    try {
      const stored = window.localStorage.getItem(storageKey)
      const parsed = stored ? JSON.parse(stored) : {}
      setAnswers(
        parsed && typeof parsed === 'object'
          ? (parsed as Record<string, string>)
          : {},
      )
    } catch {
      setAnswers({})
    } finally {
      setLoaded(true)
    }
  }, [storageKey])

  function updateAnswer(questionId: string, value: string) {
    setAnswers((current) => {
      const next = { ...current, [questionId]: value }
      window.localStorage.setItem(storageKey, JSON.stringify(next))
      return next
    })
  }

  return (
    <section className="mt-7 rounded-2xl border border-[var(--line)] bg-[var(--surface-strong)] p-5 sm:p-7">
      <div className="flex flex-col gap-2 border-b border-[var(--line)] pb-5 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="island-kicker m-0">Preserved survey</p>
          <h3 className="mb-0 mt-2 text-2xl font-semibold text-[var(--sea-ink)]">
            Your private reflection
          </h3>
        </div>
        <span className="w-fit rounded-full border border-[var(--chip-line)] bg-[var(--chip-bg)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink-soft)]">
          Saved in this browser
        </span>
      </div>

      <p className="mb-0 mt-4 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
        These questions were recovered from the original Thinkific lesson. Your
        answers remain on this device and are not submitted anywhere.
      </p>

      <div className="mt-7 space-y-7">
        {questions.map((question, index) => {
          const answer = answers[question.id] ?? ''
          const inputName = `survey-${lessonSlug}-${question.id}`

          return (
            <fieldset
              key={question.id}
              className="m-0 rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-4 sm:p-5"
              disabled={!loaded}
            >
              <legend className="px-2 text-sm font-bold text-[var(--sea-ink)]">
                Question {index + 1}
                {question.optional ? (
                  <span className="ml-2 font-normal text-[var(--sea-ink-soft)]">
                    Optional
                  </span>
                ) : null}
              </legend>
              <p className="mb-0 mt-1 whitespace-pre-line text-sm leading-7 text-[var(--sea-ink-soft)] sm:text-base">
                {question.prompt}
              </p>

              {question.type === 'free-text' ? (
                <textarea
                  value={answer}
                  onChange={(event) =>
                    updateAnswer(question.id, event.target.value)
                  }
                  rows={5}
                  aria-label={`Answer to question ${index + 1}`}
                  className="mt-4 w-full resize-y rounded-xl border border-[var(--line)] bg-[var(--surface-strong)] px-4 py-3 text-sm leading-relaxed text-[var(--sea-ink)] outline-none transition focus:border-[var(--lagoon-deep)] focus:ring-2 focus:ring-[rgba(50,143,151,0.2)]"
                />
              ) : (
                <div
                  className={`mt-4 grid gap-2 ${
                    question.type === 'rating'
                      ? 'grid-cols-5 sm:grid-cols-10'
                      : 'sm:grid-cols-5'
                  }`}
                >
                  {question.choices?.map((choice) => (
                    <label
                      key={choice}
                      className={`flex cursor-pointer items-center justify-center rounded-xl border px-3 py-3 text-center text-sm font-semibold transition ${
                        answer === choice
                          ? 'border-[var(--lagoon-deep)] bg-[rgba(79,184,178,0.16)] text-[var(--sea-ink)]'
                          : 'border-[var(--line)] bg-[var(--surface-strong)] text-[var(--sea-ink-soft)] hover:border-[var(--lagoon-deep)]'
                      }`}
                    >
                      <input
                        type="radio"
                        name={inputName}
                        value={choice}
                        checked={answer === choice}
                        onChange={() => updateAnswer(question.id, choice)}
                        className="sr-only"
                      />
                      {choice}
                    </label>
                  ))}
                </div>
              )}
            </fieldset>
          )
        })}
      </div>
    </section>
  )
}
