export interface VoteTallyInput {
  count?: number
  preferenceOrder?: string | null
}

export interface VoteTally {
  count: number
  preferenceOrder: string
}

export interface VoteCandidate {
  id: string
  title?: string
  description?: string
}

export interface VoteCandidateResult {
  rank: number
  candidate: string
  score: number
  pairWins?: number[]
}

export interface VoteResultGroup {
  rank: number
  score: number
  candidates: VoteCandidateResult[]
}

export interface BallotSummaryRow {
  preferenceOrder: string
  count: number
}

export interface VotingComputation {
  candidates: string[]
  ballotSummary: BallotSummaryRow[]
  plurality: VoteResultGroup[]
  borda: VoteResultGroup[]
  schulze: VoteResultGroup[]
  pairWins: number[][]
  pathStrengths: number[][]
}

const NO_CANDIDATE_REASON =
  'Vote contains no candidates (empty, or just whitespace characters)'
const INVALID_COUNT_REASON =
  'Vote has a 0 or negative count. Each vote must have a count of 1 or greater'
const DUPLICATE_REASON =
  'One or more candidates appear more than once in this vote. Note: All candidates are converted to upper case, so A = a.'

export function createVoteTally(input: VoteTallyInput): VoteTally {
  return {
    count: input.count ?? 1,
    preferenceOrder: input.preferenceOrder ?? '',
  }
}

export function getVoteCandidates(vote: VoteTally): string[] {
  return Array.from(
    new Set(
      normalizePreferenceOrder(vote.preferenceOrder)
        .split('')
        .filter((candidate) => candidate !== '>' && candidate !== '='),
    ),
  ).sort()
}

export function getInvalidVoteReasons(vote: VoteTally): string[] {
  const result: string[] = []
  const preferenceOrder = vote.preferenceOrder

  if (vote.count <= 0) {
    result.push(INVALID_COUNT_REASON)
  }

  if (
    preferenceOrder == null ||
    !preferenceOrder
      .trim()
      .split('')
      .filter((candidate) => candidate !== '>' && candidate !== '=').length
  ) {
    result.push(NO_CANDIDATE_REASON)
  }

  if (preferenceOrder == null) {
    return result
  }

  const allCandidates = normalizePreferenceOrder(preferenceOrder)
    .split('')
    .filter((candidate) => candidate !== '>' && candidate !== '=')

  if (getVoteCandidates(vote).length !== allCandidates.length) {
    result.push(DUPLICATE_REASON)
  }

  return result
}

export function isVoteValid(vote: VoteTally): boolean {
  return getInvalidVoteReasons(vote).length === 0
}

export function voteOrderPreferencesAsAlphabeticalVoteTally(
  inputs: Iterable<string>,
): VoteTally[] {
  const results: VoteTally[] = []

  for (const input of inputs) {
    let preferenceOrder = input.toUpperCase()
    let count = 1
    let index = 0

    while (
      index < preferenceOrder.length - 1 &&
      /\d/.test(preferenceOrder[index] ?? '')
    ) {
      index++
    }

    if (index > 0) {
      count = Number.parseInt(preferenceOrder.slice(0, index), 10)
      preferenceOrder = preferenceOrder.slice(index).trim()
    }

    results.push({ count, preferenceOrder })
  }

  return results
}

export function getAllCandidates(votes: VoteTally[]): string[] {
  const candidates = new Set<string>()

  for (const vote of votes) {
    for (const candidate of normalizePreferenceOrder(vote.preferenceOrder)) {
      if (candidate !== '>' && candidate !== '=') {
        candidates.add(candidate)
      }
    }
  }

  return Array.from(candidates).sort()
}

export function rankOrderCentroid(totalCandidates: number, rank: number): number {
  let result = 0
  for (let index = rank; index <= totalCandidates; index++) {
    result += 1 / index
  }
  return result / totalCandidates
}

