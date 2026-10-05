import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  parseThroughputSamples,
  runMultiFeatureForecaster,
  runThroughputForecaster,
} from './monte-carlo'
import type {
  MultiFeatureInputs,
  ThroughputForecasterInputs,
} from './monte-carlo'

const throughputInputs: ThroughputForecasterInputs = {
  storyCountLow: 1,
  storyCountHigh: 1,
  complexityLowMultiplier: 1,
  complexityHighMultiplier: 1,
  splitRateLow: 1,
  splitRateHigh: 1,
  throughputMode: 'data',
  throughputLow: 10,
  throughputHigh: 10,
  samples: [1],
  focusPercentage: 1,
  daysPerUnit: 7,
  risks: [],
  numTrials: 1,
  maxPeriods: 4,
  weeksToForecast: 1,
}

const multiFeatureInputs: MultiFeatureInputs = {
  ...throughputInputs,
  startDate: '2026-10-05',
  targetDate: '2026-10-12',
  targetLikelihood: 0.85,
  monthlyAdjustments: Array(12).fill(1),
  features: [
    {
      name: 'Feature',
      storyCountLow: 1,
      storyCountHigh: 1,
      complexityLowMultiplier: 1,
      complexityHighMultiplier: 1,
    },
  ],
}

afterEach(() => vi.restoreAllMocks())

describe('historical throughput samples', () => {
  it('retains each zero-delivery sprint in the reported example', () => {
    expect(parseThroughputSamples('13\n4\n12\n9\n0\n0\n0')).toEqual([
      13, 4, 12, 9, 0, 0, 0,
    ])
  })

  it('ignores blanks and invalid values without turning them into zero samples', () => {
    expect(
      parseThroughputSamples(
        '\n , 0, , 2.5\r\n-1,invalid,Infinity,-Infinity,NaN, ',
      ),
    ).toEqual([0, 2.5])
    expect(parseThroughputSamples(' ,\n\r\n ')).toEqual([])
  })

  it('delays throughput completion when a zero sprint is sampled', () => {
    const random = vi.spyOn(Math, 'random').mockReturnValue(0.75)
    const baseline = runThroughputForecaster(throughputInputs)
    random
      .mockReturnValueOnce(0.75)
      .mockReturnValueOnce(0.75)
      .mockReturnValueOnce(0)
    const result = runThroughputForecaster({
      ...throughputInputs,
      samples: parseThroughputSamples('0,1'),
    })
    expect(baseline.weeksToZeroRaw).toEqual([1])
    expect(result.weeksToZeroRaw).toEqual([2])
  })

  it('delays multi-feature completion when a zero sprint is sampled', () => {
    const random = vi.spyOn(Math, 'random').mockReturnValue(0.75)
    const baseline = runMultiFeatureForecaster(multiFeatureInputs)
    random
      .mockReturnValueOnce(0.75)
      .mockReturnValueOnce(0.75)
      .mockReturnValueOnce(0)
    const result = runMultiFeatureForecaster({
      ...multiFeatureInputs,
      samples: parseThroughputSamples('0\n1'),
    })
    expect(baseline.rawResults).toEqual([[1]])
    expect(result.rawResults).toEqual([[2]])
    expect(result.features[0].status).toBe(2)
  })

  it('forecasts no delivery for all-zero history without falling back to estimates', () => {
    const samples = parseThroughputSamples('0,0,0')
    const result = runThroughputForecaster({ ...throughputInputs, samples })
    expect(result.weeksToZeroRaw).toEqual([4])
    expect(result.unfinishedTrials).toBe(1)
    expect(result.completionHistogram).toEqual([])
    expect(
      result.completionPercentiles.every(
        (row) => row.exceedsHorizon && row.date === null,
      ),
    ).toBe(true)
    expect(result.totalThroughputRaw).toEqual([0])
    expect(result.storyCountPercentiles.every(({ count }) => count === 0)).toBe(
      true,
    )
    expect(
      runMultiFeatureForecaster({ ...multiFeatureInputs, samples }).rawResults,
    ).toEqual([[4]])
    expect(
      runMultiFeatureForecaster({ ...multiFeatureInputs, samples }).features[0],
    ).toMatchObject({
      unfinishedTrials: 1,
      intervalsAtLikelihood: null,
      forecastDate: null,
    })
  })

  it('distinguishes completion in the last period from an unfinished trial', () => {
    const result = runThroughputForecaster({
      ...throughputInputs,
      storyCountLow: 4,
      storyCountHigh: 4,
    })
    expect(result.unfinishedTrials).toBe(0)
    expect(
      result.completionPercentiles.every(
        (row) => !row.exceedsHorizon && row.weeks === 4,
      ),
    ).toBe(true)
    expect(
      runMultiFeatureForecaster({
        ...multiFeatureInputs,
        features: [
          {
            ...multiFeatureInputs.features[0],
            storyCountLow: 4,
            storyCountHigh: 4,
          },
        ],
      }).features[0],
    ).toMatchObject({ unfinishedTrials: 0, intervalsAtLikelihood: 4 })
  })
})
