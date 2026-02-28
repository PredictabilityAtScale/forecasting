/**
 * Monte Carlo simulation engine.
 *
 * Replicates the spreadsheet logic from:
 *   - "Throughput Forecaster.xlsx"  (single-item completion forecast)
 *   - "Multiple Feature Cut Line Forecaster.xlsx"  (multi-feature cut-line)
 *   - "Story Count Forecaster.xlsx"  (total story count estimation via bootstrap)
 */

// ─── helpers ────────────────────────────────────────────────────────────────

/** Inclusive random integer [low, high]. */
function randBetween(low: number, high: number): number {
  return Math.floor(Math.random() * (high - low + 1)) + low
}

/** Random float in [low, high). */
function randFloat(low: number, high: number): number {
  return Math.random() * (high - low) + low
}

/**
 * Triangle-distribution sample via inverse CDF.
 * Used when a "most likely" value is supplied alongside low & high.
 */
function triangleSample(low: number, mode: number, high: number): number {
  const u = Math.random()
  const fc = (mode - low) / (high - low)
  if (u < fc) {
    return low + Math.sqrt(u * (high - low) * (mode - low))
  }
  return high - Math.sqrt((1 - u) * (high - low) * (high - mode))
}

/** Return the k-th percentile (0-1) of a sorted (ascending) array. */
function percentile(sorted: number[], p: number): number {
  if (sorted.length === 0) return 0
  const idx = p * (sorted.length - 1)
  const lo = Math.floor(idx)
  const hi = Math.ceil(idx)
  if (lo === hi) return sorted[lo]
  return sorted[lo] + (sorted[hi] - sorted[lo]) * (idx - lo)
}

// ─── Throughput Forecaster ─────────────────────────────────────────────────

export interface ThroughputForecasterInputs {
  /** Optional start date for computing calendar dates */
  startDate?: string
  /** Story count low / high estimates (pre-complexity adjustment) */
  storyCountLow: number
  storyCountHigh: number
  /** Scope complexity – multiplied into story counts */
  complexityLowMultiplier: number
  complexityHighMultiplier: number
  /** Story split rate range */
  splitRateLow: number
  splitRateHigh: number
  /** Throughput source mode */
  throughputMode: 'estimate' | 'data'
  /** Only used when mode = 'estimate' */
  throughputLow: number
  throughputHigh: number
  /** Optional "most likely" – enables triangle distribution */
  throughputMostLikely?: number | null
  /** Historical throughput samples – used when mode = 'data' */
  samples: number[]
  /** Team focus 0-1 */
  focusPercentage: number
  /** Days per throughput unit (7 = 1 week) */
  daysPerUnit: number
  /** Risks: array of {likelihood (0-1), impactLow, impactHigh} */
  risks: { likelihood: number; impactLow: number; impactHigh: number }[]
  /** Number of simulation trials */
  numTrials: number
  /** Max periods (weeks) to simulate */
  maxPeriods: number
  /** How many weeks for the "story count" forecast */
  weeksToForecast: number
}

export interface ThroughputForecasterResults {
  /** e.g. [{likelihood:0.95, weeks:12, date:'2021-03-24'}, …] */
  completionPercentiles: {
    likelihood: number
    weeks: number
    date: string | null
  }[]
  /** For the "story count in N weeks" forecast */
  storyCountPercentiles: { likelihood: number; count: number }[]
  /** Raw weeks-to-zero values, sorted ascending */
  weeksToZeroRaw: number[]
  /** Raw total-throughput-in-N-weeks values, sorted ascending */
  totalThroughputRaw: number[]
  /** Histogram data for the completion chart */
  completionHistogram: { week: number; count: number }[]
}