export function getPairWins(votes: VoteTally[]): number[][] {
  const candidates = getAllCandidates(votes)
  const pairWins = createSquareMatrix(candidates.length, 0)

  for (const vote of votes) {
    const preferenceOrder = normalizePreferenceOrder(vote.preferenceOrder)
    const specialCharsPresent =
      preferenceOrder.includes('>') || preferenceOrder.includes('=')

    for (let j = 0; j < preferenceOrder.length - 1; j++) {
      const jChar = preferenceOrder[j]
      if (jChar === '>' || jChar === '=') continue

      const candidateWin = candidates.indexOf(jChar)
      let greaterThanFound = false

      for (let k = j + 1; k < preferenceOrder.length; k++) {
        const kChar = preferenceOrder[k]

        switch (kChar) {
          case '>':
            greaterThanFound = true
            break
          case '=':
            break
          default: {
            if (!specialCharsPresent || greaterThanFound) {
              const candidateLose = candidates.indexOf(kChar)
              pairWins[candidateWin]![candidateLose]! += vote.count
            }
            break
          }
        }
      }
    }

    const voteCandidates = new Set(getVoteCandidates(vote))
    const skippedIndexes = candidates
      .filter((candidate) => !voteCandidates.has(candidate))
      .map((candidate) => candidates.indexOf(candidate))

    if (skippedIndexes.length > 0) {
      for (let row = 0; row < pairWins.length; row++) {
        if (skippedIndexes.includes(row)) continue

        for (let column = 0; column < pairWins[row]!.length; column++) {
          if (skippedIndexes.includes(column)) {
            pairWins[row]![column]! += vote.count
          }
        }
      }
    }
  }

  return pairWins
}

export function computePluralityResults(votes: VoteTally[]): VoteResultGroup[] {
  const scores = new Map<string, VoteCandidateResult>()

  for (const vote of votes) {
    const preferenceOrder = normalizePreferenceOrder(vote.preferenceOrder)
    if (!preferenceOrder) continue

    const candidate = preferenceOrder[0]
    if (!candidate) continue

    const existing = scores.get(candidate) ?? {
      rank: 0,
      candidate,
      score: 0,
    }
    existing.score += vote.count
    scores.set(candidate, existing)
  }

  return groupResultsByScore(Array.from(scores.values()))
}

export function computeBordaResults(votes: VoteTally[]): VoteResultGroup[] {
  const scores = new Map<string, VoteCandidateResult>()

  for (const vote of votes) {
    const preferenceOrder = normalizePreferenceOrder(vote.preferenceOrder)
    const numberOfCandidates = preferenceOrder.length

    for (let index = 0; index < numberOfCandidates; index++) {
      const candidate = preferenceOrder[index]
      if (!candidate) continue

      const score = numberOfCandidates - index - 1
      const existing = scores.get(candidate) ?? {
        rank: 0,
        candidate,
        score: 0,
      }
      existing.score += score * vote.count
      scores.set(candidate, existing)
    }
  }

  return groupResultsByScore(Array.from(scores.values()))
}

export function findPathStrength(pairWins: number[][]): number[][] {
  const strengths = createSquareMatrix(pairWins.length, 0)

  for (let i = 0; i < pairWins.length; i++) {
    for (let j = 0; j < pairWins.length; j++) {
      if (i === j) continue
      strengths[i]![j] =
        pairWins[i]![j]! > pairWins[j]![i]! ? pairWins[i]![j]! : 0
    }

    for (let i1 = 0; i1 < strengths.length; i1++) {
      for (let j1 = 0; j1 < strengths.length; j1++) {
        if (i1 === j1) continue

        for (let k1 = 0; k1 < strengths.length; k1++) {
          if (i1 === k1 || j1 === k1) continue

          strengths[j1]![k1] = Math.max(
            strengths[j1]![k1]!,
            Math.min(strengths[j1]![i1]!, strengths[i1]![k1]!),
          )
        }
      }
    }
  }

  return strengths
}

export function makeWinners(pathStrengthMatrix: number[][]): boolean[] {
  const result = Array.from({ length: pathStrengthMatrix.length }, () => true)

  for (let i = 0; i < pathStrengthMatrix.length; i++) {
    for (let j = 0; j < pathStrengthMatrix.length; j++) {
      if (pathStrengthMatrix[i]![j]! < 0) {
        result[i] = false
      }

      if (pathStrengthMatrix[i]![j]! < pathStrengthMatrix[j]![i]!) {
        result[i] = false
      }
    }
  }

  return result
}

