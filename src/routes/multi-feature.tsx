import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useState, useEffect } from 'react'
import { z } from 'zod'
import { runMultiFeatureForecaster } from '#/lib/monte-carlo'
import type { MultiFeatureInputs, MultiFeatureResults, Feature } from '#/lib/monte-carlo'
import Field from '#/components/Field'
import NumberInput from '#/components/NumberInput'
import CopyLinkButton from '#/components/CopyLinkButton'

const featureSearchSchema = z.object({
  name: z.string(),
  storyLow: z.number(),
  storyHigh: z.number(),
  complexityIdx: z.number().int(),
})

const multiFeatureSearchSchema = z.object({
  startDate: z.string().optional(),
  targetDate: z.string().optional(),
  targetLikelihood: z.number().optional(),
  splitLow: z.number().optional(),
  splitHigh: z.number().optional(),
  durationIdx: z.number().int().optional(),
  throughputMode: z.enum(['estimate', 'data']).optional(),
  tpLow: z.number().optional(),
  tpHigh: z.number().optional(),
  samplesText: z.string().optional(),
  focusIdx: z.number().int().optional(),
  monthDeltas: z.string().optional(),
  featuresText: z.string().optional(),
  numTrials: z.number().int().optional(),
})

export const Route = createFileRoute('/multi-feature')({
  validateSearch: (search) =>
    multiFeatureSearchSchema.parse({
      startDate: parseSearchString(search.startDate),
      targetDate: parseSearchString(search.targetDate),
      targetLikelihood: parseSearchNumber(search.targetLikelihood),
      splitLow: parseSearchNumber(search.splitLow),
      splitHigh: parseSearchNumber(search.splitHigh),
      durationIdx: parseSearchInteger(search.durationIdx),
      throughputMode:
        search.throughputMode === 'estimate' || search.throughputMode === 'data'
          ? search.throughputMode
          : undefined,
      tpLow: parseSearchNumber(search.tpLow),
      tpHigh: parseSearchNumber(search.tpHigh),
      samplesText:
        parseSearchString(search.samplesText) ??
        normalizeSamplesInput(parseSearchString(search.samples)),
      focusIdx:
        parseSearchInteger(search.focusIdx) ??
        focusIndexFromValue(parseSearchNumber(search.focus)),
      monthDeltas: encodeMonthDeltas(
        parseSearchMonthMultipliers(search.monthDeltas ?? search.monthMultipliers),
      ),
      featuresText: encodeFeatureRows(
        parseSearchFeatures(search.featuresText ?? search.features),
      ),
      numTrials: parseSearchInteger(search.numTrials),
    }),
  component: MultiFeatureForecasterPage,
})

/* ── Setting tables ──────────────────────────────────────────────────────── */

const DURATION_OPTIONS = [
  { label: 'Week', days: 7 },
  { label: 'Sprint (2 week)', days: 14 },
  { label: 'Sprint (3 week)', days: 21 },
  { label: 'Sprint (4 week)', days: 28 },
]

const COMPLEXITY_OPTIONS: {
  label: string
  lowMult: number
  highMult: number
}[] = [
  { label: 'Clear and understood', lowMult: 1, highMult: 1 },
  { label: 'Somewhat understood', lowMult: 1, highMult: 1.5 },
  { label: 'Not really understood yet', lowMult: 1.5, highMult: 2 },
  { label: 'Very unclear', lowMult: 1.75, highMult: 3 },
]

const FOCUS_OPTIONS = [
  { label: '100% (only this work)', value: 1 },
  { label: '75% (mostly this work)', value: 0.75 },
  { label: '50% (half this work)', value: 0.5 },
  { label: '25% (some of this work)', value: 0.25 },
]

const MONTHS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
]

const DEFAULT_SAMPLES_TEXT = '1\n3\n5\n3\n7\n8'
const DEFAULT_MONTH_MULTIPLIERS = Array(12).fill(1)

function makeDefaultFeatures(): FeatureRow[] {
  return [
    { name: 'Feature 1', storyLow: 5, storyHigh: 10, complexityIdx: 0 },
    { name: 'Feature 2', storyLow: 8, storyHigh: 15, complexityIdx: 0 },
    { name: 'Feature 3', storyLow: 15, storyHigh: 25, complexityIdx: 0 },
    { name: 'Feature 4', storyLow: 20, storyHigh: 30, complexityIdx: 0 },
    { name: 'Feature 5', storyLow: 10, storyHigh: 40, complexityIdx: 0 },
  ]
}

const DEFAULT_FEATURES = makeDefaultFeatures()

