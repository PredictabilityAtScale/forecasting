import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useState, useCallback, useMemo, useEffect } from 'react'
import { z } from 'zod'
import {
  computeCapabilityMatrix,
  SKILL_LEVELS,
  FUTURE_NEED_LEVELS,
  riskColor,
  levelColor,
} from '#/lib/capability-matrix'
import type {
  CapabilityResult,
  FutureNeed,
  SkillMeta,
} from '#/lib/capability-matrix'
import CopyLinkButton from '#/components/CopyLinkButton'

export const Route = createFileRoute('/capability-matrix')({
  validateSearch: (search) =>
    capabilitySearchSchema.parse({
      people: parseSearchPeople(search.people),
      skills: parseSearchSkills(search.skills),
      matrix: parseSearchMatrix(search.matrix),
    }),
  component: CapabilityMatrixPage,
})

const skillSearchSchema = z.object({
  name: z.string(),
  futureNeed: z.enum(FUTURE_NEED_LEVELS),
  trainingLeadMonths: z.number(),
})

const capabilitySearchSchema = z.object({
  people: z.array(z.string()).optional(),
  skills: z.array(skillSearchSchema).optional(),
  matrix: z
    .array(
      z.array(
        z
          .number()
          .int()
          .min(0)
          .max(SKILL_LEVELS.length - 1),
      ),
    )
    .optional(),
})

function parseSearchPeople(value: unknown): string[] | undefined {
  if (!Array.isArray(value)) return undefined
  const people = value.filter(
    (entry): entry is string => typeof entry === 'string',
  )
  return people.length > 0 ? people : undefined
}

function parseSearchSkills(value: unknown): SkillMeta[] | undefined {
  if (!Array.isArray(value)) return undefined
  const parsed = value
    .map((entry) => skillSearchSchema.safeParse(entry))
    .filter((entry) => entry.success)
    .map((entry) => entry.data)
  return parsed.length > 0 ? parsed : undefined
}

function parseSearchMatrix(value: unknown): number[][] | undefined {
  if (!Array.isArray(value)) return undefined
  const parsedRows = value
    .filter((row): row is unknown[] => Array.isArray(row))
    .map((row) =>
      row
        .map((cell) =>
          typeof cell === 'number'
            ? cell
            : typeof cell === 'string'
              ? Number(cell)
              : NaN,
        )
        .filter(
          (cell) =>
            Number.isInteger(cell) && cell >= 0 && cell < SKILL_LEVELS.length,
        ),
    )
    .filter((row) => row.length > 0)

  return parsedRows.length > 0 ? parsedRows : undefined
}

/* ── Sample data (matches spreadsheet) ───────────────────────────────── */

const INITIAL_PEOPLE = ['Person 1', 'Person 2', 'Team 1']

const INITIAL_SKILLS: SkillMeta[] = [
  { name: 'CSS', futureNeed: 'Moderate need (often)', trainingLeadMonths: 3 },
  {
    name: 'Javascript',
    futureNeed: 'Heavy need (always)',
    trainingLeadMonths: 2,
  },
  {
    name: 'DB Backup/Restore',
    futureNeed: 'Light need (occasional)',
    trainingLeadMonths: 0.5,
  },
]

// matrix[skillIndex][personIndex] = level (0-4)
const INITIAL_MATRIX: number[][] = [
  [0, 0, 2], // CSS: Person1=Know nothing, Person2=Know nothing, Team1=Tweak
  [4, 3, 0], // JS:  Person1=Teach, Person2=Create, Team1=Know nothing
  [1, 2, 3], // DB:  Person1=Run, Person2=Tweak, Team1=Create
]

function normalizeMatrix(
  matrix: number[][] | undefined,
  skillCount: number,
  peopleCount: number,
): number[][] | undefined {
  if (skillCount <= 0 || peopleCount <= 0) return undefined
  const safeMatrix = matrix ?? []
  return Array.from({ length: skillCount }, (_row, skillIdx) =>
    Array.from({ length: peopleCount }, (_column, personIdx) => {
      const level = safeMatrix[skillIdx]?.[personIdx]
      return Number.isInteger(level) &&
        level >= 0 &&
        level < SKILL_LEVELS.length
        ? level
        : 0
    }),
  )
}