export function computeSchulzeResults(votes: VoteTally[]): {
  results: VoteResultGroup[]
  pairWins: number[][]
  pathStrengths: number[][]
} {
  const candidates = getAllCandidates(votes)
  const pairWins = getPairWins(votes)
  const pathStrengths = findPathStrength(pairWins)
  const workingPathStrengths = pathStrengths.map((row) => [...row])
  const results: VoteCandidateResult[] = []

  let winners = makeWinners(workingPathStrengths)
  let rank = 1

  while (anyWinners(winners)) {
    for (let i = 0; i < winners.length; i++) {
      if (!winners[i]) continue

      let score = 0
      for (let j = 0; j < workingPathStrengths.length; j++) {
        if (
          workingPathStrengths[j]![i]! > -1 &&
          workingPathStrengths[i]![j]! > workingPathStrengths[j]![i]!
        ) {
          score += 1
        }
      }

      results.push({
        rank,
        candidate: candidates[i]!,
        score,
      })

      for (let j = 0; j < workingPathStrengths.length; j++) {
        workingPathStrengths[i]![j] = -1
      }
    }

    winners = makeWinners(workingPathStrengths)
    rank++
  }

  return {
    results: groupResultsByRank(results),
    pairWins,
    pathStrengths,
  }
}

export function computeVoting(votes: VoteTally[]): VotingComputation {
  const candidates = getAllCandidates(votes)
  const plurality = computePluralityResults(votes)
  const borda = computeBordaResults(votes)
  const schulze = computeSchulzeResults(votes)

  return {
    candidates,
    ballotSummary: summarizeBallots(votes),
    plurality,
    borda,
    schulze: schulze.results,
    pairWins: schulze.pairWins,
    pathStrengths: schulze.pathStrengths,
  }
}

export function summarizeBallots(votes: VoteTally[]): BallotSummaryRow[] {
  const summary = new Map<string, number>()

  for (const vote of votes) {
    const key = normalizePreferenceOrder(vote.preferenceOrder).trim()
    if (!key) continue
    summary.set(key, (summary.get(key) ?? 0) + vote.count)
  }

  return Array.from(summary.entries())
    .map(([preferenceOrder, count]) => ({ preferenceOrder, count }))
    .sort((left, right) => right.count - left.count)
}

function groupResultsByScore(results: VoteCandidateResult[]): VoteResultGroup[] {
  const sorted = [...results].sort(
    (left, right) =>
      right.score - left.score || left.candidate.localeCompare(right.candidate),
  )
  const groups = new Map<number, VoteCandidateResult[]>()

  for (const result of sorted) {
    const list = groups.get(result.score) ?? []
    list.push(result)
    groups.set(result.score, list)
  }

  return Array.from(groups.entries()).map(([score, candidates], index) => ({
    rank: index + 1,
    score,
    candidates,
  }))
}

function groupResultsByRank(results: VoteCandidateResult[]): VoteResultGroup[] {
  const sorted = [...results].sort(
    (left, right) =>
      left.rank - right.rank || left.candidate.localeCompare(right.candidate),
  )
  const groups = new Map<number, VoteCandidateResult[]>()

  for (const result of sorted) {
    const list = groups.get(result.rank) ?? []
    list.push(result)
    groups.set(result.rank, list)
  }

  return Array.from(groups.entries()).map(([rank, candidates]) => ({
    rank,
    score: candidates[0]?.score ?? 0,
    candidates,
  }))
}

function anyWinners(winners: boolean[]): boolean {
  let foundFalse = false
  let foundTrue = false

  for (const winner of winners) {
    if (winner) foundTrue = true
    if (!winner) foundFalse = true
  }

  return foundTrue && foundFalse
}

function normalizePreferenceOrder(value: string | null | undefined): string {
  return (value ?? '').trim().toUpperCase()
}

function createSquareMatrix(size: number, initialValue: number): number[][] {
  return Array.from({ length: size }, () =>
    Array.from({ length: size }, () => initialValue),
  )
}
