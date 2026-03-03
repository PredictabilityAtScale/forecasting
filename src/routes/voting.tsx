import { createFileRoute } from '@tanstack/react-router'
import { useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import StatCard from '#/components/StatCard'
import {
  computeVoting,
  createVoteTally,
  getInvalidVoteReasons,
  rankOrderCentroid,
  type VoteResultGroup,
} from '#/lib/voting'

export const Route = createFileRoute('/voting')({
  component: VotingPage,
})

type VotingMethod = 'plurality' | 'borda' | 'schulze'

interface EditableBallot {
  id: number
  count: string
  preferenceOrder: string
}

interface VotingReference {
  title: string
  href: string
  summary: string
  strengths: string[]
  weaknesses: string[]
}

const EXAMPLE_BALLOTS: EditableBallot[] = [
  { id: 1, count: '7', preferenceOrder: 'BACD' },
  { id: 2, count: '1', preferenceOrder: 'ACDB' },
  { id: 3, count: '2', preferenceOrder: 'CADB' },
  { id: 4, count: '2', preferenceOrder: 'DACB' },
]

const VOTING_REFERENCES: VotingReference[] = [
  {
    title: 'Plurality voting',
    href: 'https://en.wikipedia.org/wiki/Plurality_voting',
    summary:
      'Plurality elects the candidate with more votes than any other candidate, even when that total is not a majority. It is the simplest of the three methods here because each ballot effectively contributes only to its top-ranked option.',
    strengths: [
      'Very simple ballot design and counting process.',
      'Easy to explain to voters and stakeholders.',
      'Fast to tally and operationally cheap to administer.',
    ],
    weaknesses: [
      'A candidate can win without majority support.',
      'Wikipedia highlights wasted votes, tactical voting, and spoiler effects as common issues.',
      'Lower-ranked preferences are ignored, so consensus candidates can lose.',
    ],
  },
  {
    title: 'Borda count',
    href: 'https://en.wikipedia.org/wiki/Borda_count',
    summary:
      'Borda count is a positional ranked system: candidates receive points based on how many options they are ranked above, and the highest total wins. It tends to reward broad acceptability instead of only first-choice intensity.',
    strengths: [
      'Uses the full ranking instead of only the top choice.',
      'Often favors broadly acceptable candidates over polarizing ones.',
      'Straightforward point-based calculation once rankings are collected.',
    ],
    weaknesses: [
      'Wikipedia notes it is easy to manipulate strategically.',
      'Results can be sensitive to nomination effects and similar candidates on the ballot.',
      'A majority-preferred candidate is not guaranteed to win.',
    ],
  },
  {
    title: 'Schulze method',
    href: 'https://en.wikipedia.org/wiki/Schulze_method',
    summary:
      'The Schulze method is a Condorcet completion rule that compares candidates pairwise and resolves cycles using strongest paths, also called beatpaths. If there is a candidate who would beat every other candidate head-to-head, Schulze will elect that candidate.',
    strengths: [
      'Condorcet-consistent when a head-to-head winner exists.',
      'Wikipedia lists strong theoretical properties including monotonicity, clone independence, and the Smith criterion.',
      'Uses pairwise comparisons, which usually aligns better with majority preferences in ranked elections.',
    ],
    weaknesses: [
      'Harder to explain and audit informally than plurality or Borda.',
      'Requires pairwise matrix and path-strength calculations, which increases implementation complexity.',
      'Tie handling and interpretation of equal rankings add design choices that need to be made explicitly.',
    ],
  },
]

function VotingPage() {
  const [ballots, setBallots] = useState<EditableBallot[]>(EXAMPLE_BALLOTS)
  const [nextId, setNextId] = useState(5)
  const [method, setMethod] = useState<VotingMethod>('schulze')

  const ballotRows = useMemo(
    () =>
      ballots.map((ballot) => {
        const vote = createVoteTally({
          count: Number.parseInt(ballot.count, 10) || 0,
          preferenceOrder: ballot.preferenceOrder,
        })
        const invalidReasons = getInvalidVoteReasons(vote)

        return {
          ...ballot,
          vote,
          invalidReasons,
          valid: invalidReasons.length === 0,
        }
      }),
    [ballots],
  )

  const validVotes = useMemo(
    () => ballotRows.filter((row) => row.valid).map((row) => row.vote),
    [ballotRows],
  )

  const computation = useMemo(
    () => (validVotes.length > 0 ? computeVoting(validVotes) : null),
    [validVotes],
  )

  const activeResults = computation
    ? method === 'plurality'
      ? computation.plurality
      : method === 'borda'
        ? computation.borda
        : computation.schulze
    : []

  function updateBallot(id: number, patch: Partial<EditableBallot>) {
    setBallots((current) =>
      current.map((ballot) => (ballot.id === id ? { ...ballot, ...patch } : ballot)),
    )
  }

  function addBallot() {
    setBallots((current) => [
      ...current,
      { id: nextId, count: '1', preferenceOrder: '' },
    ])
    setNextId((current) => current + 1)
  }

  function removeBallot(id: number) {
    setBallots((current) => current.filter((ballot) => ballot.id !== id))
  }

  function loadExample() {
    setBallots(EXAMPLE_BALLOTS)
    setNextId(5)
  }

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="island-kicker mb-2">Voting</p>
            <h1 className="display-title text-3xl font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
              Score ranked ballots with plurality, Borda, and Schulze
            </h1>
            <p className="mt-3 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Add ballots with a count and ranked candidate order. Use plain
              sequences like <code>BACD</code> or explicit ties / ranking gaps
              like <code>A=B&gt;C=D</code>. Invalid ballots stay visible but are
              excluded from scoring.
            </p>
          </div>
        </div>

        <div className="mt-8 grid gap-8 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
          <section className="space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <h2 className="text-base font-semibold text-[var(--sea-ink)]">
                  Ballots
                </h2>
                <p className="text-xs text-[var(--sea-ink-soft)]">
                  Add weighted ranked ballots before reviewing the score output.
                </p>
              </div>
              <div className="flex flex-wrap gap-3">
                <button
                  className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-5 py-2.5 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
                  type="button"
                  onClick={loadExample}
                >
                  Load example
                </button>
                <button
                  className="rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-5 py-2.5 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
                  type="button"
                  onClick={addBallot}
                >
                  Add ballot
                </button>
              </div>
            </div>

            <div className="overflow-hidden rounded-2xl border border-[var(--line)]">
              <table className="w-full text-sm">
                <thead className="bg-[var(--surface)]">
                  <tr className="border-b border-[var(--line)]">
                    <th className="px-4 py-3 text-left font-semibold text-[var(--sea-ink-soft)]">
                      Count
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-[var(--sea-ink-soft)]">
                      Preference order
                    </th>
                    <th className="px-4 py-3 text-right font-semibold text-[var(--sea-ink-soft)]">
                      Action
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {ballotRows.map((row) => (
                    <tr
                      key={row.id}
                      className="border-b border-[var(--line)] align-top last:border-0"
                    >
                      <td className="px-4 py-3">
                        <input
                          className="field-input w-24"
                          inputMode="numeric"
                          value={row.count}
                          onChange={(event) =>
                            updateBallot(row.id, { count: event.target.value })
                          }
                        />
                      </td>
                      <td className="px-4 py-3">
                        <input
                          className="field-input font-mono uppercase"
                          placeholder="BACD or A=B>C=D"
                          value={row.preferenceOrder}
                          onChange={(event) =>
                            updateBallot(row.id, {
                              preferenceOrder: event.target.value.toUpperCase(),
                            })
                          }
                        />
                        {row.invalidReasons.length > 0 && (
                          <div className="mt-2 space-y-1">
                            {row.invalidReasons.map((reason) => (
                              <p
                                key={`${row.id}-${reason}`}
                                className="text-xs text-amber-700 dark:text-amber-300"
                              >
                                {reason}
                              </p>
                            ))}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <button
                          className="rounded-full border border-[var(--line)] px-3 py-1 text-xs font-semibold text-[var(--sea-ink-soft)] transition hover:border-[var(--lagoon)] hover:text-[var(--sea-ink)]"
                          type="button"
                          onClick={() => removeBallot(row.id)}
                          disabled={ballots.length === 1}
                        >
                          Remove
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              <StatCard label="Ballots entered" value={ballots.length} />
              <StatCard
                label="Valid ballots"
                value={ballotRows.filter((row) => row.valid).length}
              />
              <StatCard
                label="Weighted votes"
                value={validVotes.reduce((sum, vote) => sum + vote.count, 0)}
              />
            </div>

            <section className="rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5">
              <p className="island-kicker mb-3">Input notes</p>
              <div className="grid gap-4 sm:grid-cols-3">
                <InfoCard
                  title="Simple ranking"
                  body="ABCD means A over B over C over D."
                />
                <InfoCard
                  title="Ties"
                  body="A=B>C=D means A tied with B, both ahead of C tied with D."
                />
                <InfoCard
                  title="Skipped candidates"
                  body="Missing candidates are treated as being below listed candidates in pairwise comparisons."
                />
              </div>
            </section>
          </section>

          <section className="space-y-5">
            {computation ? (
              <>
                <div className="grid gap-3 sm:grid-cols-3">
                  <MethodCard
                    title="Plurality"
                    summary={formatGroupSummary(computation.plurality)}
                    active={method === 'plurality'}
                    onClick={() => setMethod('plurality')}
                  />
                  <MethodCard
                    title="Borda"
                    summary={formatGroupSummary(computation.borda)}
                    active={method === 'borda'}
                    onClick={() => setMethod('borda')}
                  />
                  <MethodCard
                    title="Schulze"
                    summary={formatGroupSummary(computation.schulze)}
                    active={method === 'schulze'}
                    onClick={() => setMethod('schulze')}
                  />
                </div>

                <section className="rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5">
                  <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
                    <div>
                      <p className="island-kicker mb-2">Results</p>
                      <h2 className="text-xl font-semibold text-[var(--sea-ink)]">
                        {methodLabel(method)}
                      </h2>
                    </div>
                    <p className="text-xs text-[var(--sea-ink-soft)]">
                      Candidates: {computation.candidates.join(', ')}
                    </p>
                  </div>

                  <div className="mt-4 space-y-3">
                    {activeResults.map((group) => (
                      <div
                        key={`${method}-${group.rank}-${group.score}`}
                        className="rounded-2xl border border-[var(--line)] bg-white/70 p-4 dark:bg-black/10"
                      >
                        <div className="flex items-center justify-between gap-4">
                          <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--sea-ink-soft)]">
                              {method === 'schulze'
                                ? `Rank ${group.rank}`
                                : `Score ${group.score}`}
                            </p>
                            <p className="mt-1 text-lg font-semibold text-[var(--sea-ink)]">
                              {group.candidates.map((candidate) => candidate.candidate).join(' = ')}
                            </p>
                          </div>
                          {method === 'schulze' && (
                            <p className="text-sm text-[var(--sea-ink-soft)]">
                              Pairwise wins:{' '}
                              {group.candidates.map((candidate) => candidate.score).join(', ')}
                            </p>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </section>

                <section className="grid gap-5 lg:grid-cols-2">
                  <MatrixCard
                    title="Ballot summary"
                    content={
                      <div className="overflow-x-auto">
                        <table className="w-full text-xs">
                          <thead>
                            <tr className="border-b border-[var(--line)]">
                              <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                                Ballot
                              </th>
                              <th className="px-3 py-2 text-right font-semibold text-[var(--sea-ink-soft)]">
                                Count
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            {computation.ballotSummary.map((row) => (
                              <tr
                                key={row.preferenceOrder}
                                className="border-b border-[var(--line)] last:border-0"
                              >
                                <td className="px-3 py-2 font-mono text-[var(--sea-ink)]">
                                  {row.preferenceOrder}
                                </td>
                                <td className="px-3 py-2 text-right tabular-nums text-[var(--sea-ink)]">
                                  {row.count}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    }
                  />

                  <MatrixCard
                    title="Rank-order centroid"
                    content={
                      <div className="space-y-3">
                        {computation.candidates.map((candidate, index) => (
                          <div
                            key={candidate}
                            className="flex items-center justify-between rounded-xl border border-[var(--line)] px-3 py-2"
                          >
                            <span className="font-medium text-[var(--sea-ink)]">
                              Position {index + 1}
                            </span>
                            <span className="tabular-nums text-[var(--sea-ink-soft)]">
                              {rankOrderCentroid(
                                computation.candidates.length,
                                index + 1,
                              ).toFixed(4)}
                            </span>
                          </div>
                        ))}
                      </div>
                    }
                  />
                </section>

                <section className="grid gap-5 lg:grid-cols-2">
                  <MatrixTable
                    title="Pairwise wins"
                    candidates={computation.candidates}
                    matrix={computation.pairWins}
                  />
                  <MatrixTable
                    title="Schulze path strengths"
                    candidates={computation.candidates}
                    matrix={computation.pathStrengths}
                  />
                </section>
              </>
            ) : (
              <div className="rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-6 dark:border-amber-500/30 dark:bg-amber-950/30">
                <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">
                  No valid ballots yet
                </p>
                <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                  Add at least one valid ballot with a positive count and at
                  least one candidate to generate results.
                </p>
              </div>
            )}
          </section>
        </div>
      </section>

      <section className="island-shell mt-8 rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-3">References</p>
        <h2 className="text-2xl font-semibold text-[var(--sea-ink)] sm:text-3xl">
          Voting system summaries, strengths, and weaknesses
        </h2>
        <p className="mt-2 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
          These notes summarize the corresponding Wikipedia pages for the three
          methods implemented above and call out the tradeoffs most relevant to
          this tool.
        </p>

        <div className="mt-6 grid gap-5">
          {VOTING_REFERENCES.map((reference) => (
            <article
              key={reference.title}
              className="rounded-[1.75rem] border border-[var(--line)] bg-[var(--surface)] p-5 sm:p-6"
            >
              <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <h3 className="text-xl font-semibold text-[var(--sea-ink)]">
                    {reference.title}
                  </h3>
                  <p className="mt-2 max-w-3xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
                    {reference.summary}
                  </p>
                </div>
                <a
                  href={reference.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1 rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-xs font-semibold text-[var(--lagoon-deep)] no-underline transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)]"
                >
                  Wikipedia
                  <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor">
                    <path d="M4.5 2a.5.5 0 0 0 0 1h6.793L2.146 12.146a.5.5 0 0 0 .708.708L12 3.707V10.5a.5.5 0 0 0 1 0v-9a.5.5 0 0 0-.5-.5h-8Z" />
                  </svg>
                </a>
              </div>

              <div className="mt-5 grid gap-5 lg:grid-cols-2">
                <div className="rounded-2xl border border-emerald-200/70 bg-emerald-50/50 p-4 dark:border-emerald-900/60 dark:bg-emerald-950/20">
                  <p className="text-sm font-semibold text-emerald-800 dark:text-emerald-300">
                    Strengths
                  </p>
                  <ul className="mt-3 space-y-2 pl-5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
                    {reference.strengths.map((item) => (
                      <li key={item}>{item}</li>
                    ))}
                  </ul>
                </div>

                <div className="rounded-2xl border border-rose-200/70 bg-rose-50/50 p-4 dark:border-rose-900/60 dark:bg-rose-950/20">
                  <p className="text-sm font-semibold text-rose-800 dark:text-rose-300">
                    Weaknesses
                  </p>
                  <ul className="mt-3 space-y-2 pl-5 text-sm leading-relaxed text-[var(--sea-ink-soft)]">
                    {reference.weaknesses.map((item) => (
                      <li key={item}>{item}</li>
                    ))}
                  </ul>
                </div>
              </div>
            </article>
          ))}
        </div>
      </section>
    </main>
  )
}

function MethodCard({
  title,
  summary,
  active,
  onClick,
}: {
  title: string
  summary: string
  active: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-2xl border p-4 text-left transition ${
        active
          ? 'border-[var(--lagoon)] bg-[rgba(79,184,178,0.12)]'
          : 'border-[var(--line)] bg-[var(--surface)] hover:border-[var(--lagoon)]/60'
      }`}
    >
      <p className="text-sm font-semibold text-[var(--sea-ink)]">{title}</p>
      <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">{summary}</p>
    </button>
  )
}

function InfoCard({ title, body }: { title: string; body: string }) {
  return (
    <div className="rounded-2xl border border-[var(--line)] bg-white/70 p-4 dark:bg-black/10">
      <h3 className="text-sm font-semibold text-[var(--sea-ink)]">{title}</h3>
      <p className="mt-2 text-xs leading-relaxed text-[var(--sea-ink-soft)]">
        {body}
      </p>
    </div>
  )
}

function MatrixCard({
  title,
  content,
}: {
  title: string
  content: ReactNode
}) {
  return (
    <section className="rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5">
      <h3 className="text-base font-semibold text-[var(--sea-ink)]">{title}</h3>
      <div className="mt-4">{content}</div>
    </section>
  )
}

function MatrixTable({
  title,
  candidates,
  matrix,
}: {
  title: string
  candidates: string[]
  matrix: number[][]
}) {
  return (
    <section className="rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-5">
      <h3 className="text-base font-semibold text-[var(--sea-ink)]">{title}</h3>
      <div className="mt-4 overflow-x-auto">
        <table className="w-full text-xs">
          <thead>
            <tr className="border-b border-[var(--line)]">
              <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink-soft)]">
                Candidate
              </th>
              {candidates.map((candidate) => (
                <th
                  key={`${title}-${candidate}`}
                  className="px-3 py-2 text-right font-semibold text-[var(--sea-ink-soft)]"
                >
                  {candidate}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {matrix.map((row, rowIndex) => (
              <tr
                key={`${title}-${candidates[rowIndex]}`}
                className="border-b border-[var(--line)] last:border-0"
              >
                <th className="px-3 py-2 text-left font-semibold text-[var(--sea-ink)]">
                  {candidates[rowIndex]}
                </th>
                {row.map((value, columnIndex) => (
                  <td
                    key={`${title}-${rowIndex}-${columnIndex}`}
                    className="px-3 py-2 text-right tabular-nums text-[var(--sea-ink-soft)]"
                  >
                    {value}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

function formatGroupSummary(groups: VoteResultGroup[]): string {
  const winner = groups[0]?.candidates.map((candidate) => candidate.candidate).join(' = ')
  if (!winner) return 'No winner'
  return `Leader: ${winner}`
}

function methodLabel(method: VotingMethod): string {
  switch (method) {
    case 'plurality':
      return 'Plurality results'
    case 'borda':
      return 'Borda count results'
    case 'schulze':
      return 'Schulze method results'
  }
}