function areStringArraysEqual(left: string[], right: string[]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function areSkillsEqual(left: SkillMeta[], right: SkillMeta[]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function areMatrixEqual(left: number[][], right: number[][]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

/* ── Page ────────────────────────────────────────────────────────────── */

function CapabilityMatrixPage() {
  const search = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })
  const [people, setPeople] = useState<string[]>(
    search.people ?? INITIAL_PEOPLE,
  )
  const [skills, setSkills] = useState<SkillMeta[]>(
    search.skills ?? INITIAL_SKILLS,
  )
  const [matrix, setMatrix] = useState<number[][]>(
    normalizeMatrix(
      search.matrix,
      search.skills?.length ?? INITIAL_SKILLS.length,
      search.people?.length ?? INITIAL_PEOPLE.length,
    ) ?? INITIAL_MATRIX,
  )

  useEffect(() => {
    const nextPeople = search.people ?? INITIAL_PEOPLE
    const nextSkills = search.skills ?? INITIAL_SKILLS
    const nextMatrix =
      normalizeMatrix(search.matrix, nextSkills.length, nextPeople.length) ??
      INITIAL_MATRIX

    setPeople((prev) =>
      areStringArraysEqual(prev, nextPeople) ? prev : nextPeople,
    )
    setSkills((prev) => (areSkillsEqual(prev, nextSkills) ? prev : nextSkills))
    setMatrix((prev) => (areMatrixEqual(prev, nextMatrix) ? prev : nextMatrix))
  }, [search])

  useEffect(() => {
    const nextSearch = {
      people,
      skills,
      matrix: normalizeMatrix(matrix, skills.length, people.length),
    }

    if (JSON.stringify(search) === JSON.stringify(nextSearch)) return
    void navigate({ search: nextSearch, replace: true })
  }, [matrix, navigate, people, search, skills])

  /* ── People management ─────────────────────────────────────── */
  const addPerson = useCallback(() => {
    setPeople((p) => [...p, `Person ${p.length + 1}`])
    setMatrix((m) => m.map((row) => [...row, 0]))
  }, [])

  const removePerson = useCallback((idx: number) => {
    setPeople((p) => p.filter((_, i) => i !== idx))
    setMatrix((m) => m.map((row) => row.filter((_, i) => i !== idx)))
  }, [])

  const renamePerson = useCallback((idx: number, name: string) => {
    setPeople((p) => p.map((n, i) => (i === idx ? name : n)))
  }, [])

  /* ── Skill management ──────────────────────────────────────── */
  const addSkill = useCallback(() => {
    setSkills((s) => [
      ...s,
      {
        name: `Skill ${s.length + 1}`,
        futureNeed: 'Not needed',
        trainingLeadMonths: 0,
      },
    ])
    setMatrix((m) => [...m, new Array(people.length).fill(0)])
  }, [people.length])

  const removeSkill = useCallback((idx: number) => {
    setSkills((s) => s.filter((_, i) => i !== idx))
    setMatrix((m) => m.filter((_, i) => i !== idx))
  }, [])

  const updateSkill = useCallback((idx: number, patch: Partial<SkillMeta>) => {
    setSkills((s) => s.map((sk, i) => (i === idx ? { ...sk, ...patch } : sk)))
  }, [])

  /* ── Matrix cell update ────────────────────────────────────── */
  const setLevel = useCallback(
    (skillIdx: number, personIdx: number, level: number) => {
      setMatrix((m) =>
        m.map((row, si) =>
          si === skillIdx
            ? row.map((v, pi) => (pi === personIdx ? level : v))
            : row,
        ),
      )
    },
    [],
  )

  /* ── Live result ────────────────────────────────────────────── */
  const result = useMemo(() => {
    const validSkills = skills.filter((s) => s.name.trim() !== '')
    if (people.length === 0 || validSkills.length === 0) return null
    const validMatrix = matrix.slice(0, validSkills.length)
    return computeCapabilityMatrix(people, validSkills, validMatrix)
  }, [people, skills, matrix])

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <div className="mb-2 flex items-start justify-between gap-4">
          <h1 className="display-title text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
            Capability Matrix
          </h1>
          <CopyLinkButton />
        </div>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Assess your team's skills to identify capability gaps and risk areas.
          Add people/teams and skills, then rate each person's proficiency
          level. The heatmap scorecard highlights where you're strong and where
          you need to invest.
        </p>
        <p className="mb-8 max-w-2xl text-xs text-[var(--sea-ink-soft)]">
          The current inputs are stored in the URL for sharing.
        </p>

        {/* ── Setup: People ──────────────────────────────────────── */}
        <div className="mb-6">
          <h2 className="field-legend mb-2">People / Teams</h2>
          <div className="flex flex-wrap gap-2">
            {people.map((p, i) => (
              <div key={i} className="flex items-center gap-1">
                <input
                  className="field-input w-32"
                  value={p}
                  onChange={(e) => renamePerson(i, e.target.value)}
                />
                <button
                  type="button"
                  onClick={() => removePerson(i)}
                  className="text-[var(--sea-ink-soft)] transition hover:text-red-500"
                  title="Remove"
                >
                  <XIcon />
                </button>
              </div>
            ))}
            <button
              type="button"
              onClick={addPerson}
              className="rounded-lg border border-dashed border-[var(--line)] px-3 py-1 text-sm font-medium text-[var(--sea-ink-soft)] transition hover:border-[var(--lagoon)] hover:text-[var(--lagoon)]"
            >
              + Add
            </button>
          </div>
        </div>

        {/* ── Setup: Skills ──────────────────────────────────────── */}
        <div className="mb-6">
          <h2 className="field-legend mb-2">Skills / Technology Expertise</h2>
          <div className="space-y-2">
            <div className="hidden items-center gap-2 text-[10px] font-semibold uppercase tracking-wider text-[var(--sea-ink-soft)] sm:flex">
              <span className="flex-[3]">Skill</span>
              <span className="flex-[2]">Future Need</span>
              <span className="w-20">Onboarding</span>
              <span className="w-6" />
            </div>
            {skills.map((sk, si) => (
              <div key={si} className="flex items-center gap-2">
                <input
                  className="field-input min-w-0 flex-[3]"
                  value={sk.name}
                  onChange={(e) => updateSkill(si, { name: e.target.value })}
                  placeholder="Skill name"
                />
                <select
                  className="field-input min-w-0 flex-[2] text-xs text-black dark:text-black"
                  value={sk.futureNeed}
                  onChange={(e) =>
                    updateSkill(si, {
                      futureNeed: e.target.value as FutureNeed,
                    })
                  }
                >
                  {FUTURE_NEED_LEVELS.map((f) => (
                    <option key={f} value={f}>
                      {f}
                    </option>
                  ))}
                </select>
                <div className="flex shrink-0 items-center gap-1">
                  <input
                    type="number"
                    min={0}
                    step={0.5}
                    className="field-input w-16 text-xs"
                    value={sk.trainingLeadMonths}
                    onChange={(e) =>
                      updateSkill(si, {
                        trainingLeadMonths: parseFloat(e.target.value) || 0,
                      })
                    }
                  />
                  <span className="text-[10px] text-[var(--sea-ink-soft)]">
                    mo.
                  </span>
                </div>
                <button
                  type="button"
                  onClick={() => removeSkill(si)}
                  className="shrink-0 text-[var(--sea-ink-soft)] transition hover:text-red-500"
                  title="Remove skill"
                >
                  <XIcon />
                </button>
              </div>
            ))}
            <button
              type="button"
              onClick={addSkill}
              className="rounded-lg border border-dashed border-[var(--line)] px-3 py-1 text-sm font-medium text-[var(--sea-ink-soft)] transition hover:border-[var(--lagoon)] hover:text-[var(--lagoon)]"
            >
              + Add Skill
            </button>
          </div>
          <p className="mt-2 text-xs italic text-[var(--sea-ink-soft)]">
            Advice: Pick skills that you need to deliver as a team. Think of
            skills that new hires might need to learn. You are planning skill
            development, not impressing everyone with all things people CAN do.
          </p>
        </div>

        {/* ── Assessment matrix ──────────────────────────────────── */}
        <div className="mb-6">
          <h2 className="field-legend mb-2">Assessment Matrix</h2>
          <div className="overflow-x-auto rounded-xl border border-[var(--line)]">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                  <th className="sticky left-0 z-10 bg-[var(--surface)] px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                    Skill
                  </th>
                  {people.map((p, pi) => (
                    <th
                      key={pi}
                      className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]"
                    >
                      <span className="block max-w-[80px] truncate">{p}</span>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {skills.map((sk, si) => (
                  <tr
                    key={si}
                    className="border-b border-[var(--line)] last:border-0"
                  >
                    <td className="sticky left-0 z-10 bg-[var(--bg-base)] px-3 py-1.5 font-medium text-[var(--sea-ink)]">
                      {sk.name || <span className="opacity-40">—</span>}
                    </td>
                    {people.map((_, pi) => (
                      <td key={pi} className="px-1 py-1 text-center">
                        <select
                          className={`w-full max-w-[90px] rounded border border-[var(--line)] px-1 py-0.5 text-[11px] font-medium outline-none focus:border-[var(--lagoon)] ${levelColor(matrix[si]?.[pi] ?? 0)} text-black dark:text-black [&>option]:bg-white [&>option]:text-black`}
                          value={matrix[si]?.[pi] ?? 0}
                          onChange={(e) =>
                            setLevel(si, pi, parseInt(e.target.value))
                          }
                        >
                          {SKILL_LEVELS.map((l) => (
                            <option key={l.value} value={l.value}>
                              {l.short}
                            </option>
                          ))}
                        </select>
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-2 flex flex-wrap gap-3 text-[10px] text-[var(--sea-ink-soft)]">
            {SKILL_LEVELS.map((l) => (
              <span key={l.value} className="flex items-center gap-1">
                <span
                  className={`inline-block h-3 w-3 rounded ${levelColor(l.value)}`}
                />
                {l.label}
              </span>
            ))}
          </div>
        </div>

        {/* ── Results ────────────────────────────────────────────── */}
        {result ? (
          <ResultsSection result={result} />
        ) : (
          <div className="flex items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-6 dark:border-amber-500/30 dark:bg-amber-950/30">
            <svg
              className="h-6 w-6 flex-shrink-0 text-amber-500"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
              <line x1="12" y1="9" x2="12" y2="13" />
              <line x1="12" y1="17" x2="12.01" y2="17" />
            </svg>
            <div>
              <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">
                Missing required inputs
              </p>
              <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                Add at least <strong>one person/team</strong> and{' '}
                <strong>one skill with a name</strong> to generate the
                scorecard.
              </p>
            </div>
          </div>
        )}
      </section>

      {/* ── Educational content ──────────────────────────────── */}
      <EducationalSection />
    </main>
  )
}

/* ── Results Section ─────────────────────────────────────────────────── */

function ResultsSection({ result }: { result: CapabilityResult }) {
  return (
    <div className="space-y-6">
      {/* Heatmap scorecard */}
      <div>
        <h3 className="mb-2 text-sm font-bold text-[var(--sea-ink)]">
          Heatmap Scorecard
        </h3>
        <div className="overflow-x-auto rounded-xl border border-[var(--line)]">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
                <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                  Skill
                </th>
                {result.people.map((p, i) => (
                  <th
                    key={i}
                    className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]"
                  >
                    <span className="block max-w-[80px] truncate">{p}</span>
                  </th>
                ))}
                <th className="border-l-2 border-[var(--line)] px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Teach &amp; Create
                </th>
                <th className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Do &amp; Maintain
                </th>
                <th className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Novice
                </th>
                <th className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Risk Score
                </th>
                <th className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Future Need
                </th>
                <th className="px-2 py-2 text-center font-semibold text-[var(--sea-ink-soft)]">
                  Lead Time
                </th>
              </tr>
            </thead>
            <tbody>
              {result.skills.map((sk, i) => (
                <tr
                  key={i}
                  className="border-b border-[var(--line)] last:border-0"
                >
                  <td className="px-3 py-2 font-medium text-[var(--sea-ink)]">
                    {sk.name}
                  </td>
                  {sk.personLevels.map((lvl, pi) => (
                    <td key={pi} className="px-1 py-1 text-center">
                      <span
                        className={`inline-block w-full rounded px-1.5 py-0.5 text-[10px] font-bold ${levelColor(lvl)}`}
                      >
                        {SKILL_LEVELS[lvl]?.short ?? '?'}
                      </span>
                    </td>
                  ))}
                  <td className="border-l-2 border-[var(--line)] px-2 py-2 text-center tabular-nums font-semibold text-sky-700 dark:text-sky-400">
                    {sk.teachAndCreate}
                  </td>
                  <td className="px-2 py-2 text-center tabular-nums font-semibold text-yellow-700 dark:text-yellow-400">
                    {sk.doAndMaintain}
                  </td>
                  <td className="px-2 py-2 text-center tabular-nums font-semibold text-orange-600 dark:text-orange-400">
                    {sk.novice}
                  </td>
                  <td className="px-2 py-2 text-center">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-[10px] font-bold tabular-nums ${riskColor(sk.riskScore, result.minRisk, result.maxRisk)}`}
                    >
                      {sk.riskScore}
                    </span>
                  </td>
                  <td className="px-2 py-2 text-center text-[10px] text-[var(--sea-ink-soft)]">
                    {sk.futureNeed === 'Not needed' ? '—' : sk.futureNeed}
                  </td>
                  <td className="px-2 py-2 text-center tabular-nums text-[var(--sea-ink-soft)]">
                    {sk.trainingLeadMonths > 0
                      ? `${sk.trainingLeadMonths} mo`
                      : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Risk legend */}
      <div className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-4">
        <p className="mb-2 text-xs font-bold text-[var(--sea-ink)]">
          Risk Score Legend
        </p>
        <p className="mb-2 text-xs text-[var(--sea-ink-soft)]">
          The risk score is the sum of all people's skill levels for that skill.
          Teachers = 4 pts, Creators = 3 pts, Bug fixers = 2 pts, Runners = 1
          pt, Know nothing = 0 pts. Lower score = higher risk.
        </p>
        <div className="flex flex-wrap gap-2 text-[10px]">
          <span className="flex items-center gap-1">
            <span className="inline-block h-3 w-6 rounded bg-red-100 dark:bg-red-900/40" />
            Highest risk
          </span>
          <span className="flex items-center gap-1">
            <span className="inline-block h-3 w-6 rounded bg-orange-100 dark:bg-orange-900/40" />
            High risk
          </span>
          <span className="flex items-center gap-1">
            <span className="inline-block h-3 w-6 rounded bg-yellow-100 dark:bg-yellow-900/40" />
            Moderate
          </span>
          <span className="flex items-center gap-1">
            <span className="inline-block h-3 w-6 rounded bg-sky-100 dark:bg-sky-900/40" />
            Healthy
          </span>
        </div>
      </div>
    </div>
  )
}

/* ── Educational — Planning & Stabilizing Teams ──────────────────────── */

function EducationalSection() {
  return (
    <>
      {/* Stage 1 */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">
          Stage 1 — Getting to an Effective &amp; Resilient Team
        </p>
        <p className="mb-4 text-sm text-[var(--sea-ink-soft)]">
          Stabilizing now and managing risk. Use the urgency matrix below to
          prioritize which skills need immediate attention.
        </p>

        {/* Urgency matrix */}
        <div className="mb-6 grid gap-6 sm:grid-cols-2">
          <UrgencyTable
            title="Doer Coverage"
            colHeader="Teachers"
            rowHeader="Doers"
          />
          <UrgencyTable
            title="Novice Pipeline"
            colHeader="Teachers"
            rowHeader="Novices"
            subtitle="If skill is growing in demand, prepare the bench strength"
          />
        </div>

        <div className="mb-4">
          <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
            Goals
          </h3>
          <ul className="m-0 list-inside list-disc space-y-1.5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
            <li>
              Have 2+ people who are Doer-level for each skill on the team. If
              creating new innovations, have at least 1 teacher for each skill.
            </li>
            <li>
              If a skill is in demand, have at least 1 (preferably 2) teachers
              on the team, and know who is willing to be a novice training to
              doer.
            </li>
            <li>
              Know what skills might be needed elsewhere in the company — your
              team members might be pulled off at short notice.
            </li>
            <li>
              Know what skills might be needed to fix incoming defects or
              production issues when rolling to customer usage.
            </li>
            <li>
              Know how long (and plan to reduce) the onboarding time from novice
              to doer levels, prioritized by the skills most anticipated in need
              for the future.
            </li>
            <li>
              It's <em>not</em> a goal to have everyone at Teacher level for
              every skill. Your goal is to have a resilient team given unplanned
              disruptions and future feature demands.
            </li>
          </ul>
        </div>

        <div>
          <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
            Key Questions
          </h3>
          <ul className="m-0 list-inside list-disc space-y-1.5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
            <li>
              How do I protect the skill sets where the team is at bare minimum
              (urgency &gt; 5)?
            </li>
            <li>Will this skill be needed more in the future?</li>
            <li>
              How many people are needed to maintain the rate of production
              support?
            </li>
            <li>
              Is support for this skill needed 24/7 or just during normal hours?
            </li>
            <li>
              Is there a Teacher-level person in each location, or at least
              within the same timezone if a Doer has questions?
            </li>
            <li>
              Will this skill be needed by other important projects if they get
              into difficulty?
            </li>
            <li>
              How long does it take a teacher to train a novice to Doer level?
            </li>
            <li>
              How long as a Doer does it take to become a Teacher? Can this be
              accelerated?
            </li>
            <li>
              How much effort does it take a teacher to create a doer from a
              novice — do I lose the Teacher completely?
            </li>
            <li>
              Do we need teacher-level on-staff, or can we obtain training from
              an external consultant?
            </li>
            <li>
              Is this skill growing or decreasing in the hiring community?
            </li>
            <li>
              What skills might be needed in higher numbers for stabilization or
              initial production support? (Where will the bug load come from?)
            </li>
            <li>How can onboarding be accelerated for this skill?</li>
          </ul>
        </div>
      </section>

      {/* Stage 2 */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">
          Stage 2 — Growing a Team to Split, Splitting &amp; Restabilizing
        </p>

        <div className="mb-6">
          <UrgencyTable
            title="Bench Readiness"
            colHeader="Doers"
            rowHeader="Novices"
            subtitle="How many novices do we have ready to fill needed skills?"
          />
        </div>

        <div className="mb-4">
          <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
            Goals
          </h3>
          <ul className="m-0 list-inside list-disc space-y-1.5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
            <li>
              Build the bench of Novices who can become Doers, and promote
              senior Doers to Teachers.
            </li>
            <li>
              Grow a team to have 1 Teacher-level for each skill, and 1
              Doer-level ready to become a Teacher.
            </li>
            <li>
              Grow each skill to have at least 3 Doers in all needed skill
              areas, even if you don't need that many. You need to be plump on
              skills to split!
            </li>
            <li>
              Split the teams and revisit Stage 1 — Getting to a Stable &amp;
              Resilient Team as soon as possible.
            </li>
          </ul>
        </div>

        <div>
          <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
            Key Questions
          </h3>
          <ul className="m-0 list-inside list-disc space-y-1.5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
            <li>
              Who is willing to learn an in-demand skill (become novices)?
            </li>
            <li>
              Which Doers are candidates to become Teacher-level for a skill?
            </li>
            <li>
              Can I loan a Teacher from another team to upskill my Doers or
              mentor a new Teacher after splitting the team?
            </li>
            <li>Is one location better than another to split?</li>
          </ul>
        </div>
      </section>

      {/* How it works */}
      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">How It Works</p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Skill Levels
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Each person is rated from "Know nothing" (0) to "Can teach others"
              (4). The level determines both the heatmap color and the points
              that contribute to the risk score.
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Risk Score
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              The risk score is the sum of all people's levels for a skill.
              Lower scores mean fewer capable people — higher organizational
              risk. The heatmap colors range from red (highest risk) to blue
              (healthy).
            </p>
          </div>
          <div>
            <h3 className="mb-2 text-base font-semibold text-[var(--sea-ink)]">
              Future Planning
            </h3>
            <p className="m-0 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Pair the risk score with anticipated future need and training lead
              time. A low-risk skill that's "Not needed" may be fine, but a
              low-risk skill with "Critical" need demands immediate action.
            </p>
          </div>
        </div>
      </section>
    </>
  )
}

/* ── Urgency Table (from spreadsheet) ────────────────────────────────── */

function UrgencyTable({
  title,
  colHeader,
  rowHeader,
  subtitle,
}: {
  title: string
  colHeader: string
  rowHeader: string
  subtitle?: string
}) {
  // 3×3 urgency grid from spreadsheet
  // Rows = 0, 1, 2+ of rowHeader; Cols = 0, 1, 2+ of colHeader
  const grid = [
    [9, 7, 3],
    [8, 5, 2],
    [6, 4, 1],
  ]

  const urgencyColor = (v: number) => {
    if (v >= 8)
      return 'bg-red-200 text-red-900 dark:bg-red-900/50 dark:text-red-200'
    if (v >= 6)
      return 'bg-orange-200 text-orange-900 dark:bg-orange-900/50 dark:text-orange-200'
    if (v >= 4)
      return 'bg-yellow-200 text-yellow-900 dark:bg-yellow-900/40 dark:text-yellow-200'
    if (v >= 2)
      return 'bg-sky-200 text-sky-900 dark:bg-sky-900/40 dark:text-sky-200'
    return 'bg-blue-200 text-blue-900 dark:bg-blue-900/40 dark:text-blue-200'
  }

  return (
    <div>
      <p className="mb-1 text-xs font-bold text-[var(--sea-ink)]">{title}</p>
      {subtitle && (
        <p className="mb-2 text-[10px] italic text-[var(--sea-ink-soft)]">
          {subtitle}
        </p>
      )}
      <div className="overflow-x-auto rounded-lg border border-[var(--line)]">
        <table className="w-full text-xs">
          <thead>
            <tr className="border-b border-[var(--line)] bg-[var(--surface)]">
              <th className="px-2 py-1.5 text-left text-[var(--sea-ink-soft)]">
                <span className="block text-[9px] uppercase tracking-wider opacity-60">
                  {rowHeader} ↓ / {colHeader} →
                </span>
              </th>
              <th className="min-w-[40px] px-2 py-1.5 text-center font-semibold text-[var(--sea-ink-soft)]">
                0
              </th>
              <th className="min-w-[40px] px-2 py-1.5 text-center font-semibold text-[var(--sea-ink-soft)]">
                1
              </th>
              <th className="min-w-[40px] px-2 py-1.5 text-center font-semibold text-[var(--sea-ink-soft)]">
                2+
              </th>
            </tr>
          </thead>
          <tbody>
            {['0', '1', '2+'].map((rowLabel, ri) => (
              <tr
                key={ri}
                className="border-b border-[var(--line)] last:border-0"
              >
                <td className="px-2 py-1.5 font-semibold text-[var(--sea-ink-soft)]">
                  {rowLabel}
                </td>
                {grid[ri].map((v, ci) => (
                  <td key={ci} className="px-1 py-1 text-center">
                    <span
                      className={`inline-block w-8 rounded py-0.5 text-[11px] font-bold ${urgencyColor(v)}`}
                    >
                      {v}
                    </span>
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

/* ── Tiny icon ───────────────────────────────────────────────────────── */

function XIcon() {
  return (
    <svg
      className="h-4 w-4"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <line x1="18" y1="6" x2="6" y2="18" />
      <line x1="6" y1="6" x2="18" y2="18" />
    </svg>
  )
}