export function runThroughputForecaster(
  inputs: ThroughputForecasterInputs,
): ThroughputForecasterResults {
  const {
    storyCountLow,
    storyCountHigh,
    complexityLowMultiplier,
    complexityHighMultiplier,
    splitRateLow,
    splitRateHigh,
    throughputMode,
    throughputLow,
    throughputHigh,
    throughputMostLikely,
    samples,
    focusPercentage,
    daysPerUnit,
    risks,
    numTrials,
    maxPeriods,
    weeksToForecast,
    startDate,
  } = inputs

  const adjLow = Math.round(storyCountLow * complexityLowMultiplier)
  const adjHigh = Math.round(storyCountHigh * complexityHighMultiplier)

  const weeksToZero: number[] = []
  const totalThroughput: number[] = [] // sum of throughput in weeksToForecast

  for (let t = 0; t < numTrials; t++) {
    // 1. Initial story count with split rate and risks
    const splitRate = randFloat(splitRateLow, splitRateHigh)
    let initialStories = Math.ceil(randBetween(adjLow, adjHigh) * splitRate)

    // Add risk impacts
    for (const r of risks) {
      if (r.likelihood > 0 && Math.random() <= r.likelihood) {
        initialStories += randBetween(r.impactLow, r.impactHigh)
      }
    }

    // 2. Simulate burndown
    let remaining = initialStories
    let weeksToComplete = maxPeriods // fallback
    for (let w = 0; w < maxPeriods; w++) {
      if (remaining <= 0) {
        weeksToComplete = w
        break
      }
      const tp = sampleThroughput(
        throughputMode,
        throughputLow,
        throughputHigh,
        throughputMostLikely,
        samples,
        focusPercentage,
      )
      remaining = Math.max(0, remaining - tp)
      if (remaining <= 0) {
        weeksToComplete = w + 1
        break
      }
    }
    weeksToZero.push(weeksToComplete)

    // 3. Throughput total in weeksToForecast periods (for story count mode)
    let sumTp = 0
    for (let w = 0; w < weeksToForecast; w++) {
      const tp = sampleThroughput(
        throughputMode,
        throughputLow,
        throughputHigh,
        throughputMostLikely,
        samples,
        focusPercentage,
      )
      // Divide by random split rate to get pre-split stories
      const splitDiv = randFloat(splitRateLow, splitRateHigh)
      sumTp += Math.max(1, tp / splitDiv)
    }
    totalThroughput.push(sumTp)
  }

  // Sort for percentile calculation
  weeksToZero.sort((a, b) => a - b)
  totalThroughput.sort((a, b) => a - b)

  // Percentiles for completion
  const likelihoods = [1, 0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5]
  const startDt = startDate ? new Date(startDate) : null

  const completionPercentiles = likelihoods.map((l) => {
    const weeks = Math.ceil(percentile(weeksToZero, l))
    let date: string | null = null
    if (startDt) {
      const d = new Date(startDt)
      d.setDate(d.getDate() + weeks * daysPerUnit)
      date = d.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      })
    }
    return { likelihood: l, weeks, date }
  })

  // Percentiles for story count (inverted – report at 95%, 85%, 5%)
  const storyLikelihoods = [0.95, 0.85, 0.5, 0.05]
  const storyCountPercentiles = storyLikelihoods.map((l) => ({
    likelihood: l,
    count: Math.round(percentile(totalThroughput, 1 - l)),
  }))

  // Histogram for completion weeks
  const histMap = new Map<number, number>()
  for (const w of weeksToZero) {
    histMap.set(w, (histMap.get(w) ?? 0) + 1)
  }
  const completionHistogram = Array.from(histMap.entries())
    .map(([week, count]) => ({ week, count }))
    .sort((a, b) => a.week - b.week)

  return {
    completionPercentiles,
    storyCountPercentiles,
    weeksToZeroRaw: weeksToZero,
    totalThroughputRaw: totalThroughput,
    completionHistogram,
  }
}