function buildExampleSearch(params: Record<string, string>): string {
  const search = new URLSearchParams(params)
  return `?${search.toString()}`
}

const QUERY_PARAM_DOCS = [
  ['startDate, targetDate', 'Forecast window in YYYY-MM-DD format.'],
  ['targetLikelihood', 'Confidence level as a decimal, such as 0.85 for 85%.'],
  ['splitLow, splitHigh', 'Story split-rate range applied before simulation.'],
  ['durationIdx', 'Throughput unit index: 0=week, 1=2 weeks, 2=3 weeks, 3=4 weeks.'],
  ['throughputMode', 'Use estimate or data.'],
  ['tpLow, tpHigh', 'Estimate-mode throughput inputs.'],
  ['samplesText', 'Historical throughput samples, separated by commas or new lines.'],
  ['focusIdx', 'Focus index: 0=100%, 1=75%, 2=50%, 3=25%.'],
  ['monthDeltas', 'Compact month:value pairs for non-default months, such as 7:0.8,8:0.8,12:0.7.'],
  ['featuresText', 'Compact feature rows in the form name~storyLow~storyHigh~complexityIdx, separated by |.'],
  ['numTrials', 'Simulation trials, typically 100-10000.'],
] as const

const QUERY_PARAM_EXAMPLES = [
  {
    title: 'Estimate mode with a prioritized five-feature backlog',
    description:
      'Prefills an estimate-based scenario with five feature rows and seasonal throughput adjustments.',
    search: buildExampleSearch({
      startDate: '2026-04-07',
      targetDate: '2026-07-14',
      targetLikelihood: '0.85',
      splitLow: '1',
      splitHigh: '1.7',
      durationIdx: '0',
      throughputMode: 'estimate',
      tpLow: '4',
      tpHigh: '7',
      focusIdx: '1',
      monthDeltas: '7:0.8,8:0.8,12:0.7',
      featuresText: encodeFeatureRows([
        { name: 'Authentication', storyLow: 5, storyHigh: 9, complexityIdx: 0 },
        { name: 'Billing', storyLow: 8, storyHigh: 13, complexityIdx: 1 },
        { name: 'Reporting', storyLow: 13, storyHigh: 21, complexityIdx: 1 },
        { name: 'Audit Trail', storyLow: 8, storyHigh: 18, complexityIdx: 2 },
        { name: 'Admin Console', storyLow: 21, storyHigh: 34, complexityIdx: 2 },
      ]) ?? '',
      numTrials: '800',
    }),
  },
  {
    title: 'Historical-data mode with three higher-uncertainty features',
    description:
      'Uses throughput samples and feature rows encoded directly in the URL for a shared scenario.',
    search: buildExampleSearch({
      startDate: '2026-05-01',
      targetDate: '2026-08-28',
      targetLikelihood: '0.7',
      splitLow: '1.1',
      splitHigh: '2',
      durationIdx: '1',
      throughputMode: 'data',
      samplesText: '3,4,5,6,4,7,5,6',
      focusIdx: '2',
      monthDeltas: '',
      featuresText: encodeFeatureRows([
        { name: 'API Migration', storyLow: 12, storyHigh: 20, complexityIdx: 1 },
        { name: 'Workflow Rules', storyLow: 15, storyHigh: 28, complexityIdx: 2 },
        { name: 'Partner Integrations', storyLow: 20, storyHigh: 36, complexityIdx: 3 },
      ]) ?? '',
      numTrials: '600',
    }),
  },
] as const

function parseSearchMaybeJson(value: unknown): unknown {
  if (typeof value !== 'string') return value
  try {
    return JSON.parse(value)
  } catch {
    return value
  }
}

function parseSearchNumber(value: unknown): number | undefined {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value)
    if (Number.isFinite(parsed)) return parsed
  }
  return undefined
}

function parseSearchInteger(value: unknown): number | undefined {
  const parsed = parseSearchNumber(value)
  return parsed != null ? Math.trunc(parsed) : undefined
}

function parseSearchString(value: unknown): string | undefined {
  return typeof value === 'string' && value.length > 0 ? value : undefined
}

function decodeSearchToken(value: string): string {
  try {
    return decodeURIComponent(value)
  } catch {
    return value
  }
}

function encodeSearchToken(value: string): string {
  return encodeURIComponent(value)
}

