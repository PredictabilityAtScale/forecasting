import { describe, expect, it } from 'vitest'
import {
  computeBordaResults,
  computePluralityResults,
  computeSchulzeResults,
  createVoteTally,
  findPathStrength,
  getInvalidVoteReasons,
  getPairWins,
  isVoteValid,
  makeWinners,
  voteOrderPreferencesAsAlphabeticalVoteTally,
  type VoteResultGroup,
  type VoteTally,
} from './voting'

describe('vote tally validation', () => {
  it('flags invalid counts', () => {
    let actual = getInvalidVoteReasons(
      createVoteTally({ count: 0, preferenceOrder: 'ABC' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('0 or negative')

    actual = getInvalidVoteReasons(
      createVoteTally({ count: -1, preferenceOrder: 'ABC' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('0 or negative')
  })

  it('flags empty preference order', () => {
    let actual = getInvalidVoteReasons(
      createVoteTally({ count: 1, preferenceOrder: '' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('no candidates')

    actual = getInvalidVoteReasons(
      createVoteTally({ count: 1, preferenceOrder: null }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('no candidates')

    actual = getInvalidVoteReasons(
      createVoteTally({ count: 1, preferenceOrder: '>>==>>==' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('no candidates')
  })

  it('flags duplicate candidates', () => {
    let actual = getInvalidVoteReasons(
      createVoteTally({ count: 1, preferenceOrder: 'ABCA' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('appear more than once')

    actual = getInvalidVoteReasons(
      createVoteTally({ count: 1, preferenceOrder: 'ABCa' }),
    )
    expect(actual).toHaveLength(1)
    expect(actual[0]).toContain('appear more than once')
  })

  it('reports validity', () => {
    const vote = createVoteTally({ count: -1, preferenceOrder: 'ABC' })
    expect(isVoteValid(vote)).toBe(false)

    vote.count = 1
    expect(isVoteValid(vote)).toBe(true)
  })
})

describe('plurality voting', () => {
  it('matches the original results example', () => {
    const votes: VoteTally[] = [
      { count: 5, preferenceOrder: 'ABCD' },
      { count: 5, preferenceOrder: 'BACD' },
      { count: 6, preferenceOrder: 'BDCA' },
    ]

    const actual = computePluralityResults(votes)
    expect(actual).toHaveLength(2)
    expect(actual[0]?.score).toBe(11)
    expect(actual[0]?.candidates[0]?.candidate).toBe('B')
    expect(actual[1]?.score).toBe(5)
    expect(actual[1]?.candidates[0]?.candidate).toBe('A')
  })

  it('matches the condorcet paradox sample', () => {
    const votes: VoteTally[] = [
      { count: 7, preferenceOrder: 'BACD' },
      { count: 1, preferenceOrder: 'ACDB' },
      { count: 2, preferenceOrder: 'CADB' },
      { count: 2, preferenceOrder: 'DACB' },
    ]

    const actual = computePluralityResults(votes)
    expect(actual[0]?.candidates[0]?.candidate).toBe('B')
  })
})

describe('pair wins', () => {
  it('matches the base pair win example', () => {
    const votes: VoteTally[] = [
      { count: 3, preferenceOrder: 'ABCD' },
      { count: 4, preferenceOrder: 'BCDA' },
      { count: 2, preferenceOrder: 'CDAB' },
      { count: 1, preferenceOrder: 'DCBA' },
      { count: 1, preferenceOrder: 'CABD' },
    ]

    expect(getPairWins(votes)).toEqual([
      [0, 6, 3, 4],
      [5, 0, 7, 8],
      [8, 4, 0, 10],
      [7, 3, 1, 0],
    ])
  })

  it('matches the symbol handling example', () => {
    const votes: VoteTally[] = [
      { count: 1, preferenceOrder: 'AB>CD' },
      { count: 2, preferenceOrder: 'A=B>C=D' },
      { count: 4, preferenceOrder: 'A>B>C>D' },
    ]

    expect(getPairWins(votes)).toEqual([
      [0, 4, 7, 7],
      [0, 0, 7, 7],
      [0, 0, 0, 4],
      [0, 0, 0, 0],
    ])
  })
})

describe('borda counting', () => {
  it('matches the original borda example', () => {
    const votes: VoteTally[] = [
      { count: 2, preferenceOrder: 'ABCD' },
      { count: 2, preferenceOrder: 'BACD' },
      { count: 2, preferenceOrder: 'BDCA' },
    ]

    const actual = computeBordaResults(votes)
    expect(actual).toHaveLength(4)
    expect(compareResults(actual, 'B>A>C>D')).toBe(true)
    expect(actual[0]?.score).toBe(16)
    expect(actual[1]?.score).toBe(10)
    expect(actual[2]?.score).toBe(6)
    expect(actual[3]?.score).toBe(4)
  })

  it('matches the first test run example', () => {
    const votes: VoteTally[] = [
      { count: 4, preferenceOrder: 'BACD' },
      { count: 2, preferenceOrder: 'ACDB' },
      { count: 1, preferenceOrder: 'DCBA' },
    ]

    const actual = computeBordaResults(votes)
    expect(compareResults(actual, 'A>B>C>D')).toBe(true)
    expect(actual[0]?.score).toBe(14)
    expect(actual[1]?.score).toBe(13)
    expect(actual[2]?.score).toBe(10)
    expect(actual[3]?.score).toBe(5)
  })

  it('matches the removing candidate example', () => {
    const votes: VoteTally[] = [
      { count: 4, preferenceOrder: 'BAC' },
      { count: 2, preferenceOrder: 'ACB' },
      { count: 1, preferenceOrder: 'CBA' },
    ]

    const actual = computeBordaResults(votes)
    expect(compareResults(actual, 'B>A>C')).toBe(true)
    expect(actual[0]?.score).toBe(9)
    expect(actual[1]?.score).toBe(8)
    expect(actual[2]?.score).toBe(4)
  })

  it('matches the condorcet paradox sample', () => {
    const votes: VoteTally[] = [
      { count: 7, preferenceOrder: 'BACD' },
      { count: 1, preferenceOrder: 'ACDB' },
      { count: 2, preferenceOrder: 'CADB' },
      { count: 2, preferenceOrder: 'DACB' },
    ]

    const actual = computeBordaResults(votes)
    expect(actual[0]?.candidates[0]?.candidate).toBe('A')
  })

  it('matches electorama case 4', () => {
    const votes = voteOrderPreferencesAsAlphabeticalVoteTally(
      '3 ABCD|2 DABC|2 DBCA|2 CBDA|'.split('|'),
    )

    const actual = computeBordaResults(votes)
    expect(compareResults(actual, 'B>D>A>C')).toBe(true)
  })
})

describe('schulze method', () => {
  it('finds path strengths', () => {
    const pairWins = [
      [0, 6, 3, 4],
      [5, 0, 7, 8],
      [8, 4, 0, 10],
      [7, 3, 1, 0],
    ]

    expect(findPathStrength(pairWins)).toEqual([
      [0, 6, 6, 6],
      [7, 0, 7, 8],
      [8, 6, 0, 10],
      [7, 6, 6, 0],
    ])
  })

  it('identifies winners from a path strength matrix', () => {
    const pathStrengthMatrix = [
      [0, 6, 6, 6],
      [7, 0, 7, 8],
      [8, 6, 0, 10],
      [7, 6, 6, 0],
    ]

    expect(makeWinners(pathStrengthMatrix)).toEqual([false, true, false, false])
  })

  it('matches the condorcet paradox sample', () => {
    const votes: VoteTally[] = [
      { count: 7, preferenceOrder: 'BACD' },
      { count: 1, preferenceOrder: 'ACDB' },
      { count: 2, preferenceOrder: 'CADB' },
      { count: 2, preferenceOrder: 'DACB' },
    ]

    const actual = computeSchulzeResults(votes).results
    expect(actual[0]?.candidates[0]?.candidate).toBe('B')
  })

  it('matches electorama case 1', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '5 ACBED|5 ADECB|8 BEDAC|3 CABED|7 CAEBD|2 CBADE|7 DCEBA|8 EBADC'.split(
          '|',
        ),
      ),
    ).results

    expect(compareResults(actual, 'E>A>C>B>D')).toBe(true)
  })

  it('matches electorama case 2', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '5 ACBD|2 ACDB|3 ADCB|4 BACD|3 CBDA|3 CDBA|1 DACB|5 DBAC|4 DCBA'.split(
          '|',
        ),
      ),
    ).results

    expect(compareResults(actual, 'D>A>C>B')).toBe(true)
  })

  it('matches electorama case 3', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '3 ABDEC|5 ADEBC|1 ADECB|2 BADEC|2 BDECA|4 CABDE|6 CBADE|2 DBECA|5 DECAB'.split(
          '|',
        ),
      ),
    ).results

    expect(compareResults(actual, 'B>A>D>E>C')).toBe(true)
  })

  it('matches electorama case 4 ties', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '3 ABCD|2 DABC|2 DBCA|2 CBDA|'.split('|'),
      ),
    ).results

    expect(compareResults(actual, 'BD>AC')).toBe(true)
  })

  it('matches schulze example 3.1', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '8 acdb|2 badc|4 cdba|4 dbac|3 dcba'.split('|'),
      ),
    ).results

    expect(compareResults(actual, 'D>A>C>B')).toBe(true)
  })

  it('matches schulze example 3.2', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '3 abcd|2 cbda|2 dabc|2 dbca'.split('|'),
      ),
    ).results

    expect(compareResults(actual, 'BD>AC')).toBe(true)
  })

  it('matches schulze example 3.3', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '6 abcd|12 acdb|21 bcad|9 cdba|15 dbac'.split('|'),
      ),
    ).results

    expect(compareResults(actual, 'B>AC>D')).toBe(true)
  })

  it('matches schulze example 3.4 and the modified follow-up', () => {
    const votes = voteOrderPreferencesAsAlphabeticalVoteTally(
      '3 adebcf|3 bfecda|4 cabfde|1 dbcefa|4 defabc|2 ecbdfa|2 facdbe'.split(
        '|',
      ),
    )

    expect(compareResults(computeSchulzeResults(votes).results, 'A>B>F>D>E>C')).toBe(
      true,
    )

    votes.push({ count: 2, preferenceOrder: 'AEFCBD' })
    expect(compareResults(computeSchulzeResults(votes).results, 'D>E>C>A>B>F')).toBe(
      true,
    )
  })

  it('matches schulze example 6 with ties and skipped rankings', () => {
    const actual = computeSchulzeResults(
      voteOrderPreferencesAsAlphabeticalVoteTally(
        '6 a>b>c>d|8 a=b>c=d|8 a=c>b=d|18 a=c>d>b|8 a=c=d>b|40 b>a=c=d|4 c>b>d>a|9 c>d>a>b|8 c=d>a=b|14 d>a>b>c|11 d>b>c>a|4 d>c>a>b'.split(
          '|',
        ),
      ),
    ).results

    expect(compareResults(actual, 'D>A>B>C')).toBe(true)
  })
})

function compareResults(actual: VoteResultGroup[], expected: string): boolean {
  const expectedGroups = expected.split(/[\|>]/)
  if (actual.length !== expectedGroups.length) {
    return false
  }

  for (let index = 0; index < actual.length; index++) {
    const group = actual[index]
    const expectedGroup = expectedGroups[index]
    if (!group || !expectedGroup || group.candidates.length !== expectedGroup.length) {
      return false
    }

    for (let resultIndex = 0; resultIndex < group.candidates.length; resultIndex++) {
      if (group.candidates[resultIndex]?.candidate !== expectedGroup[resultIndex]) {
        return false
      }
    }
  }

  return true
}