function sampleThroughput(
  mode: 'estimate' | 'data',
  low: number,
  high: number,
  mostLikely: number | null | undefined,
  samples: number[],
  focusPct: number,
): number {
  if (mode === 'data' && samples.length > 0) {
    return samples[randBetween(0, samples.length - 1)] * focusPct
  }
  // Estimate mode
  if (mostLikely != null && mostLikely > 0 && low < mostLikely && mostLikely < high) {
    return Math.round(triangleSample(low, mostLikely, high)) * focusPct
  }
  return randBetween(low, high) * focusPct
}

// ─── Multi-Feature Cut Line Forecaster ─────────────────────────────────────

export interface Feature {
  name: string
  storyCountLow: number
  storyCountHigh: number
  /** Scope complexity category key */
  complexityLowMultiplier: number
  complexityHighMultiplier: number
}

export interface MultiFeatureInputs {
  startDate: string
  targetDate: string
  targetLikelihood: number
  splitRateLow: number
  splitRateHigh: number
  throughputMode: 'estimate' | 'data'
  throughputLow: number
  throughputHigh: number
  samples: number[]
  focusPercentage: number
  daysPerUnit: number
  /** Monthly multipliers [jan=1..dec=12] → multiplier */
  monthlyAdjustments: number[]
  features: Feature[]
  numTrials: number
  maxPeriods: number
}

export interface MultiFeatureResult {
  name: string
  intervalsAtLikelihood: number | null
  forecastDate: string | null
  /** 1 = on/before target, 2 = within 1 period, 3 = after */
  status: 1 | 2 | 3
}

export interface MultiFeatureResults {
  features: MultiFeatureResult[]
  /** Raw result intervals per feature [featureIdx][trialIdx] for charts */
  rawResults: number[][]
}

export function runMultiFeatureForecaster(
  inputs: MultiFeatureInputs,
): MultiFeatureResults {
  const {
    startDate,
    targetDate,
    targetLikelihood,
    splitRateLow,
    splitRateHigh,
    throughputMode,
    throughputLow,
    throughputHigh,
    samples,
    focusPercentage,
    daysPerUnit,
    monthlyAdjustments,
    features,
    numTrials,
    maxPeriods,
  } = inputs

  const startDt = new Date(startDate)
  const targetDt = new Date(targetDate)

  // Pre-compute dates for each period (for monthly adjustment lookup)
  const periodDates: Date[] = []
  for (let w = 0; w < maxPeriods; w++) {
    const d = new Date(startDt)
    d.setDate(d.getDate() + (w + 1) * daysPerUnit)
    periodDates.push(d)
  }

  const activeFeatures = features.filter(
    (f) => f.storyCountLow > 0 || f.storyCountHigh > 0,
  )

  // rawResults[featureIdx][trialIdx] = number of intervals to complete
  const rawResults: number[][] = activeFeatures.map(() => [])

  for (let t = 0; t < numTrials; t++) {
    // 1. Random split rate for this trial
    const splitMult = randFloat(splitRateLow, splitRateHigh)

    // 2. Build cumulative goals per feature
    const goals: number[] = []
    let cumulative = 0
    for (const f of activeFeatures) {
      const adjLow = Math.round(f.storyCountLow * f.complexityLowMultiplier)
      const adjHigh = Math.round(f.storyCountHigh * f.complexityHighMultiplier)
      const featureStories = Math.ceil(
        randBetween(adjLow, adjHigh) * splitMult,
      )
      cumulative += featureStories
      goals.push(cumulative)
    }

    // 3. Simulate cumulative throughput
    const cumulativeTp: number[] = []
    let cumTp = 0
    for (let w = 0; w < maxPeriods; w++) {
      const monthIdx = periodDates[w].getMonth() // 0-11
      const monthMult = monthlyAdjustments[monthIdx] ?? 1

      let tp: number
      if (throughputMode === 'data' && samples.length > 0) {
        tp = samples[randBetween(0, samples.length - 1)]
      } else {
        tp = randBetween(throughputLow, throughputHigh)
      }
      cumTp += tp * monthMult * focusPercentage
      cumulativeTp.push(cumTp)
    }

    // 4. For each feature, find the first week where cumTp >= goal
    for (let fi = 0; fi < activeFeatures.length; fi++) {
      const goal = goals[fi]
      let found = maxPeriods
      for (let w = 0; w < maxPeriods; w++) {
        if (cumulativeTp[w] >= goal) {
          found = w + 1 // 1-based
          break
        }
      }
      rawResults[fi].push(found)
    }
  }

  // Sort each feature's results
  for (const arr of rawResults) {
    arr.sort((a, b) => a - b)
  }

  // Calculate intervals at target likelihood & forecast dates
  const featureResults: MultiFeatureResult[] = activeFeatures.map((f, fi) => {
    const intervals = Math.ceil(
      percentile(rawResults[fi], targetLikelihood),
    )
    const forecastDt = new Date(startDt)
    forecastDt.setDate(forecastDt.getDate() + intervals * daysPerUnit)

    const forecastDate = forecastDt.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })

    // Status: compare forecast date to target date
    const diffMs = forecastDt.getTime() - targetDt.getTime()
    const diffDays = diffMs / (1000 * 60 * 60 * 24)
    let status: 1 | 2 | 3
    if (diffDays <= 0) {
      status = 1 // on or before target
    } else if (diffDays <= daysPerUnit) {
      status = 2 // within 1 period
    } else {
      status = 3 // after target
    }

    return {
      name: f.name,
      intervalsAtLikelihood: intervals,
      forecastDate,
      status,
    }
  })

  return { features: featureResults, rawResults }
}