function parseSearchMonthMultipliers(value: unknown): number[] | undefined {
  const parsed = parseSearchMaybeJson(value)
  if (Array.isArray(parsed)) {
    return Array.from({ length: 12 }, (_, index) => parseSearchNumber(parsed[index]) ?? 1)
  }

  if (typeof parsed !== 'string') return undefined

  const monthMultipliers = [...DEFAULT_MONTH_MULTIPLIERS]
  if (parsed.trim() === '') return monthMultipliers

  for (const entry of parsed.split(',').map((part) => part.trim()).filter(Boolean)) {
    const [monthToken, valueToken] = entry.split(':')
    const month = parseSearchInteger(monthToken)
    const multiplier = parseSearchNumber(valueToken)
    if (month == null || multiplier == null || month < 1 || month > 12) continue
    monthMultipliers[month - 1] = multiplier
  }

  return monthMultipliers
}

function encodeMonthDeltas(values: number[] | undefined): string | undefined {
  if (!values || values.length === 0) return undefined

  const encoded = values
    .map((value, index) => (value === 1 ? null : `${index + 1}:${value}`))
    .filter((value): value is string => value != null)
    .join(',')

  return encoded || undefined
}

function parseSearchFeatures(value: unknown): FeatureRow[] | undefined {
  const parsed = parseSearchMaybeJson(value)
  if (Array.isArray(parsed)) {
    const features = parsed
      .map((entry) => {
        const candidate = parseSearchMaybeJson(entry)
        if (!candidate || typeof candidate !== 'object') return null

        const feature = candidate as Record<string, unknown>
        const result = featureSearchSchema.safeParse({
          name: typeof feature.name === 'string' ? feature.name : undefined,
          storyLow: parseSearchInteger(feature.storyLow),
          storyHigh: parseSearchInteger(feature.storyHigh),
          complexityIdx: parseSearchInteger(feature.complexityIdx),
        })

        return result.success ? result.data : null
      })
      .filter((feature): feature is FeatureRow => feature != null)

    return features.length > 0 ? features : undefined
  }

  if (typeof parsed !== 'string' || parsed.trim() === '') return undefined

  const features = parsed
    .split('|')
    .map((row) => row.trim())
    .filter(Boolean)
    .map((row) => {
      const [nameToken = '', storyLowToken = '', storyHighToken = '', complexityToken = ''] = row.split('~')
      const result = featureSearchSchema.safeParse({
        name: decodeSearchToken(nameToken),
        storyLow: parseSearchInteger(storyLowToken),
        storyHigh: parseSearchInteger(storyHighToken),
        complexityIdx: parseSearchInteger(complexityToken),
      })

      return result.success ? result.data : null
    })
    .filter((feature): feature is FeatureRow => feature != null)

  return features.length > 0 ? features : undefined
}

function encodeFeatureRows(features: FeatureRow[] | undefined): string | undefined {
  if (!features || features.length === 0) return undefined
  return features
    .map((feature) => [
      encodeSearchToken(feature.name),
      String(feature.storyLow),
      String(feature.storyHigh),
      String(feature.complexityIdx),
    ].join('~'))
    .join('|')
}

function focusIndexFromValue(value: number | undefined): number | undefined {
  if (value == null) return undefined
  const match = FOCUS_OPTIONS.findIndex((option) => option.value === value)
  return match >= 0 ? match : undefined
}

function normalizeSamplesInput(value: string | undefined): string | undefined {
  if (!value) return undefined
  const normalized = value
    .split(/[\n,]+/)
    .map((sample) => sample.trim())
    .filter(Boolean)
    .join('\n')
  return normalized || undefined
}

function clampIndex(index: number | undefined, max: number): number {
  if (index == null || Number.isNaN(index)) return 0
  return Math.min(Math.max(index, 0), max)
}