// ─── Story Count Forecaster ────────────────────────────────────────────────
// Replicates "Story Count Forecaster.xlsx".
// The user enters historical feature/epic size estimates (and optionally
// actuals).  The Monte Carlo resamples from the estimates to project
// total story count for a given number of features, applying a split rate.

export interface FeatureEstimate {
  id: string
  name: string
  /** Estimated stories/points before starting */
  estimate: number | null
  /** Actual stories/points after completion (optional) */
  actual: number | null
}

export interface StoryCountInputs {
  /** Total number of features to forecast */
  totalFeatureCount: number
  /** Split rate range (low / high, e.g. 1 = no split) */
  splitRateLow: number
  splitRateHigh: number
  /** Historical feature size estimates entered by the user */
  features: FeatureEstimate[]
  /** Number of simulation trials (spreadsheet uses 1000) */
  numTrials: number
}

export interface StoryCountResults {
  /** Percentile results – e.g. [{likelihood:0.50, count:42}, …] */
  percentiles: { likelihood: number; count: number }[]
  /** Raw trial sums, sorted ascending */
  rawTrialSums: number[]
  /** Histogram bins for the chart */
  histogram: { binMin: number; count: number; probability: number; cumProbability: number }[]
  /** Stability analysis */
  stability: {
    sampleCount: number
    sampleQuality: string
    /** Normalised error (0 = identical groups, 1 = max divergence) */
    errorOfAvgRatio: number | null
    /** Min/max of split rates from actuals, if available */
    actualSplitRange: { min: number; max: number } | null
  }
}