function areNumberArraysEqual(left: number[], right: number[]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function areFeaturesEqual(left: FeatureRow[], right: FeatureRow[]): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

/* ── Feature row type ────────────────────────────────────────────────────── */

interface FeatureRow {
  name: string
  storyLow: number
  storyHigh: number
  complexityIdx: number
}

const emptyFeature = (i: number): FeatureRow => ({
  name: `Feature ${i + 1}`,
  storyLow: 0,
  storyHigh: 0,
  complexityIdx: 0,
})

/* ── Page ────────────────────────────────────────────────────────────────── */

function MultiFeatureForecasterPage() {
  const search = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })
  const parsedMonthMultipliers =
    parseSearchMonthMultipliers(search.monthDeltas) ?? DEFAULT_MONTH_MULTIPLIERS
  const parsedFeatures = parseSearchFeatures(search.featuresText) ?? DEFAULT_FEATURES

  // Inputs
  const [startDate, setStartDate] = useState(search.startDate ?? '2025-03-01')
  const [targetDate, setTargetDate] = useState(search.targetDate ?? '2025-06-01')
  const [targetLikelihood, setTargetLikelihood] = useState(
    search.targetLikelihood ?? 0.85,
  )
  const [splitLow, setSplitLow] = useState(search.splitLow ?? 1)
  const [splitHigh, setSplitHigh] = useState(search.splitHigh ?? 2)
  const [durationIdx, setDurationIdx] = useState(
    clampIndex(search.durationIdx, DURATION_OPTIONS.length - 1),
  )
  const [throughputMode, setThroughputMode] = useState<'estimate' | 'data'>(
    search.throughputMode ?? 'estimate',
  )
  const [tpLow, setTpLow] = useState(search.tpLow ?? 5)
  const [tpHigh, setTpHigh] = useState(search.tpHigh ?? 8)
  const [samplesText, setSamplesText] = useState(
    search.samplesText ?? DEFAULT_SAMPLES_TEXT,
  )
  const [focusIdx, setFocusIdx] = useState(
    clampIndex(search.focusIdx, FOCUS_OPTIONS.length - 1),
  )
  const [monthMultipliers, setMonthMultipliers] = useState<number[]>(
    parsedMonthMultipliers,
  )
  const [features, setFeatures] = useState<FeatureRow[]>(parsedFeatures)
  const [numTrials, setNumTrials] = useState(search.numTrials ?? 500)

  // Results
  const [results, setResults] = useState<MultiFeatureResults | null>(null)
  const [running, setRunning] = useState(false)

  const dur = DURATION_OPTIONS[durationIdx]

  // Sync URL → State only when the URL search params change (e.g. external
  // navigation, back/forward).  Functional setState avoids reading stale state
  // and keeps the dep array limited to `search`.
  useEffect(() => {
    const nextStartDate = search.startDate ?? '2025-03-01'
    const nextTargetDate = search.targetDate ?? '2025-06-01'
    const nextTargetLikelihood = search.targetLikelihood ?? 0.85
    const nextSplitLow = search.splitLow ?? 1
    const nextSplitHigh = search.splitHigh ?? 2
    const nextDurationIdx = clampIndex(search.durationIdx, DURATION_OPTIONS.length - 1)
    const nextThroughputMode = search.throughputMode ?? 'estimate'
    const nextTpLow = search.tpLow ?? 5
    const nextTpHigh = search.tpHigh ?? 8
    const nextSamplesText = search.samplesText ?? DEFAULT_SAMPLES_TEXT
    const nextFocusIdx = clampIndex(search.focusIdx, FOCUS_OPTIONS.length - 1)
    const nextMonthMultipliers =
      parseSearchMonthMultipliers(search.monthDeltas) ?? DEFAULT_MONTH_MULTIPLIERS
    const nextFeatures = parseSearchFeatures(search.featuresText) ?? DEFAULT_FEATURES
    const nextNumTrials = search.numTrials ?? 500

    setStartDate(prev => prev === nextStartDate ? prev : nextStartDate)
    setTargetDate(prev => prev === nextTargetDate ? prev : nextTargetDate)
    setTargetLikelihood(prev => prev === nextTargetLikelihood ? prev : nextTargetLikelihood)
    setSplitLow(prev => prev === nextSplitLow ? prev : nextSplitLow)
    setSplitHigh(prev => prev === nextSplitHigh ? prev : nextSplitHigh)
    setDurationIdx(prev => prev === nextDurationIdx ? prev : nextDurationIdx)
    setThroughputMode(prev => prev === nextThroughputMode ? prev : nextThroughputMode)
    setTpLow(prev => prev === nextTpLow ? prev : nextTpLow)
    setTpHigh(prev => prev === nextTpHigh ? prev : nextTpHigh)
    setSamplesText(prev => prev === nextSamplesText ? prev : nextSamplesText)
    setFocusIdx(prev => prev === nextFocusIdx ? prev : nextFocusIdx)
    setMonthMultipliers(prev => areNumberArraysEqual(prev, nextMonthMultipliers) ? prev : nextMonthMultipliers)
    setFeatures(prev => areFeaturesEqual(prev, nextFeatures) ? prev : nextFeatures)
    setNumTrials(prev => prev === nextNumTrials ? prev : nextNumTrials)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search])

  useEffect(() => {
    const currentSearch = {
      startDate: search.startDate,
      targetDate: search.targetDate,
      targetLikelihood: search.targetLikelihood,
      splitLow: search.splitLow,
      splitHigh: search.splitHigh,
      durationIdx: search.durationIdx,
      throughputMode: search.throughputMode,
      tpLow: search.tpLow,
      tpHigh: search.tpHigh,
      samplesText: search.samplesText,
      focusIdx: search.focusIdx,
      monthDeltas: search.monthDeltas,
      featuresText: search.featuresText,
      numTrials: search.numTrials,
    }

    const nextSearch = {
      startDate: startDate || undefined,
      targetDate: targetDate || undefined,
      targetLikelihood,
      splitLow,
      splitHigh,
      durationIdx,
      throughputMode,
      tpLow,
      tpHigh,
      samplesText,
      focusIdx,
      monthDeltas: encodeMonthDeltas(monthMultipliers),
      featuresText: encodeFeatureRows(features),
      numTrials,
    }

    const searchChanged = JSON.stringify(currentSearch) !== JSON.stringify(nextSearch)
    if (!searchChanged) return

    void navigate({ search: nextSearch, replace: true })
  }, [
    durationIdx,
    features,
    focusIdx,
    monthMultipliers,
    navigate,
    numTrials,
    samplesText,
    search,
    splitHigh,
    splitLow,
    startDate,
    targetDate,
    targetLikelihood,
    throughputMode,
    tpHigh,
    tpLow,
  ])

  const splitError =
    splitLow > splitHigh ? 'Low split must be ≤ high split.' : ''
  const tpError =
    throughputMode === 'estimate' && tpLow > tpHigh
      ? 'Low must be ≤ high.'
      : ''
  const hasActiveFeatures = features.some((f) => f.storyLow > 0 || f.storyHigh > 0)
  const canRun = !splitError && !tpError && startDate && targetDate && hasActiveFeatures

  /* ── Auto-run simulation on input change ───────────────────── */
  useEffect(() => {
    if (!canRun) {
      setResults(null)
      return
    }
    setRunning(true)
    const timer = setTimeout(() => {
      const dur = DURATION_OPTIONS[durationIdx]
      const focus = FOCUS_OPTIONS[focusIdx]
      const samples = samplesText
        .split(/[\n,]+/)
        .map((s) => Number(s.trim()))
        .filter((n) => !isNaN(n) && n > 0)

      const activeFeatures: Feature[] = features
        .filter((f) => f.storyLow > 0 || f.storyHigh > 0)
        .map((f) => {
          const c = COMPLEXITY_OPTIONS[f.complexityIdx]
          return {
            name: f.name,
            storyCountLow: f.storyLow,
            storyCountHigh: f.storyHigh,
            complexityLowMultiplier: c.lowMult,
            complexityHighMultiplier: c.highMult,
          }
        })

      const input: MultiFeatureInputs = {
        startDate,
        targetDate,
        targetLikelihood,
        splitRateLow: splitLow,
        splitRateHigh: splitHigh,
        throughputMode,
        throughputLow: tpLow,
        throughputHigh: tpHigh,
        samples,
        focusPercentage: focus.value,
        daysPerUnit: dur.days,
        monthlyAdjustments: monthMultipliers,
        features: activeFeatures,
        numTrials,
        maxPeriods: 104,
      }

      requestAnimationFrame(() => {
        const res = runMultiFeatureForecaster(input)
        setResults(res)
        setRunning(false)
      })
    }, 300)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    canRun,
    startDate,
    targetDate,
    targetLikelihood,
    splitLow,
    splitHigh,
    throughputMode,
    tpLow,
    tpHigh,
    samplesText,
    focusIdx,
    durationIdx,
    monthMultipliers,
    features,
    numTrials,
  ])

  const updateFeature = (
    idx: number,
    field: keyof FeatureRow,
    value: string | number,
  ) => {
    setFeatures((prev) => {
      const next = [...prev]
      next[idx] = { ...next[idx], [field]: value }
      return next
    })
  }

  const updateMonth = (idx: number, value: number) => {
    setMonthMultipliers((prev) => {
      const next = [...prev]
      next[idx] = value
      return next
    })
  }

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-8 sm:px-6 lg:px-8">
      <section className="island-shell rise-in rounded-[2rem] px-6 py-10 sm:px-10 sm:py-14">
        <h1 className="display-title mb-2 text-3xl font-bold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Multiple Feature Cut Line Forecaster
        </h1>
        <p className="mb-8 max-w-2xl text-sm text-[var(--sea-ink-soft)]">
          Monte Carlo simulation for multiple features in priority order.
          Determines which features can be completed by a target date and
          forecasts completion dates at a chosen confidence level.
        </p>

        <div className="space-y-8">
          {/* ── Top inputs row ─────────────────────────────────────────── */}
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <Field label="1. Start Date">
              <input
                type="date"
                className="field-input"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </Field>
            <Field label="2. Target Date">
              <input
                type="date"
                className="field-input"
                value={targetDate}
                onChange={(e) => setTargetDate(e.target.value)}
              />
            </Field>
            <Field label="3. Confidence Level">
              <select
                className="field-input"
                value={targetLikelihood}
                onChange={(e) => setTargetLikelihood(Number(e.target.value))}
              >
                {[0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5].map(
                  (v) => (
                    <option key={v} value={v}>
                      {(v * 100).toFixed(0)}%
                    </option>
                  ),
                )}
              </select>
            </Field>
            <Field label="Simulation trials">
              <NumberInput
                value={numTrials}
                onChange={setNumTrials}
                min={100}
                max={10000}
                step={100}
              />
            </Field>
          </div>

          {/* ── Split / Throughput / Focus row ─────────────────────────── */}
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <fieldset className="space-y-2">
              <legend className="field-legend">4. Story split rate</legend>
              <div className="flex gap-4">
                <Field label="Low" inline>
                  <NumberInput
                    value={splitLow}
                    onChange={setSplitLow}
                    min={1}
                    step={0.1}
                  />
                </Field>
                <Field label="High" inline>
                  <NumberInput
                    value={splitHigh}
                    onChange={setSplitHigh}
                    min={1}
                    step={0.1}
                  />
                </Field>
              </div>
              {splitError && (
                <p className="text-xs text-red-600">{splitError}</p>
              )}
            </fieldset>

            <fieldset className="space-y-2">
              <legend className="field-legend">5. Throughput</legend>
              <div className="flex flex-wrap gap-4">
                <Field label="Unit" inline>
                  <select
                    className="field-input"
                    value={durationIdx}
                    onChange={(e) => setDurationIdx(Number(e.target.value))}
                  >
                    {DURATION_OPTIONS.map((d, i) => (
                      <option key={d.label} value={i}>
                        {d.label}
                      </option>
                    ))}
                  </select>
                </Field>
                <Field label="Source" inline>
                  <select
                    className="field-input"
                    value={throughputMode}
                    onChange={(e) =>
                      setThroughputMode(e.target.value as 'estimate' | 'data')
                    }
                  >
                    <option value="estimate">Estimate</option>
                    <option value="data">Historical Data</option>
                  </select>
                </Field>
              </div>
              {throughputMode === 'estimate' ? (
                <div className="flex gap-4">
                  <Field label="Worst" inline>
                    <NumberInput value={tpLow} onChange={setTpLow} min={0} />
                  </Field>
                  <Field label="Best" inline>
                    <NumberInput value={tpHigh} onChange={setTpHigh} min={0} />
                  </Field>
                </div>
              ) : (
                <Field label="Samples (one per line)">
                  <textarea
                    className="field-input min-h-[4rem] font-mono text-xs"
                    value={samplesText}
                    onChange={(e) => setSamplesText(e.target.value)}
                  />
                </Field>
              )}
              {tpError && <p className="text-xs text-red-600">{tpError}</p>}
              <div className="flex items-center gap-3">
                <CopyLinkButton />
                <span className="text-xs text-[var(--sea-ink-soft)]">
                  The current inputs are stored in the URL for sharing.
                </span>
              </div>
            </fieldset>

            <Field label="Team focus">
              <select
                className="field-input"
                value={focusIdx}
                onChange={(e) => setFocusIdx(Number(e.target.value))}
              >
                {FOCUS_OPTIONS.map((f, i) => (
                  <option key={f.label} value={i}>
                    {f.label}
                  </option>
                ))}
              </select>
            </Field>
          </div>

          {/* ── Features table ─────────────────────────────────────────── */}
          <fieldset className="space-y-2">
            <legend className="field-legend">
              6. Features / Epics (in priority order)
            </legend>
            <div className="rounded-xl border border-[var(--line)] overflow-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                    <th className="w-8 px-2 py-2 text-center">#</th>
                    <th className="px-2 py-2 text-left">Feature Name</th>
                    <th className="px-2 py-2 text-right">Low Stories</th>
                    <th className="px-2 py-2 text-right">High Stories</th>
                    <th className="px-2 py-2 text-left">Complexity</th>
                    {results && (
                      <>
                        <th className="px-2 py-2 text-right">
                          Complete by {dur.label}
                        </th>
                        <th className="px-2 py-2 text-right">
                          Forecast Date ({(targetLikelihood * 100).toFixed(0)}%
                          CI)
                        </th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {features.map((f, i) => {
                    const res = results?.features[i]
                    return (
                      <tr
                        key={i}
                        className="border-b border-[var(--line)] last:border-0"
                      >
                        <td className="px-2 py-1 text-center text-[var(--sea-ink-soft)]">
                          {i + 1}
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="text"
                            className="field-input w-full"
                            value={f.name}
                            onChange={(e) =>
                              updateFeature(i, 'name', e.target.value)
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-20 text-right"
                            value={f.storyLow || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'storyLow',
                                Number(e.target.value),
                              )
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <input
                            type="number"
                            className="field-input w-20 text-right"
                            value={f.storyHigh || ''}
                            min={0}
                            placeholder="0"
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'storyHigh',
                                Number(e.target.value),
                              )
                            }
                          />
                        </td>
                        <td className="px-2 py-1">
                          <select
                            className="field-input"
                            value={f.complexityIdx}
                            onChange={(e) =>
                              updateFeature(
                                i,
                                'complexityIdx',
                                Number(e.target.value),
                              )
                            }
                          >
                            {COMPLEXITY_OPTIONS.map((c, ci) => (
                              <option key={c.label} value={ci}>
                                {c.label}
                              </option>
                            ))}
                          </select>
                        </td>
                        {results && res && (
                          <>
                            <td className="px-2 py-1 text-right font-mono">
                              {res.intervalsAtLikelihood ?? '—'}
                            </td>
                            <td className="px-2 py-1 text-right">
                              <span
                                className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${statusClasses(res.status)}`}
                              >
                                {res.forecastDate ?? '—'}
                              </span>
                            </td>
                          </>
                        )}
                        {results && !res && (
                          <>
                            <td className="px-2 py-1 text-right text-[var(--sea-ink-soft)]">
                              —
                            </td>
                            <td className="px-2 py-1 text-right text-[var(--sea-ink-soft)]">
                              —
                            </td>
                          </>
                        )}
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
            <button
              type="button"
              className="text-xs text-[var(--lagoon-deep)] hover:underline"
              onClick={() =>
                setFeatures((prev) => [...prev, emptyFeature(prev.length)])
              }
            >
              + Add feature
            </button>
          </fieldset>

          {/* ── Monthly Throughput Adjustments ──────────────────────────── */}
          <details className="space-y-2">
            <summary className="field-legend cursor-pointer">
              7. Monthly Throughput Adjustments (optional)
            </summary>
            <p className="text-xs text-[var(--sea-ink-soft)]">
              Multiply throughput by a factor each month (e.g., 0.5 for December
              holidays).
            </p>
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 lg:grid-cols-6">
              {MONTHS.map((m, i) => (
                <Field key={m} label={m} inline>
                  <NumberInput
                    value={monthMultipliers[i]}
                    onChange={(v) => updateMonth(i, v)}
                    min={0}
                    step={0.1}
                  />
                </Field>
              ))}
            </div>
          </details>

          {/* ── Status ───────────────────────────────────────────────── */}
          {!canRun && (
            <div className="flex items-center gap-3 rounded-2xl border-2 border-dashed border-amber-400/50 bg-amber-50/60 p-4 dark:border-amber-500/30 dark:bg-amber-950/30">
              <svg className="h-5 w-5 flex-shrink-0 text-amber-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
                <line x1="12" y1="9" x2="12" y2="13" />
                <line x1="12" y1="17" x2="12.01" y2="17" />
              </svg>
              <div>
                <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">Missing required inputs</p>
                <p className="mt-1 text-xs text-amber-700 dark:text-amber-300">
                  {splitError || tpError || (!startDate && 'Start date is required.') || (!targetDate && 'Target date is required.') || (!hasActiveFeatures && 'At least one feature needs story counts > 0.')}
                </p>
              </div>
            </div>
          )}
          {running && (
            <p className="text-sm font-medium text-[var(--lagoon-deep)] animate-pulse">Simulating…</p>
          )}

          {/* ── Results legend ──────────────────────────────────────────── */}
          {results && (
            <div className="island-shell rounded-2xl p-5">
              <h2 className="mb-3 text-base font-semibold text-[var(--sea-ink)]">
                Results Summary
              </h2>
              <p className="mb-3 text-xs text-[var(--sea-ink-soft)]">
                Start: {new Date(startDate).toLocaleDateString()} · Target:{' '}
                {new Date(targetDate).toLocaleDateString()} ·{' '}
                {(targetLikelihood * 100).toFixed(0)}% confidence · {numTrials}{' '}
                trials
              </p>
              <div className="flex gap-3 text-xs">
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-blue-200 dark:bg-blue-800" />
                  On or before target
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-amber-200 dark:bg-amber-800" />
                  Within 1 {dur.label}
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <span className="inline-block h-3 w-3 rounded-full bg-red-200 dark:bg-red-800" />
                  After target
                </span>
              </div>

              {/* Gantt-style visual */}
              <div className="mt-4 space-y-1">
                {results.features.map((f, i) => (
                  <div key={i} className="flex items-center gap-2 text-xs">
                    <span className="w-28 truncate text-[var(--sea-ink-soft)]">
                      {f.name}
                    </span>
                    <div className="relative flex-1 h-5 rounded bg-[var(--line)]">
                      <GanttBar
                        intervals={f.intervalsAtLikelihood ?? 0}
                        maxIntervals={Math.max(
                          ...results.features.map(
                            (x) => x.intervalsAtLikelihood ?? 1,
                          ),
                        )}
                        status={f.status}
                      />
                    </div>
                    <span className="w-20 text-right text-[var(--sea-ink-soft)]">
                      {f.forecastDate}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="border-t border-[var(--line)] pt-6">
            <details className="space-y-4">
              <summary className="field-legend cursor-pointer text-sm">
                URL Parameters (advanced)
              </summary>
              <p className="max-w-3xl text-xs text-[var(--sea-ink-soft)]">
                Append query parameters to prefill the form for testing or shared
                scenarios. Complex values such as feature rows and monthly
                adjustments are stored in compact encoded strings instead of raw
                JSON.
              </p>

              <div className="rounded-2xl border border-[var(--line)] overflow-auto">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-[var(--line)] bg-[var(--header-bg)]">
                      <th className="px-3 py-2 text-left font-semibold">Parameter</th>
                      <th className="px-3 py-2 text-left font-semibold">Meaning</th>
                    </tr>
                  </thead>
                  <tbody>
                    {QUERY_PARAM_DOCS.map(([name, description]) => (
                      <tr key={name} className="border-b border-[var(--line)] last:border-0">
                        <td className="px-3 py-2 font-mono text-[var(--sea-ink)]">
                          {name}
                        </td>
                        <td className="px-3 py-2 text-[var(--sea-ink-soft)]">
                          {description}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="grid gap-4 lg:grid-cols-2">
                {QUERY_PARAM_EXAMPLES.map((example) => (
                  <div
                    key={example.title}
                    className="rounded-2xl border border-[var(--line)] bg-[var(--surface-strong)] p-4"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h2 className="text-sm font-semibold text-[var(--sea-ink)]">
                          {example.title}
                        </h2>
                        <p className="mt-1 text-xs text-[var(--sea-ink-soft)]">
                          {example.description}
                        </p>
                      </div>
                      <a
                        href={`/multi-feature${example.search}`}
                        className="rounded-full border border-[var(--line)] px-3 py-1 text-xs font-semibold text-[var(--lagoon-deep)] transition hover:border-[var(--lagoon)] hover:text-[var(--sea-ink)]"
                      >
                        Open example
                      </a>
                    </div>
                    <p className="mt-3 break-all rounded-xl bg-[var(--header-bg)] px-3 py-2 font-mono text-[11px] text-[var(--sea-ink-soft)]">
                      /multi-feature{example.search}
                    </p>
                  </div>
                ))}
              </div>
            </details>
          </div>
        </div>
      </section>
    </main>
  )
}

/* ── Sub-components ──────────────────────────────────────────────────────── */

function statusClasses(status: 1 | 2 | 3): string {
  switch (status) {
    case 1:
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300'
    case 2:
      return 'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300'
    case 3:
      return 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300'
  }
}

function GanttBar({
  intervals,
  maxIntervals,
  status,
}: {
  intervals: number
  maxIntervals: number
  status: 1 | 2 | 3
}) {
  const pct = maxIntervals > 0 ? (intervals / maxIntervals) * 100 : 0
  const bg =
    status === 1
      ? 'bg-blue-400 dark:bg-blue-600'
      : status === 2
        ? 'bg-amber-400 dark:bg-amber-600'
        : 'bg-red-400 dark:bg-red-600'

  return (
    <div
      className={`absolute inset-y-0 left-0 rounded ${bg}`}
      style={{ width: `${Math.max(pct, 2)}%` }}
    />
  )
}