export function runStoryCountForecaster(
  inputs: StoryCountInputs,
): StoryCountResults {
  const {
    totalFeatureCount,
    splitRateLow,
    splitRateHigh,
    features,
    numTrials,
  } = inputs

  // ── Collect the estimates that have values ────────────────────────────
  const estimates: number[] = features
    .map((f) => f.estimate)
    .filter((v): v is number => v != null && v > 0)

  const numberOfEstimates = estimates.length
  if (numberOfEstimates === 0) {
    return {
      percentiles: [],
      rawTrialSums: [],
      histogram: [],
      stability: {
        sampleCount: 0,
        sampleQuality: '5 recommended minimum',
        errorOfAvgRatio: null,
        actualSplitRange: null,
      },
    }
  }

  // ── Build a sequential rank for resampling (matches spreadsheet) ─────
  // Sort estimates ascending; ties broken by original order
  const ranked = estimates
    .map((v, i) => ({ value: v, origIdx: i }))
    .sort((a, b) => a.value - b.value || a.origIdx - b.origIdx)
  const rankedValues = ranked.map((r) => r.value)

  // ── Monte Carlo simulation ───────────────────────────────────────────
  const trialSums: number[] = []

  for (let t = 0; t < numTrials; t++) {
    let sum = 0
    for (let f = 0; f < totalFeatureCount; f++) {
      // Randomly pick one of the estimates (1-based RANDBETWEEN in spreadsheet
      // maps to MATCH against SequentialRankRange → sample by rank index)
      const idx = randBetween(0, numberOfEstimates - 1)
      sum += rankedValues[idx]
    }
    // Apply random split rate and ceiling
    const splitRate = randFloat(splitRateLow, splitRateHigh)
    trialSums.push(Math.ceil(sum * splitRate))
  }

  trialSums.sort((a, b) => a - b)

  // ── Percentiles (spreadsheet shows 50%, 85%, 95%) ────────────────────
  const likelihoods = [0.5, 0.85, 0.95]
  const percentiles = likelihoods.map((l) => ({
    likelihood: l,
    count: Math.round(percentile(trialSums, l)),
  }))

  // ── Histogram (21 bins, matching Chart Data sheet) ───────────────────
  const minVal = trialSums[0]
  const maxVal = trialSums[trialSums.length - 1]
  const numBins = 21
  const binWidth = maxVal > minVal ? (maxVal - minVal) / 20 : 1
  const histogram: StoryCountResults['histogram'] = []
  let cumProb = 0

  for (let b = 0; b < numBins; b++) {
    const binMin = minVal + b * binWidth
    const binMax = b < numBins - 1 ? minVal + (b + 1) * binWidth : Infinity
    const count = trialSums.filter(
      (v) => v >= binMin && (b < numBins - 1 ? v < binMax : true),
    ).length
    const prob = count / numTrials
    cumProb += prob
    histogram.push({ binMin: Math.round(binMin), count, probability: prob, cumProbability: cumProb })
  }

  // ── Stability analysis ───────────────────────────────────────────────
  // Split samples into two alternating groups by rank (odd=1, even=2)
  const group1: number[] = []
  const group2: number[] = []
  rankedValues.forEach((v, i) => {
    if (i % 2 === 0) group1.push(v)
    else group2.push(v)
  })

  const avg = (arr: number[]) =>
    arr.length > 0 ? arr.reduce((s, v) => s + v, 0) / arr.length : 0

  const avg1 = avg(group1)
  const avg2 = avg(group2)
  const avgError = Math.abs(avg1 - avg2)
  const range = maxVal > minVal ? (rankedValues[rankedValues.length - 1] - rankedValues[0]) : 0
  const errorOfAvgRatio = range > 0 ? avgError / range : null

  let sampleQuality: string
  if (numberOfEstimates < 5) sampleQuality = '5 recommended minimum'
  else if (numberOfEstimates <= 7) sampleQuality = 'Acceptable'
  else if (numberOfEstimates <= 11) sampleQuality = 'Good'
  else sampleQuality = 'Excellent'

  // Actual split rates (from features that have both estimate and actual)
  const splitRates = features
    .filter((f) => f.estimate != null && f.estimate > 0 && f.actual != null && f.actual > 0)
    .map((f) => f.actual! / f.estimate!)

  const actualSplitRange =
    splitRates.length > 0
      ? { min: Math.min(...splitRates), max: Math.max(...splitRates) }
      : null

  return {
    percentiles,
    rawTrialSums: trialSums,
    histogram,
    stability: {
      sampleCount: numberOfEstimates,
      sampleQuality,
      errorOfAvgRatio,
      actualSplitRange,
    },
  }
}
