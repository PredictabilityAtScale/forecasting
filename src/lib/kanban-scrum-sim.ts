export type SimulationKind = 'kanban' | 'scrum'

export interface SimulationExampleFile {
  id: string
  title: string
  group: string
  section: string
  type: SimulationKind
  path: string
  source: string
  metadata: {
    name?: string
    locale?: string
    example?: string
  }
}

export interface SimColumnOverride {
  columnId: number
  estimateLowBound?: number
  estimateHighBound?: number
  skipPercentage?: number
}

export interface SimWorkItemTemplate {
  id: string
  name: string
  count: number
  completed: boolean
  deliverableOrder?: number
  deliverableSkipPercentage?: number
  dueDate?: string
  order: number
  estimateLowBound?: number
  estimateHighBound?: number
  valueLowBound: number
  valueHighBound: number
  percentageLowBound: number
  percentageHighBound: number
  initialColumn?: number
  deliverable?: string
  preRequisiteDeliverables: string[]
  earliestStartDate?: string
  classOfService?: string
  columnOverrides: SimColumnOverride[]
}

export type SimOccurrenceType = 'count' | 'percentage' | 'points'
export type SimAggregationValue = 'Average' | 'Median' | 'Fifth' | 'NinetyFifth' | 'Max' | 'Min'

export type SimPullOrder = 'randomAfterOrdering' | 'random' | 'indexSequence' | 'FIFO' | 'FIFOStrict'

export interface SimBacklog {
  type: 'simple' | 'custom'
  nameFormat?: string
  simpleCount: number
  shuffle: boolean
  items: SimWorkItemTemplate[]
}

export interface SimColumn {
  id: number
  name: string
  estimateLowBound: number
  estimateHighBound: number
  wipLimit: number
  displayWidth: number
  isBuffer: boolean
  replenishInterval?: number
  completeInterval?: number
  skipPercentage: number
}

export interface SimForecastDate {
  startDate?: string
  workDays: number[]
  workDaysPerIteration: number
  intervalsToOneDay: number
  costPerDay: number
  targetLikelihood: number
  targetDate?: string
  revenue: number
  revenueUnit: 'day' | 'week' | 'month' | 'year'
  actuals: { date: string; count: number; annotation?: string }[]
  excludedDates: string[]
}

export interface SimExecute {
  simulationType: SimulationKind
  dateFormat: string
  deliverables: string[]
  completePercentage: number
  activePositionsCompletePercentage: number
  limitIntervalsTo: number
  monteCarloCycles: number
  aggregationValue: SimAggregationValue
  sensitivityCycles: number
  sensitivityEstimateMultiplier: number
  sensitivityOccurrenceMultiplier: number
  sensitivityIterationMultiplier: number
  pullOrder: SimPullOrder
  decimalRounding: number
}

export interface SimIteration {
  storyPointsPerIterationLowBound: number
  storyPointsPerIterationHighBound: number
  allowedToOverAllocate: boolean
}

export interface SimThroughput {
  itemsPerIterationLowBound: number
  itemsPerIterationHighBound: number
}

export interface SimDefect {
  name: string
  columnId: number
  startsInColumnId?: number
  occurrenceLowBound: number
  occurrenceHighBound: number
  estimateLowBound: number
  estimateHighBound: number
  count: number
  columnOverrides: SimColumnOverride[]
}

export interface SimBlockingEvent {
  name: string
  columnId: number
  occurrenceLowBound: number
  occurrenceHighBound: number
  estimateLowBound: number
  estimateHighBound: number
  occurrenceType: SimOccurrenceType
  scale: number
  phases: string[]
  targetCustomBacklog?: string
  targetDeliverable?: string
  blockWork: boolean
  blockDefects: boolean
  blockAddedScope: boolean
}

export interface SimAddedScope {
  name: string
  occurrenceLowBound: number
  occurrenceHighBound: number
  estimateLowBound: number
  estimateHighBound: number
  count: number
}

export interface SimPhaseColumnOverride {
  columnId: number
  wipLimit: number
}

export interface SimPhase {
  name: string
  start: number
  end: number
  estimateMultiplier: number
  occurrenceMultiplier: number
  iterationMultiplier: number
  costPerDay: number
  columns: SimPhaseColumnOverride[]
}

export interface SimPhases {
  unit: 'percentage' | 'interval' | 'iteration'
  phases: SimPhase[]
}

export interface SimClassOfService {
  name: string
  order: number
  isDefault: boolean
  violateWip: boolean
  skipPercentage: number
  maximumAllowedOnBoard: number
  columnOverrides: SimColumnOverride[]
}

export interface SimModel {
  name: string
  locale: string
  example?: string
  execute: SimExecute
  setup: {
    backlog: SimBacklog
    columns: SimColumn[]
    iteration?: SimIteration
    throughput?: SimThroughput
    forecastDate?: SimForecastDate
    defects: SimDefect[]
    blockingEvents: SimBlockingEvent[]
    addedScopes: SimAddedScope[]
    phases?: SimPhases
    classOfServices: SimClassOfService[]
  }
  warnings: string[]
}

export interface BoardCard {
  id: string
  label: string
  deliverable?: string
  kind: 'story' | 'defect' | 'addedScope'
  status?: 'active' | 'blocked' | 'queued' | 'done'
  isBlocked?: boolean
  blockerLabel?: string
}

export interface BoardSnapshot {
  step: number
  label: string
  backlogCount: number
  doneCount: number
  activePhase?: string
  columns: {
    id: string
    label: string
    wipLimit?: number
    cards: BoardCard[]
  }[]
}

export interface MonteCarloPercentile {
  likelihood: number
  steps: number
  date: string | null
}

export interface MonteCarloResult {
  averageSteps: number
  medianSteps: number
  minSteps: number
  maxSteps: number
  standardDeviation: number
  percentileSteps: MonteCarloPercentile[]
  histogram: { step: number; count: number }[]
  rawSteps: number[]
  summarySteps: number
}

export interface SensitivityResult {
  baselineAverageSteps: number
  tests: {
    name: string
    type: string
    averageSteps: number
    deltaSteps: number
    deltaPercent: number
  }[]
}

export interface SimulationRunResult {
  kind: SimulationKind
  snapshots: BoardSnapshot[]
  totalSteps: number
  completedItems: number
  completionDate: string | null
  totalCost: number
  costOfDelay: number
  valueDeliveredByStep: { step: number; cumulativeValue: number }[]
  cumulativeFlow: Array<{ step: number; backlog: number; done: number; columns: Record<string, number> }>
}

const RAW_EXAMPLES: Record<string, string> = import.meta.glob(
  '../../KanbanAndScrumSim/FocusedObjective.KanbanAndScrumSim/**/*.[sS][iI][mM][mM][lL]',
  {
    eager: true,
    query: '?raw',
    import: 'default',
  },
)

const WORKDAY_LOOKUP: Record<string, number> = {
  sunday: 0,
  monday: 1,
  tuesday: 2,
  wednesday: 3,
  thursday: 4,
  friday: 5,
  saturday: 6,
}

const DEFAULT_WORK_DAYS = [1, 2, 3, 4, 5]

export function loadSimulationExamples(): SimulationExampleFile[] {
  return Object.entries(RAW_EXAMPLES)
    .filter(([path]) => !path.replace(/\\/g, '/').toLowerCase().includes('snippets'))
    .map(([path, source]) => {
      const normalized = path.replace(/\\/g, '/')
      const relative = normalized.split('FocusedObjective.KanbanAndScrumSim/')[1] ?? normalized
      const segments = relative.split('/')
      const displaySegments = segments.map(stripOrderingPrefix)
      const title = stripOrderingPrefix(segments.at(-1)?.replace(/\.[^.]+$/, '') ?? relative)
      const metadata = extractExampleMetadata(source)
      const type = segments[0]?.toLowerCase().includes('scrum') ? 'scrum' : 'kanban'
      return {
        id: relative.toLowerCase(),
        title: metadata.name || title,
        group: displaySegments[0] || 'Examples',
        section: displaySegments.slice(1, -1).join(' / ') || 'Root',
        type,
        path: relative,
        source,
        metadata,
      }
    })
    .sort((a, b) => a.path.localeCompare(b.path))
}

function stripOrderingPrefix(label: string) {
  const cleaned = label.replace(/^\d+\s*(?:--|-|–)\s*/, '').trim()
  return cleaned || label
}

function extractExampleMetadata(source: string) {
  const simulationMatch = source.match(/<simulation\b[^>]*>/i)?.[0] ?? ''
  const nameMatch = simulationMatch.match(/\bname\s*=\s*"([^"]*)"/i)
  const localeMatch = simulationMatch.match(/\blocale\s*=\s*"([^"]*)"/i)
  const exampleMatch = source.match(/<example>([\s\S]*?)<\/example>/i)

  return {
    name: nameMatch?.[1]?.trim(),
    locale: localeMatch?.[1]?.trim(),
    example: exampleMatch?.[1]?.replace(/\s+/g, ' ').trim(),
  }
}

export function parseSimMl(xmlSource: string): SimModel {
  const warnings: string[] = []
  const expanded = preprocessSimMl(xmlSource, warnings)
  const doc = new DOMParser().parseFromString(expanded, 'application/xml')
  const root = doc.querySelector('simulation')
  const parserError = doc.querySelector('parsererror')

  if (parserError || !root) {
    throw new Error('Unable to parse SimML. Check the XML for malformed tags or attributes.')
  }

  const execute = root.querySelector(':scope > execute')
  const setup = root.querySelector(':scope > setup')

  if (!execute || !setup) {
    throw new Error('SimML requires both <execute> and <setup> sections.')
  }

  noteUnsupported(root, warnings)

  const simulationType = (readAttr(execute, 'type') || 'kanban').toLowerCase() === 'scrum'
    ? 'scrum'
    : 'kanban'

  const backlog = parseBacklog(setup, simulationType)
  const columns = simulationType === 'kanban' ? parseColumns(setup) : []
  const iteration = simulationType === 'scrum' ? parseIteration(setup) : undefined
  const throughput = simulationType === 'scrum' ? parseThroughput(setup) : undefined
  const forecastDate = parseForecastDate(setup)
  const defects = parseDefects(setup)
  const blockingEvents = parseBlockingEvents(setup)
  const addedScopes = parseAddedScopes(setup)
  const phases = parsePhases(setup)
  const classOfServices = parseClassOfServices(setup)

  if (simulationType === 'kanban' && columns.length === 0) {
    throw new Error('Kanban simulations require at least one <column>.')
  }

  if (simulationType === 'scrum' && !iteration && !throughput) {
    throw new Error('Scrum simulations require an <iteration> or <throughput> definition.')
  }

  return {
    name: readAttr(root, 'name') || 'Untitled simulation',
    locale: readAttr(root, 'locale') || 'en-US',
    example: readChildText(root, 'example'),
    execute: {
      simulationType,
      dateFormat: readAttr(execute, 'dateFormat') || 'yyyyMMdd',
      deliverables: (readAttr(execute, 'deliverables') || '')
        .split(/[|,]/)
        .map((value) => value.trim())
        .filter(Boolean),
      completePercentage: readNumber(execute, 'completePercentage', 100),
      activePositionsCompletePercentage: readNumber(execute, 'activePositionsCompletePercentage', 0),
      limitIntervalsTo: readNumber(execute, 'limitIntervalsTo', 9000),
      monteCarloCycles: readNumber(execute.querySelector(':scope > monteCarlo'), 'cycles', 500),
      aggregationValue: parseAggregationValue(readAttr(execute.querySelector(':scope > monteCarlo'), 'aggregationValue')),
      sensitivityCycles: readNumber(execute.querySelector(':scope > sensitivity'), 'cycles', 300),
      sensitivityEstimateMultiplier: readNumber(
        execute.querySelector(':scope > sensitivity'),
        'estimateMultiplier',
        1.2,
      ),
      sensitivityOccurrenceMultiplier: readNumber(
        execute.querySelector(':scope > sensitivity'),
        'occurrenceMultiplier',
        1.2,
      ),
      sensitivityIterationMultiplier: readNumber(
        execute.querySelector(':scope > sensitivity'),
        'iterationMultiplier',
        1.15,
      ),
      pullOrder: parsePullOrder(readAttr(execute, 'pullOrder')),
      decimalRounding: Math.max(0, Math.round(readNumber(execute, 'decimalRounding', 3))),
    },
    setup: {
      backlog,
      columns,
      iteration,
      throughput,
      forecastDate,
      defects,
      blockingEvents,
      addedScopes,
      phases,
      classOfServices,
    },
    warnings,
  }
}

function parseBacklog(setup: Element, simulationType: SimulationKind): SimBacklog {
  const backlog = setup.querySelector(':scope > backlog')
  if (!backlog) {
    throw new Error('A <backlog> definition is required.')
  }

  const backlogType = (readAttr(backlog, 'type') || 'simple').toLowerCase() === 'custom'
    ? 'custom'
    : 'simple'

  const shuffle = readBoolean(backlog, 'shuffle', true)
  const nameFormat = readAttr(backlog, 'nameFormat') || undefined

  if (backlogType === 'simple') {
    const count = readNumber(backlog, 'simpleCount', 0)
    return {
      type: 'simple',
      simpleCount: count,
      nameFormat,
      shuffle,
      items: Array.from({ length: count }, (_, index) => ({
        id: `simple-${index + 1}`,
        name: `Story ${index + 1}`,
        count: 1,
        completed: false,
        order: index + 1,
        valueLowBound: 0,
        valueHighBound: 0,
        percentageLowBound: 0,
        percentageHighBound: 100,
        preRequisiteDeliverables: [],
        columnOverrides: [],
      })),
    }
  }

  const items: SimWorkItemTemplate[] = []
  let sequence = 0

  const pushItem = (
    node: Element,
    context?: {
      deliverable?: string
      deliverableOrder?: number
      deliverableSkipPercentage?: number
      preRequisiteDeliverables: string[]
      earliestStartDate?: string
      dueDate?: string
    },
  ) => {
    const count = Math.max(1, readNumber(node, 'count', 1))
    items.push({
      id: `${context?.deliverable || 'backlog'}-${sequence++}`,
      name: readAttr(node, 'name') || node.textContent.trim() || `Item ${sequence}`,
      count,
      completed: readBoolean(node, 'completed', false),
      deliverableOrder: context?.deliverableOrder,
      deliverableSkipPercentage: context?.deliverableSkipPercentage,
      dueDate: readAttr(node, 'dueDate') || context?.dueDate,
      order: readNumber(node, 'order', Number.MAX_SAFE_INTEGER),
      estimateLowBound: readOptionalNumber(node, 'estimateLowBound'),
      estimateHighBound: readOptionalNumber(node, 'estimateHighBound'),
      valueLowBound: readNumber(node, 'valueLowBound', 0),
      valueHighBound: readNumber(node, 'valueHighBound', 0),
      percentageLowBound: readNumber(node, 'percentageLowBound', 0),
      percentageHighBound: readNumber(node, 'percentageHighBound', 100),
      initialColumn: readOptionalNumber(node, 'initialColumn'),
      deliverable: context?.deliverable,
      preRequisiteDeliverables: context?.preRequisiteDeliverables ?? [],
      earliestStartDate: context?.earliestStartDate,
      classOfService: readAttr(node, 'classOfService') || undefined,
      columnOverrides: Array.from(node.querySelectorAll(':scope > column')).map((column) => ({
        columnId: readNumber(column, 'id', -1),
        estimateLowBound: readOptionalNumber(column, 'estimateLowBound'),
        estimateHighBound: readOptionalNumber(column, 'estimateHighBound'),
        skipPercentage: readOptionalNumber(column, 'skipPercentage'),
      })),
    })
  }

  Array.from(backlog.querySelectorAll(':scope > custom')).forEach((node) =>
    pushItem(node, { preRequisiteDeliverables: [] }),
  )
  Array.from(backlog.querySelectorAll(':scope > deliverable')).forEach((deliverable) => {
    const name = readAttr(deliverable, 'name') || 'Deliverable'
    const deliverableOrder = readOptionalNumber(deliverable, 'order')
    const skipPct = readNumber(deliverable, 'skipPercentage', 0)
    const earliestStartDate = readAttr(deliverable, 'earliestStartDate') || undefined
    const dueDate = readAttr(deliverable, 'dueDate') || undefined
    const preRequisiteDeliverables = (readAttr(deliverable, 'preRequisiteDeliverables') || '')
      .split('|')
      .map((part) => part.trim())
      .filter(Boolean)
    Array.from(deliverable.querySelectorAll(':scope > custom')).forEach((node) =>
      pushItem(node, {
        deliverable: name,
        deliverableOrder,
        deliverableSkipPercentage: skipPct,
        preRequisiteDeliverables,
        earliestStartDate,
        dueDate,
      }),
    )
  })

  if (simulationType === 'scrum' && items.length === 0) {
    throw new Error('Scrum custom backlogs need at least one <custom> entry.')
  }

  return {
    type: 'custom',
    simpleCount: 0,
    nameFormat,
    shuffle,
    items: items.sort(compareTemplatePriority),
  }
}

function parseColumns(setup: Element): SimColumn[] {
  return Array.from(setup.querySelectorAll(':scope > columns > column'))
    .map((column) => ({
      id: readNumber(column, 'id', 0),
      name: (column.textContent || '').trim() || `Column ${readNumber(column, 'id', 0)}`,
      estimateLowBound: readNumber(column, 'estimateLowBound', 1),
      estimateHighBound: readNumber(column, 'estimateHighBound', 1),
      wipLimit: readNumber(column, 'wipLimit', 1),
      displayWidth: readNumber(column, 'displayWidth', 1),
      isBuffer: readBoolean(column, 'buffer', false),
      replenishInterval: readOptionalNumber(column, 'replenishInterval'),
      completeInterval: readOptionalNumber(column, 'completeInterval'),
      skipPercentage: readNumber(column, 'skipPercentage', 0),
    }))
    .sort((a, b) => a.id - b.id)
}

function parseIteration(setup: Element): SimIteration | undefined {
  const iteration = setup.querySelector(':scope > iteration')
  if (!iteration) {
    return undefined
  }
  return {
    storyPointsPerIterationLowBound: readNumber(iteration, 'storyPointsPerIterationLowBound', 10),
    storyPointsPerIterationHighBound: readNumber(iteration, 'storyPointsPerIterationHighBound', 20),
    allowedToOverAllocate: readBoolean(iteration, 'allowedToOverAllocate', true),
  }
}

function parseThroughput(setup: Element): SimThroughput | undefined {
  const throughput = setup.querySelector(':scope > throughput')
  if (!throughput) {
    return undefined
  }
  return {
    itemsPerIterationLowBound: readNumber(throughput, 'itemsPerIterationLowBound', 1),
    itemsPerIterationHighBound: readNumber(throughput, 'itemsPerIterationHighBound', 1),
  }
}

function parseForecastDate(setup: Element): SimForecastDate | undefined {
  const node = setup.querySelector(':scope > forecastDate')
  if (!node) {
    return undefined
  }

  const workDaysString = readAttr(node, 'workDays')
  const workDays = workDaysString
    ? workDaysString
        .split(/[|,]/)
        .map((part) => WORKDAY_LOOKUP[part.trim().toLowerCase()])
        .filter((value): value is number => !Number.isNaN(value))
    : DEFAULT_WORK_DAYS

  return {
    startDate: readAttr(node, 'startDate') || undefined,
    workDays: workDays.length > 0 ? workDays : DEFAULT_WORK_DAYS,
    workDaysPerIteration: readNumber(node, 'workDaysPerIteration', 10),
    intervalsToOneDay: readNumber(node, 'intervalsToOneDay', 1),
    costPerDay: readCurrency(node, 'costPerDay', 0),
    targetLikelihood: readNumber(node, 'targetLikelihood', 85),
    targetDate: readAttr(node, 'targetDate') || undefined,
    revenue: readCurrency(node, 'revenue', 0),
    revenueUnit: parseRevenueUnit(readAttr(node, 'revenueUnit')),
    actuals: parseForecastActuals(node),
    excludedDates: Array.from(node.querySelectorAll(':scope > excludeDate'))
      .map((ed) => readAttr(ed, 'date') || ed.textContent.trim() || '')
      .filter(Boolean),
  }
}

function noteUnsupported(root: Element, warnings: string[]) {
  const unsupported: Array<[string, string]> = [
    ['distributions', 'Named distributions are ignored; low/high bounds are used instead.'],
  ]

  unsupported.forEach(([selector, message]) => {
    if (root.querySelector(selector)) {
      warnings.push(message)
    }
  })
}

function preprocessSimMl(source: string, warnings: string[]) {
  const variables = new Map<string, string>()
  const piRegex = /<\?(variable|parameter)\s+([^?]+)\?>/gi

  source.replace(piRegex, (_match, _type, attrs) => {
    const name = /name="(.*?)"/i.exec(attrs)?.[1]
    const value = /value="(.*?)"/i.exec(attrs)?.[1]
    if (name && value != null) {
      const replaced = replaceVariables(value, variables)
      variables.set(name, maybeEvaluateExpression(replaced))
    }
    return ''
  })

  const cleaned = source.replace(piRegex, '')
  const replaced = cleaned.replace(/"([^"]*@[^"]*)"/g, (_full, value) => {
    const next = maybeEvaluateExpression(replaceVariables(value, variables))
    return `"${next}"`
  })

  if (source.includes('<?include')) {
    warnings.push('Include processing instructions are ignored in the browser version.')
  }

  return replaced
}

function replaceVariables(input: string, variables: Map<string, string>) {
  let output = input
  for (const [name, value] of variables.entries()) {
    output = output.split(name).join(value)
  }
  return output.trim()
}

function maybeEvaluateExpression(input: string) {
  const expression = input.trim().replace(/^=\s*/, '')
  if (!expression.includes('@') && /^[0-9+\-*/().\s]+$/.test(expression)) {
    try {
      return String(Function(`"use strict"; return (${expression})`)())
    } catch {
      return input
    }
  }
  return input
}

function readAttr(node: Element | null, name: string) {
  return node?.getAttribute(name)?.trim() ?? ''
}

function parsePullOrder(value: string): SimPullOrder {
  const normalized = value.toLowerCase()
  if (normalized === 'afterordering' || normalized === 'randomafterordering') return 'randomAfterOrdering'
  if (normalized === 'random') return 'random'
  if (normalized === 'index' || normalized === 'indexsequence') return 'indexSequence'
  if (normalized === 'fifo') return 'FIFO'
  if (normalized === 'fifostrict') return 'FIFOStrict'
  return 'randomAfterOrdering'
}


function parseAggregationValue(value: string): SimAggregationValue {
  const normalized = value.trim().toLowerCase()
  if (normalized === 'median') return 'Median'
  if (normalized === 'fifth' || normalized === '5th') return 'Fifth'
  if (normalized === 'ninetyfifth' || normalized === '95th') return 'NinetyFifth'
  if (normalized === 'max' || normalized === 'maximum') return 'Max'
  if (normalized === 'min' || normalized === 'minimum') return 'Min'
  return 'Average'
}

function parseOccurrenceType(value: string): SimOccurrenceType {
  const normalized = value.trim().toLowerCase()
  if (normalized === 'percentage') return 'percentage'
  if (normalized === 'size' || normalized === 'points') return 'points'
  return 'count'
}

function parseRevenueUnit(value: string): 'day' | 'week' | 'month' | 'year' {
  const normalized = value.trim().toLowerCase()
  if (normalized === 'day' || normalized === 'days') return 'day'
  if (normalized === 'week' || normalized === 'weeks') return 'week'
  if (normalized === 'year' || normalized === 'years') return 'year'
  return 'month'
}

function parseForecastActuals(node: Element) {
  const containers = Array.from(node.querySelectorAll(':scope > actuals > actual'))
  const direct = Array.from(node.querySelectorAll(':scope > actual'))
  return [...containers, ...direct].map((actual) => ({
    date: readAttr(actual, 'date') || '',
    count: readNumber(actual, 'count', 0),
    annotation: readAttr(actual, 'annotation') || actual.textContent?.trim() || undefined,
  })).filter((a) => a.date)
}

function readChildText(node: Element | null, selector: string) {
  return node?.querySelector(`:scope > ${selector}`)?.textContent.trim() || undefined
}

function readNumber(node: Element | null, name: string, fallback: number) {
  const value = readAttr(node, name)
  if (!value) return fallback
  const parsed = Number.parseFloat(value)
  return Number.isFinite(parsed) ? parsed : fallback
}

function readOptionalNumber(node: Element | null, name: string) {
  const value = readAttr(node, name)
  if (!value) return undefined
  const parsed = Number.parseFloat(value)
  return Number.isFinite(parsed) ? parsed : undefined
}

function readBoolean(node: Element | null, name: string, fallback: boolean) {
  const value = readAttr(node, name).toLowerCase()
  if (!value) return fallback
  return value === 'true' || value === 'yes'
}

function readCurrency(node: Element | null, name: string, fallback: number) {
  const value = readAttr(node, name)
  if (!value) return fallback
  const parsed = Number.parseFloat(value.replace(/[^0-9.-]/g, ''))
  return Number.isFinite(parsed) ? parsed : fallback
}

function parseDefects(setup: Element): SimDefect[] {
  return Array.from(setup.querySelectorAll(':scope > defects > defect')).map((node) => ({
    name: readAttr(node, 'name') || node.textContent?.trim() || 'Defect',
    columnId: readNumber(node, 'columnId', 1),
    startsInColumnId: readOptionalNumber(node, 'startsInColumnId'),
    occurrenceLowBound: readNumber(node, 'occurrenceLowBound', 5),
    occurrenceHighBound: readNumber(node, 'occurrenceHighBound', 10),
    estimateLowBound: readNumber(node, 'estimateLowBound', 1),
    estimateHighBound: readNumber(node, 'estimateHighBound', 3),
    count: readNumber(node, 'count', 1),
    columnOverrides: Array.from(node.querySelectorAll(':scope > column')).map((c) => ({
      columnId: readNumber(c, 'id', -1),
      estimateLowBound: readOptionalNumber(c, 'estimateLowBound'),
      estimateHighBound: readOptionalNumber(c, 'estimateHighBound'),
      skipPercentage: readOptionalNumber(c, 'skipPercentage'),
    })),
  }))
}

function parseBlockingEvents(setup: Element): SimBlockingEvent[] {
  return Array.from(setup.querySelectorAll(':scope > blockingEvents > blockingEvent')).map(
    (node) => ({
      name: readAttr(node, 'name') || node.textContent?.trim() || 'Blocking',
      columnId: readNumber(node, 'columnId', 1),
      occurrenceLowBound: readNumber(node, 'occurrenceLowBound', 3),
      occurrenceHighBound: readNumber(node, 'occurrenceHighBound', 6),
      estimateLowBound: readNumber(node, 'estimateLowBound', 1),
      estimateHighBound: readNumber(node, 'estimateHighBound', 5),
      occurrenceType: parseOccurrenceType(readAttr(node, 'occurrenceType')),
      scale: readNumber(node, 'scale', 1),
      phases: (readAttr(node, 'phases') || '')
        .split(/[|,]/)
        .map((value) => value.trim())
        .filter(Boolean),
      targetCustomBacklog: readAttr(node, 'targetCustomBacklog') || undefined,
      targetDeliverable: readAttr(node, 'targetDeliverable') || undefined,
      blockWork: readBoolean(node, 'blockWork', true),
      blockDefects: readBoolean(node, 'blockDefects', true),
      blockAddedScope: readBoolean(node, 'blockAddedScope', true),
    }),
  )
}

function parseAddedScopes(setup: Element): SimAddedScope[] {
  return Array.from(setup.querySelectorAll(':scope > addedScopes > addedScope')).map((node) => ({
    name: readAttr(node, 'name') || node.textContent?.trim() || 'Added scope',
    occurrenceLowBound: readNumber(node, 'occurrenceLowBound', 3),
    occurrenceHighBound: readNumber(node, 'occurrenceHighBound', 8),
    estimateLowBound: readNumber(node, 'estimateLowBound', 1),
    estimateHighBound: readNumber(node, 'estimateHighBound', 3),
    count: readNumber(node, 'count', 1),
  }))
}

function parsePhases(setup: Element): SimPhases | undefined {
  const node = setup.querySelector(':scope > phases')
  if (!node) return undefined

  const unitStr = (readAttr(node, 'unit') || 'percentage').toLowerCase()
  let unit: SimPhases['unit'] = 'percentage'
  if (unitStr === 'interval' || unitStr === 'intervals') unit = 'interval'
  else if (unitStr === 'iteration' || unitStr === 'iterations') unit = 'iteration'

  const phases: SimPhase[] = Array.from(node.querySelectorAll(':scope > phase')).map((phase) => {
    let start = readNumber(phase, 'start', 0)
    let end = readNumber(phase, 'end', 0)
    // Legacy: fall back to startPercentage/endPercentage
    if (start === 0 && end === 0) {
      start = readNumber(phase, 'startPercentage', 0)
      end = readNumber(phase, 'endPercentage', 100)
    }
    return {
      name: phase.textContent.trim(),
      start,
      end,
      estimateMultiplier: readNumber(phase, 'estimateMultiplier', 1),
      occurrenceMultiplier: readNumber(phase, 'occurrenceMultiplier', 1),
      iterationMultiplier: readNumber(phase, 'iterationMultiplier', 1),
      costPerDay: readCurrency(phase, 'costPerDay', 0),
      columns: Array.from(phase.querySelectorAll(':scope > column')).map((col) => ({
        columnId: readNumber(col, 'id', -1),
        wipLimit: readNumber(col, 'wipLimit', 0),
      })),
    }
  })

  return phases.length > 0 ? { unit, phases } : undefined
}

function parseClassOfServices(setup: Element): SimClassOfService[] {
  return Array.from(setup.querySelectorAll(':scope > classOfServices > classOfService')).map(
    (node) => ({
      name: node.textContent.trim(),
      order: readNumber(node, 'order', 1),
      isDefault: readBoolean(node, 'default', false),
      violateWip: readBoolean(node, 'violateWIP', false),
      skipPercentage: readNumber(node, 'skipPercentage', 0),
      maximumAllowedOnBoard: readNumber(node, 'maximumAllowedOnBoard', Number.MAX_SAFE_INTEGER),
      columnOverrides: Array.from(node.querySelectorAll(':scope > column')).map((c) => ({
        columnId: readNumber(c, 'id', -1),
        estimateLowBound: readOptionalNumber(c, 'estimateLowBound'),
        estimateHighBound: readOptionalNumber(c, 'estimateHighBound'),
        skipPercentage: readOptionalNumber(c, 'skipPercentage'),
      })),
    }),
  )
}

interface KanbanItem {
  id: string
  label: string
  deliverable?: string
  templateName: string
  currentColumn: number
  /** Accumulated time-so-far in the current column. */
  timeSoFar: number
  /** Random work time assigned for the current column (0 for buffer). */
  calculatedWork: number
  /** Additional blocked time from blocking events. */
  blockedTime: number
  blockerLabel?: string
  done: boolean
  status: 'backlog' | 'active' | 'blocked' | 'queued' | 'done'
  columnOverrides: SimColumnOverride[]
  percentageLowBound: number
  percentageHighBound: number
  cardType: 'work' | 'defect' | 'addedScope'
  classOfServiceRef?: SimClassOfService
  deliverableOrder?: number
  dueDate?: string
  order: number
  pullOrder: number
  preRequisiteDeliverables: string[]
  earliestStartDate?: string
  businessValue: number
}

interface ScrumItem {
  id: string
  label: string
  deliverable?: string
  templateName: string
  estimate: number
  /** Points completed so far across all iterations. */
  completedPoints: number
  /** Blocked points assigned by blocking events; must be burned before work. */
  blockedPoints: number
  /** Blocked points burned so far. */
  burnedBlockedPoints: number
  blockerLabel?: string
  done: boolean
  storyType: 'work' | 'defect' | 'addedScope'
  classOfServiceRef?: SimClassOfService
  deliverableOrder?: number
  dueDate?: string
  order: number
  sortOrder: number
  businessValue: number
}

export function runVisualSimulation(model: SimModel): SimulationRunResult {
  const result = model.execute.simulationType === 'kanban'
    ? runKanbanSimulation(model)
    : runScrumSimulation(model)
  return applyDecimalRoundingToRunResult(result, model.execute.decimalRounding)
}

export function runMonteCarlo(model: SimModel, cycles = model.execute.monteCarloCycles): MonteCarloResult {
  const steps = Array.from({ length: Math.max(1, Math.round(cycles)) }, () => runVisualSimulation(model).totalSteps).sort(
    (a, b) => a - b,
  )

  const histogramMap = new Map<number, number>()
  steps.forEach((step) => {
    histogramMap.set(step, (histogramMap.get(step) ?? 0) + 1)
  })

  const likelihoods = [0.5, 0.7, 0.85, 0.95]
  const avg = round(steps.reduce((sum, value) => sum + value, 0) / steps.length)
  return applyDecimalRoundingToMonteCarloResult({
    averageSteps: avg,
    medianSteps: computePercentile(steps, 50),
    minSteps: steps[0] ?? 0,
    maxSteps: steps[steps.length - 1] ?? 0,
    standardDeviation: round(sampleStdDev(steps)),
    percentileSteps: likelihoods.map((likelihood) => {
      const stepValue = Math.ceil(computePercentile(steps, likelihood * 100))
      return {
        likelihood,
        steps: stepValue,
        date: forecastStepToDate(model, stepValue),
      }
    }),
    histogram: Array.from(histogramMap.entries())
      .map(([step, count]) => ({ step, count }))
      .sort((a, b) => a.step - b.step),
    rawSteps: steps,
    summarySteps: selectAggregationValue(steps, model.execute.aggregationValue),
  }, model.execute.decimalRounding)
}

export function runSensitivityAnalysis(model: SimModel): SensitivityResult {
  const baselineCycles = Math.max(25, model.execute.sensitivityCycles)
  const baseline = runMonteCarlo(model, baselineCycles)
  const tests: SensitivityResult['tests'] = []

  if (model.execute.simulationType === 'kanban') {
    // -- Column estimate sensitivity -----------------------------------------
    model.setup.columns.forEach((column) => {
      const modified = structuredClone(model)
      const target = modified.setup.columns.find((candidate) => candidate.id === column.id)
      if (!target) return
      target.estimateLowBound *= modified.execute.sensitivityEstimateMultiplier
      target.estimateHighBound *= modified.execute.sensitivityEstimateMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(column.name, 'Column', baseline.averageSteps, result.averageSteps))
    })

    // -- Defect occurrence sensitivity ---------------------------------------
    model.setup.defects.forEach((defect) => {
      const modified = structuredClone(model)
      const target = modified.setup.defects.find((d) => d.name === defect.name)
      if (!target) return
      target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
      target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(defect.name, 'Defect', baseline.averageSteps, result.averageSteps))
    })

    // -- Blocking event occurrence + estimate --------------------------------
    model.setup.blockingEvents.forEach((block) => {
      {
        const modified = structuredClone(model)
        const target = modified.setup.blockingEvents.find((b) => b.name === block.name)
        if (target) {
          target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
          target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
          const result = runMonteCarlo(modified, baselineCycles)
          tests.push(toSensitivityRow(`${block.name} (occurrence)`, 'BlockingEvent', baseline.averageSteps, result.averageSteps))
        }
      }
      {
        const modified = structuredClone(model)
        const target = modified.setup.blockingEvents.find((b) => b.name === block.name)
        if (target) {
          target.estimateLowBound *= modified.execute.sensitivityEstimateMultiplier
          target.estimateHighBound *= modified.execute.sensitivityEstimateMultiplier
          const result = runMonteCarlo(modified, baselineCycles)
          tests.push(toSensitivityRow(`${block.name} (estimate)`, 'BlockingEvent', baseline.averageSteps, result.averageSteps))
        }
      }
    })

    // -- Added scope occurrence sensitivity ----------------------------------
    model.setup.addedScopes.forEach((scope) => {
      const modified = structuredClone(model)
      const target = modified.setup.addedScopes.find((s) => s.name === scope.name)
      if (!target) return
      target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
      target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(scope.name, 'AddedScope', baseline.averageSteps, result.averageSteps))
    })
  } else {
    // -- Scrum: iteration capacity -------------------------------------------
    const iteration = model.setup.iteration
    if (iteration) {
      const velocityBoost = structuredClone(model)
      if (velocityBoost.setup.iteration) {
        velocityBoost.setup.iteration.storyPointsPerIterationLowBound *=
          velocityBoost.execute.sensitivityIterationMultiplier
        velocityBoost.setup.iteration.storyPointsPerIterationHighBound *=
          velocityBoost.execute.sensitivityIterationMultiplier
        const result = runMonteCarlo(velocityBoost, baselineCycles)
        tests.push(toSensitivityRow('Iteration capacity', 'Velocity', baseline.averageSteps, result.averageSteps))
      }
    }

    // -- Scrum: backlog type estimates ---------------------------------------
    model.setup.backlog.items.forEach((item) => {
      const modified = structuredClone(model)
      const target = modified.setup.backlog.items.find((candidate) => candidate.id === item.id)
      if (!target || target.estimateLowBound == null || target.estimateHighBound == null) return
      target.estimateLowBound *= modified.execute.sensitivityEstimateMultiplier
      target.estimateHighBound *= modified.execute.sensitivityEstimateMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(item.name, 'Backlog type', baseline.averageSteps, result.averageSteps))
    })

    // -- Scrum: defect occurrence sensitivity --------------------------------
    model.setup.defects.forEach((defect) => {
      const modified = structuredClone(model)
      const target = modified.setup.defects.find((d) => d.name === defect.name)
      if (!target) return
      target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
      target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(defect.name, 'Defect', baseline.averageSteps, result.averageSteps))
    })

    // -- Scrum: blocking event occurrence + estimate -------------------------
    model.setup.blockingEvents.forEach((block) => {
      {
        const modified = structuredClone(model)
        const target = modified.setup.blockingEvents.find((b) => b.name === block.name)
        if (target) {
          target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
          target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
          const result = runMonteCarlo(modified, baselineCycles)
          tests.push(toSensitivityRow(`${block.name} (occurrence)`, 'BlockingEvent', baseline.averageSteps, result.averageSteps))
        }
      }
      {
        const modified = structuredClone(model)
        const target = modified.setup.blockingEvents.find((b) => b.name === block.name)
        if (target) {
          target.estimateLowBound *= modified.execute.sensitivityEstimateMultiplier
          target.estimateHighBound *= modified.execute.sensitivityEstimateMultiplier
          const result = runMonteCarlo(modified, baselineCycles)
          tests.push(toSensitivityRow(`${block.name} (estimate)`, 'BlockingEvent', baseline.averageSteps, result.averageSteps))
        }
      }
    })

    // -- Scrum: added scope occurrence sensitivity ---------------------------
    model.setup.addedScopes.forEach((scope) => {
      const modified = structuredClone(model)
      const target = modified.setup.addedScopes.find((s) => s.name === scope.name)
      if (!target) return
      target.occurrenceLowBound *= modified.execute.sensitivityOccurrenceMultiplier
      target.occurrenceHighBound *= modified.execute.sensitivityOccurrenceMultiplier
      const result = runMonteCarlo(modified, baselineCycles)
      tests.push(toSensitivityRow(scope.name, 'AddedScope', baseline.averageSteps, result.averageSteps))
    })
  }

  const result = {
    baselineAverageSteps: baseline.averageSteps,
    tests: tests.sort((a, b) => a.deltaSteps - b.deltaSteps),
  }

  return applyDecimalRoundingToSensitivityResult(result, model.execute.decimalRounding)
}


function toSensitivityRow(name: string, type: string, baselineAverageSteps: number, averageSteps: number) {
  const deltaSteps = round(averageSteps - baselineAverageSteps)
  return {
    name,
    type,
    averageSteps,
    deltaSteps,
    deltaPercent:
      baselineAverageSteps === 0 ? 0 : round((deltaSteps / baselineAverageSteps) * 100),
  }
}

function runKanbanSimulation(model: SimModel): SimulationRunResult {
  const columns = model.setup.columns
  const limit = Math.max(1, Math.round(model.execute.limitIntervalsTo))
  const items = createKanbanItems(model)
  const originalItemCount = items.length
  const doneItems: KanbanItem[] = items.filter((item) => item.done)
  const snapshots: BoardSnapshot[] = []
  let step = 0
  let nextItemId = items.length + 1
  let nextPullOrder = items.length + 1
  let itemsPulled = 0
  let currentPhase: SimPhase | null = null
  let accumulatedCost = 0
  const valueDeliveredByStep: { step: number; cumulativeValue: number }[] = []

  // -- Event processor state -------------------------------------------------
  const defectProcessors = model.setup.defects.map((d) => ({
    def: d,
    cardsSeen: 0,
    trigger: Math.round(sampleBetween(d.occurrenceLowBound, d.occurrenceHighBound)),
  }))
  const blockingProcessors = model.setup.blockingEvents.map((b) => ({
    def: b,
    occurrenceProgress: 0,
    trigger: 1,
  }))
  const addedScopeProcessors = model.setup.addedScopes.map((a) => ({
    def: a,
    completedSeen: 0,
    trigger: Math.round(sampleBetween(a.occurrenceLowBound, a.occurrenceHighBound)),
  }))
  const defectBacklog: KanbanItem[] = []

  // COS state
  const cosActive = model.setup.classOfServices.length > 0

  // Resolve initial phase (for percentage mode with start=0)
  currentPhase = resolvePhase(model.setup.phases, 0, 0, originalItemCount)

  const phaseEstMult = () => currentPhase?.estimateMultiplier ?? 1
  const phaseOccMult = () => currentPhase?.occurrenceMultiplier ?? 1
  blockingProcessors.forEach((bp) => { bp.trigger = sampleOccurrenceTrigger(bp.def, phaseOccMult()) })

  // Place items with initialColumn
  items.forEach((item) => {
    if (!item.done && item.currentColumn >= 0) {
      const col = columns[item.currentColumn]
      item.calculatedWork = col.isBuffer ? 0 : sampleColumnDuration(col, item, phaseEstMult())
      item.timeSoFar = 0
      item.status = 'active'
      item.pullOrder = nextPullOrder++
    }
  })

  pushKanbanSnapshot(snapshots, step, columns, items, doneItems, currentPhase)
  valueDeliveredByStep.push({ step, cumulativeValue: round(items.filter((i) => i.done).reduce((sum, i) => sum + i.businessValue, 0)) })

  while (step < limit && doneItems.length < items.length) {
    step += 1

    // Resolve phase for interval-based mode
    if (model.setup.phases?.unit === 'interval') {
      currentPhase = resolvePhase(model.setup.phases, step, itemsPulled, originalItemCount)
    }

    // Accumulate cost for this interval
    const forecast = model.setup.forecastDate
    if (forecast) {
      const phaseCost = currentPhase?.costPerDay && currentPhase.costPerDay > 0 ? currentPhase.costPerDay : forecast.costPerDay
      if (phaseCost > 0) {
        accumulatedCost += phaseCost / Math.max(1, forecast.intervalsToOneDay)
      }
    }

    // 1. Increment time-so-far for all cards on the board
    items.forEach((item) => {
      if (!item.done && item.currentColumn >= 0) {
        const col = columns[item.currentColumn]
        if (!col.isBuffer) {
          item.timeSoFar += 1.0
        }
      }
    })

    // 2. Process columns right-to-left (pull system)
    for (let colIdx = columns.length - 1; colIdx >= 0; colIdx -= 1) {
      const column = columns[colIdx]
      const canComplete = stepMatchesInterval(step, column.completeInterval)

      const cardsInColumn = orderCardsForProcessing(
        items.filter((item) => !item.done && item.currentColumn === colIdx),
        model.execute.pullOrder,
      )

      cardsInColumn.forEach((item) => {
        if (item.status === 'done') return

        const isReady = column.isBuffer || item.timeSoFar >= item.calculatedWork + item.blockedTime

        if (!isReady) {
          if (item.status !== 'blocked') item.status = 'active'
          return
        }

        if (!canComplete) {
          item.status = 'queued'
          return
        }

        if (!isStrictFifoAllowsComplete(model.execute.pullOrder, item, colIdx, items)) {
          item.status = 'queued'
          return
        }

        // Check blocking events (apply phase occurrence multiplier to trigger)
        let isBlocked = false
        for (const bp of blockingProcessors) {
          if (bp.def.columnId === column.id && blockingEventApplies(bp.def, item, currentPhase)) {
            bp.occurrenceProgress += 1
            if (bp.occurrenceProgress >= bp.trigger) {
              bp.occurrenceProgress = 0
              bp.trigger = sampleOccurrenceTrigger(bp.def, phaseOccMult())
              item.blockedTime += sampleBetween(bp.def.estimateLowBound, bp.def.estimateHighBound) * bp.def.scale * phaseEstMult()
              item.status = 'blocked'
              item.blockerLabel = bp.def.name
              isBlocked = true
              break
            }
          }
        }
        if (isBlocked) return

        // Move or complete
        const nextIdx = findNextColumnIndex(columns, colIdx, item)
        if (nextIdx === null) {
          item.done = true
          item.status = 'done'
          doneItems.push(item)
          // Defect check on complete
          defectProcessors.forEach((dp) => {
            dp.cardsSeen += 1
            if (dp.cardsSeen >= dp.trigger) {
              dp.cardsSeen = 0
              dp.trigger = Math.round(
                sampleBetween(dp.def.occurrenceLowBound, dp.def.occurrenceHighBound) * phaseOccMult(),
              )
              for (let c = 0; c < dp.def.count; c += 1) {
                const defectItem: KanbanItem = {
                  id: `def-${nextItemId++}`,
                  label: formatSpawnedLabel(dp.def.name, nextItemId),
                  templateName: dp.def.name,
                  currentColumn: resolveKanbanStartColumn(columns, dp.def.startsInColumnId),
                  timeSoFar: 0,
                  calculatedWork: 0,
                  blockedTime: 0,
                  blockerLabel: undefined,
                  done: false,
                  status: dp.def.startsInColumnId ? 'active' : 'backlog',
                  columnOverrides: dp.def.columnOverrides,
                  percentageLowBound: 0,
                  percentageHighBound: 100,
                  cardType: 'defect',
                  order: Number.MAX_SAFE_INTEGER,
                  pullOrder: nextPullOrder++,
                  preRequisiteDeliverables: [],
                  earliestStartDate: undefined,
                  businessValue: 0,
                }
                items.push(defectItem)
                if (defectItem.currentColumn >= 0) {
                  const startColumn = columns[defectItem.currentColumn]
                  defectItem.calculatedWork = startColumn.isBuffer
                    ? 0
                    : sampleColumnDuration(startColumn, defectItem, phaseEstMult())
                  defectItem.status = startColumn.isBuffer ? 'queued' : 'active'
                } else {
                  defectBacklog.push(defectItem)
                }
              }
            }
          })
          // Added scope check on complete
          addedScopeProcessors.forEach((asp) => {
            asp.completedSeen += 1
            if (asp.completedSeen >= asp.trigger) {
              asp.completedSeen = 0
              asp.trigger = Math.round(
                sampleBetween(asp.def.occurrenceLowBound, asp.def.occurrenceHighBound) * phaseOccMult(),
              )
              for (let c = 0; c < asp.def.count; c += 1) {
                items.push({
                  id: `as-${nextItemId++}`,
                  label: formatSpawnedLabel(asp.def.name, nextItemId),
                  templateName: asp.def.name,
                  currentColumn: -1,
                  timeSoFar: 0,
                  calculatedWork: 0,
                  blockedTime: 0,
                  blockerLabel: undefined,
                  done: false,
                  status: 'backlog',
                  columnOverrides: [],
                  percentageLowBound: 0,
                  percentageHighBound: 100,
                  cardType: 'addedScope',
                  order: Number.MAX_SAFE_INTEGER,
                  pullOrder: nextPullOrder++,
                  preRequisiteDeliverables: [],
                  earliestStartDate: undefined,
                  businessValue: 0,
                })
              }
            }
          })
          return
        }

        const nextColumn = columns[nextIdx]
        if (!stepMatchesInterval(step, nextColumn.replenishInterval)) {
          item.status = 'queued'
          return
        }

        const activeInTarget = items.filter((c) => !c.done && c.currentColumn === nextIdx).length
        const wipLim = getEffectiveWipLimit(nextColumn, currentPhase)

        // ViolateWip: allow WIP-violating COS cards through even if column is full
        if (activeInTarget >= wipLim) {
          if (item.classOfServiceRef?.violateWip) {
            // Place it and block the lowest-priority active card
            item.currentColumn = nextIdx
            item.calculatedWork = nextColumn.isBuffer ? 0 : sampleColumnDuration(nextColumn, item, phaseEstMult())
            item.timeSoFar = 0
            item.blockedTime = 0
            item.pullOrder = nextPullOrder++
            item.status = nextColumn.isBuffer ? 'queued' : 'active'
            blockLowestPriorityCard(items, nextIdx, item)
            if (nextColumn.isBuffer) {
              tryPassThroughBuffer(item, nextIdx, columns, items, doneItems, step, currentPhase)
            }
          } else {
            item.status = 'queued'
          }
          return
        }

        // Carry-over excess time
        const excessTime = Math.max(0, item.timeSoFar - (item.calculatedWork + item.blockedTime))
        item.currentColumn = nextIdx
        item.calculatedWork = nextColumn.isBuffer ? 0 : sampleColumnDuration(nextColumn, item, phaseEstMult())
        item.timeSoFar = excessTime
        item.blockedTime = 0
        item.blockerLabel = undefined
        item.pullOrder = nextPullOrder++
        item.status = nextColumn.isBuffer ? 'queued' : 'active'

        // Buffer passthrough
        if (nextColumn.isBuffer) {
          tryPassThroughBuffer(item, nextIdx, columns, items, doneItems, step, currentPhase)
        }
      })
    }

    // 3. Fill first column from defect backlog + main backlog
    const firstColumnIndex = columns.findIndex((c) => stepMatchesInterval(step, c.replenishInterval))
    if (firstColumnIndex >= 0) {
      const firstColumn = columns[firstColumnIndex]
      const wipLim = getEffectiveWipLimit(firstColumn, currentPhase)
      let active = items.filter((item) => !item.done && item.currentColumn === firstColumnIndex).length

      // Pull defects first
      while (active < wipLim && defectBacklog.length > 0) {
        const defectCard = defectBacklog.shift()!
        defectCard.currentColumn = firstColumnIndex
        defectCard.calculatedWork = firstColumn.isBuffer ? 0 : sampleColumnDuration(firstColumn, defectCard, phaseEstMult())
        defectCard.timeSoFar = 0
        defectCard.blockedTime = 0
        defectCard.blockerLabel = undefined
        defectCard.pullOrder = nextPullOrder++
        defectCard.status = 'active'
        active += 1
      }

      // Pull from main backlog (respecting COS rules)
      const backlogCandidates = nextAllowedBacklogCard(items, model, step)

      for (const item of backlogCandidates) {
        if (active >= wipLim && !item.classOfServiceRef?.violateWip) continue

        // MaximumAllowedOnBoard check
        if (cosActive && item.classOfServiceRef) {
          const onBoard = items.filter(
            (c) => !c.done && c.currentColumn >= 0 && c.classOfServiceRef === item.classOfServiceRef,
          ).length
          if (onBoard >= item.classOfServiceRef.maximumAllowedOnBoard) continue
        }

        // COS skipPercentage: item is auto-completed without entering the board
        if (item.classOfServiceRef?.skipPercentage && item.classOfServiceRef.skipPercentage > 0) {
          if (Math.random() * 100 < item.classOfServiceRef.skipPercentage) {
            item.done = true
            item.status = 'done'
            doneItems.push(item)
            continue
          }
        }

        // Track that we pulled from original backlog (for phase percentage mode)
        itemsPulled += 1
        if (model.setup.phases?.unit === 'percentage') {
          currentPhase = resolvePhase(model.setup.phases, step, itemsPulled, originalItemCount)
        }

        const isViolating = active >= wipLim && item.classOfServiceRef?.violateWip
        item.currentColumn = firstColumnIndex
        item.calculatedWork = firstColumn.isBuffer ? 0 : sampleColumnDuration(firstColumn, item, phaseEstMult())
        item.timeSoFar = 0
        item.blockedTime = 0
        item.blockerLabel = undefined
        item.pullOrder = nextPullOrder++
        item.status = 'active'
        active += 1

        if (isViolating) {
          blockLowestPriorityCard(items, firstColumnIndex, item)
        }
      }
    }

    pushKanbanSnapshot(snapshots, step, columns, items, doneItems, currentPhase)
    valueDeliveredByStep.push({ step, cumulativeValue: round(items.filter((i) => i.done).reduce((sum, i) => sum + i.businessValue, 0)) })

    const completePct = items.length === 0 ? 100 : (doneItems.length / items.length) * 100
    const totalBoardWip = columns.reduce((sum, column) => sum + getEffectiveWipLimit(column, currentPhase), 0)
    const cardsOnBoard = items.filter((item) => !item.done && item.currentColumn >= 0).length
    const activePositionsPct = totalBoardWip > 0 ? (cardsOnBoard / totalBoardWip) * 100 : 0

    if (
      completePct >= model.execute.completePercentage &&
      activePositionsPct <= model.execute.activePositionsCompletePercentage
    ) {
      break
    }
  }

  const completionDate = forecastStepToDate(model, step)
  const totalCost = accumulatedCost > 0 ? round(accumulatedCost) : estimateTotalCost(model, step)

  return {
    kind: 'kanban',
    snapshots,
    totalSteps: step,
    completedItems: doneItems.length,
    completionDate,
    totalCost,
    costOfDelay: calculateCostOfDelay(model, completionDate),
    valueDeliveredByStep,
    cumulativeFlow: snapshots.map((snapshot) => ({
      step: snapshot.step,
      backlog: snapshot.backlogCount,
      done: snapshot.doneCount,
      columns: Object.fromEntries(snapshot.columns.map((column) => [column.id, column.cards.length])),
    })),
  }
}

function runScrumSimulation(model: SimModel): SimulationRunResult {
  const iteration = model.setup.iteration
  const throughput = model.setup.throughput
  if (!iteration && !throughput) {
    throw new Error('Scrum simulations require iteration or throughput settings.')
  }

  const backlog = createScrumItems(model)
  const originalStoryCount = backlog.length
  const done: ScrumItem[] = backlog.filter((item) => item.done)
  const snapshots: BoardSnapshot[] = []
  let step = 0
  let nextItemId = backlog.length + 1
  let storiesStarted = 0
  let currentPhase: SimPhase | null = null
  let accumulatedCost = 0
  const valueDeliveredByStep: { step: number; cumulativeValue: number }[] = []
  const cosActive = model.setup.classOfServices.length > 0

  // -- Event processors for scrum ------------------------------------------
  const defectProcessors = model.setup.defects.map((d) => ({
    def: d,
    storiesSeen: 0,
    trigger: Math.round(sampleBetween(d.occurrenceLowBound, d.occurrenceHighBound)),
  }))
  const blockingProcessors = model.setup.blockingEvents.map((b) => ({
    def: b,
    occurrenceProgress: 0,
    trigger: 1,
  }))
  const addedScopeProcessors = model.setup.addedScopes.map((a) => ({
    def: a,
    completedSeen: 0,
    trigger: Math.round(sampleBetween(a.occurrenceLowBound, a.occurrenceHighBound)),
  }))

  // Resolve initial phase
  currentPhase = resolvePhase(model.setup.phases, 0, 0, originalStoryCount)

  pushScrumSnapshot(snapshots, step, backlog, [], done, 0, currentPhase)
  valueDeliveredByStep.push({ step, cumulativeValue: round(backlog.filter((i) => i.done).reduce((sum, i) => sum + i.businessValue, 0)) })

  // Track stories currently being worked (carried over between iterations)
  let inProgress: ScrumItem[] = []

  while (backlog.some((item) => !item.done) && step < model.execute.limitIntervalsTo) {
    step += 1

    // Resolve phase for iteration-based mode
    if (model.setup.phases?.unit === 'iteration') {
      currentPhase = resolvePhase(model.setup.phases, step, storiesStarted, originalStoryCount)
    }

    const phaseIterMult = currentPhase?.iterationMultiplier ?? 1
    const phaseEstMult = currentPhase?.estimateMultiplier ?? 1
    const phaseOccMult = currentPhase?.occurrenceMultiplier ?? 1
    if (step === 1) blockingProcessors.forEach((bp) => { bp.trigger = sampleOccurrenceTrigger(bp.def, phaseOccMult) })

    // Accumulate cost
    const forecast = model.setup.forecastDate
    if (forecast) {
      const phaseCost = currentPhase?.costPerDay && currentPhase.costPerDay > 0 ? currentPhase.costPerDay : forecast.costPerDay
      if (phaseCost > 0) {
        accumulatedCost += phaseCost * forecast.workDaysPerIteration
      }
    }

    const rawCapacity = iteration
      ? sampleBetween(
        iteration.storyPointsPerIterationLowBound,
        iteration.storyPointsPerIterationHighBound,
      )
      : sampleBetween(
        throughput?.itemsPerIterationLowBound ?? 1,
        throughput?.itemsPerIterationHighBound ?? 1,
      )
    const pointsCapacity = rawCapacity * phaseIterMult
    let pointsRemaining = pointsCapacity

    // 1. Process carry-over stories from the previous iteration
    const newInProgress: ScrumItem[] = []
    for (const story of inProgress) {
      if (story.done) continue

      const remainingBlocked = story.blockedPoints - story.burnedBlockedPoints
      const remainingWork = story.estimate - story.completedPoints

      if (pointsRemaining >= remainingBlocked + remainingWork) {
        pointsRemaining -= remainingBlocked + remainingWork
        story.burnedBlockedPoints = story.blockedPoints
        story.completedPoints = story.estimate
        story.blockerLabel = undefined
        story.done = true
        done.push(story)
        fireScrumCompletionEvents(addedScopeProcessors, defectProcessors, backlog, nextItemId)
        nextItemId = backlog.length + 1
      } else {
        if (remainingBlocked > 0 && pointsRemaining > 0) {
          const burn = Math.min(remainingBlocked, pointsRemaining)
          story.burnedBlockedPoints += burn
          if (story.burnedBlockedPoints >= story.blockedPoints) {
            story.blockerLabel = undefined
          }
          pointsRemaining -= burn
        }
        if (pointsRemaining > 0) {
          story.completedPoints += pointsRemaining
          pointsRemaining = 0
        }
        newInProgress.push(story)
      }
    }
    inProgress = newInProgress

    // 2. Pull new stories from backlog
    const pending = backlog.filter((item) => !item.done && !inProgress.includes(item))

    if (!iteration && throughput) {
      const takeCount = Math.max(0, Math.floor(pointsRemaining))
      for (const story of pending.slice(0, takeCount)) {
        story.completedPoints = story.estimate
        story.done = true
        done.push(story)
        fireScrumCompletionEvents(addedScopeProcessors, defectProcessors, backlog, nextItemId)
        nextItemId = backlog.length + 1
      }
      pointsRemaining = 0
      pushScrumSnapshot(
        snapshots,
        step,
        backlog.filter((item) => !item.done),
        [],
        done,
        Math.min(pointsCapacity, pointsCapacity - pointsRemaining),
        currentPhase,
      )
      valueDeliveredByStep.push({ step, cumulativeValue: round(backlog.filter((i) => i.done).reduce((sum, i) => sum + i.businessValue, 0)) })
      if (done.length >= backlog.length) break
      continue
    }
    for (const story of pending) {
      if (pointsRemaining <= 0) break

      // MaximumAllowedOnBoard check per COS
      if (cosActive && story.classOfServiceRef) {
        const activeCount = inProgress.filter(
          (s) => s.classOfServiceRef === story.classOfServiceRef,
        ).length
        if (activeCount >= story.classOfServiceRef.maximumAllowedOnBoard) continue
      }

      // COS skipPercentage: auto-complete without entering iteration
      if (story.classOfServiceRef?.skipPercentage && story.classOfServiceRef.skipPercentage > 0) {
        if (Math.random() * 100 < story.classOfServiceRef.skipPercentage) {
          story.done = true
          done.push(story)
          continue
        }
      }

      const storyRemaining = story.estimate - story.completedPoints

      if (pointsRemaining >= storyRemaining || iteration?.allowedToOverAllocate) {
        inProgress.push(story)

        // Track story start for percentage-based phases
        storiesStarted += 1
        if (model.setup.phases?.unit === 'percentage') {
          currentPhase = resolvePhase(model.setup.phases, step, storiesStarted, originalStoryCount)
        }

        // Check blocking events on story start (apply phase occurrence multiplier)
        for (const bp of blockingProcessors) {
          if (!blockingEventApplies(bp.def, story, currentPhase)) continue
          bp.occurrenceProgress += bp.def.occurrenceType === 'points' ? story.estimate : 1
          if (bp.occurrenceProgress >= bp.trigger) {
            bp.occurrenceProgress = 0
            bp.trigger = sampleOccurrenceTrigger(bp.def, phaseOccMult)
            story.blockedPoints += sampleBetween(bp.def.estimateLowBound, bp.def.estimateHighBound) * bp.def.scale * phaseEstMult
            story.blockerLabel = bp.def.name
          }
        }

        // Check if can complete immediately
        const totalNeeded = (story.blockedPoints - story.burnedBlockedPoints) + (story.estimate - story.completedPoints)
        if (pointsRemaining >= totalNeeded) {
          pointsRemaining -= totalNeeded
          story.burnedBlockedPoints = story.blockedPoints
          story.completedPoints = story.estimate
          story.blockerLabel = undefined
          story.done = true
          done.push(story)
          inProgress = inProgress.filter((s) => s !== story)
          fireScrumCompletionEvents(addedScopeProcessors, defectProcessors, backlog, nextItemId)
          nextItemId = backlog.length + 1
        } else {
          // Partial allocation
          let pts = pointsRemaining
          const rb = story.blockedPoints - story.burnedBlockedPoints
          if (rb > 0 && pts > 0) {
            const burn = Math.min(rb, pts)
            story.burnedBlockedPoints += burn
            if (story.burnedBlockedPoints >= story.blockedPoints) {
              story.blockerLabel = undefined
            }
            pts -= burn
          }
          if (pts > 0) {
            story.completedPoints += pts
          }
          pointsRemaining = 0
        }
      } else {
        break
      }
    }

    pushScrumSnapshot(
      snapshots,
      step,
      backlog.filter((item) => !item.done && !inProgress.includes(item)),
      inProgress,
      done,
      Math.min(pointsCapacity, pointsCapacity - pointsRemaining),
      currentPhase,
    )
    valueDeliveredByStep.push({ step, cumulativeValue: round(backlog.filter((i) => i.done).reduce((sum, i) => sum + i.businessValue, 0)) })

    if (done.length >= backlog.length) break
  }

  const completionDate = forecastStepToDate(model, step)
  const totalCost = accumulatedCost > 0 ? round(accumulatedCost) : estimateTotalCost(model, step)

  return {
    kind: 'scrum',
    snapshots,
    totalSteps: step,
    completedItems: done.length,
    completionDate,
    totalCost,
    costOfDelay: calculateCostOfDelay(model, completionDate),
    valueDeliveredByStep,
    cumulativeFlow: snapshots.map((snapshot) => ({
      step: snapshot.step,
      backlog: snapshot.backlogCount,
      done: snapshot.doneCount,
      columns: Object.fromEntries(snapshot.columns.map((column) => [column.id, column.cards.length])),
    })),
  }
}

function createKanbanItems(model: SimModel) {
  const result: KanbanItem[] = []
  let sequence = 0
  const cos = model.setup.classOfServices
  getIncludedBacklogTemplates(model).forEach((template) => {
    const cosRef = resolveCos(cos, template.classOfService)
    for (let count = 0; count < template.count; count += 1) {
      result.push({
        id: `k-${sequence + 1}`,
        label: formatBacklogItemLabel(model.setup.backlog.nameFormat, template, count + 1),
        deliverable: template.deliverable,
        templateName: template.name,
        currentColumn: template.completed
          ? -1
          : template.initialColumn
          ? Math.max(0, model.setup.columns.findIndex((column) => column.id === template.initialColumn))
          : -1,
        timeSoFar: 0,
        calculatedWork: 0,
        blockedTime: 0,
        blockerLabel: undefined,
        done: template.completed,
        status: template.completed ? 'done' : template.initialColumn ? 'active' : 'backlog',
        columnOverrides: template.columnOverrides,
        percentageLowBound: template.percentageLowBound,
        percentageHighBound: template.percentageHighBound,
        cardType: 'work',
        classOfServiceRef: cosRef,
        deliverableOrder: template.deliverableOrder,
        dueDate: template.dueDate,
        order: template.order,
        pullOrder: sequence,
        preRequisiteDeliverables: template.preRequisiteDeliverables,
        earliestStartDate: template.earliestStartDate,
        businessValue: sampleBetween(template.valueLowBound, template.valueHighBound),
      })
      sequence += 1
    }
  })
  if (model.setup.backlog.shuffle) shuffleArray(result)
  result.sort(compareWorkItemPriority)
  return result
}


function orderCardsForProcessing(items: KanbanItem[], pullOrder: SimPullOrder): KanbanItem[] {
  if (pullOrder === 'random') {
    return shuffleArray([...items])
  }

  if (pullOrder === 'indexSequence') {
    return [...items].sort((a, b) => a.order - b.order || a.pullOrder - b.pullOrder)
  }

  if (pullOrder === 'FIFO' || pullOrder === 'FIFOStrict') {
    return [...items].sort((a, b) => a.pullOrder - b.pullOrder || a.order - b.order)
  }

  return [...items]
    .map((item) => ({ item, random: Math.random() }))
    .sort((a, b) => {
      const byBacklog = a.item.order - b.item.order
      if (byBacklog !== 0) return byBacklog
      const byPriority = compareWorkItemPriority(a.item, b.item)
      if (byPriority !== 0) return byPriority
      const byRandom = a.random - b.random
      if (byRandom !== 0) return byRandom
      return a.item.pullOrder - b.item.pullOrder
    })
    .map(({ item }) => item)
}

function isStrictFifoAllowsComplete(
  pullOrder: SimPullOrder,
  item: KanbanItem,
  colIdx: number,
  allItems: KanbanItem[],
) {
  if (pullOrder !== 'FIFOStrict') return true
  return !allItems.some(
    (candidate) =>
      !candidate.done &&
      candidate.currentColumn === colIdx &&
      candidate.id !== item.id &&
      candidate.pullOrder < item.pullOrder,
  )
}

function nextAllowedBacklogCard(items: KanbanItem[], model: SimModel, step: number): KanbanItem[] {
  return items.filter((item) => {
    if (item.done || item.currentColumn !== -1 || item.cardType === 'defect') return false

    if (item.earliestStartDate) {
      const earliest = parseSimDate(item.earliestStartDate)
      const current = currentSimulationDate(model, step)
      if (earliest && current && current < earliest) {
        return false
      }
    }

    if (item.preRequisiteDeliverables.length > 0) {
      const prereqsDone = item.preRequisiteDeliverables.every((deliverableName) => {
        const deliverableCards = items.filter((candidate) => candidate.deliverable === deliverableName)
        return deliverableCards.length > 0 && deliverableCards.every((candidate) => candidate.done)
      })
      if (!prereqsDone) return false
    }

    return true
  })
}

function currentSimulationDate(model: SimModel, step: number): Date | null {
  const forecast = model.setup.forecastDate
  if (!forecast?.startDate) return null
  const startDate = parseSimDate(forecast.startDate)
  if (!startDate) return null
  const excludedSet = new Set(
    forecast.excludedDates
      .map((d) => parseSimDate(d))
      .filter((d): d is Date => d !== null)
      .map((d) => d.toDateString()),
  )
  const workUnits =
    model.execute.simulationType === 'scrum'
      ? step * forecast.workDaysPerIteration
      : Math.ceil(step / Math.max(1, forecast.intervalsToOneDay))
  return addWorkDays(startDate, workUnits, forecast.workDays, excludedSet)
}

function createScrumItems(model: SimModel) {
  const result: ScrumItem[] = []
  let sequence = 0
  const cos = model.setup.classOfServices
  getIncludedBacklogTemplates(model).forEach((template) => {
    const cosRef = resolveCos(cos, template.classOfService)
    for (let count = 0; count < template.count; count += 1) {
      const estimate = sampleBetween(
        template.estimateLowBound ?? 1,
        template.estimateHighBound ?? template.estimateLowBound ?? 1,
      )
      result.push({
        id: `s-${sequence + 1}`,
        label: formatBacklogItemLabel(model.setup.backlog.nameFormat, template, count + 1),
        deliverable: template.deliverable,
        templateName: template.name,
        estimate,
        completedPoints: template.completed
          ? estimate
          : 0,
        blockedPoints: 0,
        burnedBlockedPoints: 0,
        blockerLabel: undefined,
        done: template.completed,
        storyType: 'work',
        classOfServiceRef: cosRef,
        deliverableOrder: template.deliverableOrder,
        dueDate: template.dueDate,
        order: template.order,
        sortOrder: sequence,
        businessValue: sampleBetween(template.valueLowBound, template.valueHighBound),
      })
      sequence += 1
    }
  })
  if (model.setup.backlog.shuffle) shuffleArray(result)
  result.sort(compareWorkItemPriority)
  return result
}


function formatBacklogItemLabel(nameFormat: string | undefined, template: SimWorkItemTemplate, sequence: number) {
  const fallback = template.name.includes('{0}') ? template.name.replace('{0}', String(sequence)) : `${template.name} ${sequence}`
  if (!nameFormat) return fallback
  return nameFormat
    .replaceAll('{0}', String(sequence))
    .replaceAll('{1}', template.deliverable ?? '')
    .replaceAll('{2}', template.classOfService ?? '')
    .replaceAll('{3}', template.initialColumn != null ? String(template.initialColumn) : '')
    .replaceAll('{4}', String(Math.max(0, template.count - sequence)))
}

function selectAggregationValue(values: number[], aggregationValue: SimAggregationValue) {
  if (values.length === 0) return 0
  if (aggregationValue === 'Median') return computePercentile(values, 50)
  if (aggregationValue === 'Fifth') return computePercentile(values, 5)
  if (aggregationValue === 'NinetyFifth') return computePercentile(values, 95)
  if (aggregationValue === 'Max') return values[values.length - 1] ?? 0
  if (aggregationValue === 'Min') return values[0] ?? 0
  return round(values.reduce((sum, value) => sum + value, 0) / values.length)
}

function safePriorityDate(value?: string) {
  return parseSimDate(value ?? '')?.getTime() ?? Number.MAX_SAFE_INTEGER
}

function getIncludedBacklogTemplates(model: SimModel) {
  const deliverableFilter = model.execute.deliverables.length > 0
    ? new Set(model.execute.deliverables.map((value) => value.toLowerCase()))
    : null
  const skippedDeliverables = new Set<string>()
  const evaluatedDeliverables = new Set<string>()

  return model.setup.backlog.items.filter((template) => {
    if (!template.deliverable) {
      return deliverableFilter == null
    }

    const deliverableKey = template.deliverable.toLowerCase()
    if (deliverableFilter && !deliverableFilter.has(deliverableKey)) {
      return false
    }

    if (!evaluatedDeliverables.has(deliverableKey)) {
      evaluatedDeliverables.add(deliverableKey)
      if (
        (template.deliverableSkipPercentage ?? 0) > 0 &&
        Math.random() * 100 < (template.deliverableSkipPercentage ?? 0)
      ) {
        skippedDeliverables.add(deliverableKey)
      }
    }

    return !skippedDeliverables.has(deliverableKey)
  })
}

function compareTemplatePriority(a: SimWorkItemTemplate, b: SimWorkItemTemplate) {
  const byDeliverable = (a.deliverableOrder ?? Number.MAX_SAFE_INTEGER) - (b.deliverableOrder ?? Number.MAX_SAFE_INTEGER)
  if (byDeliverable !== 0) return byDeliverable
  const byBacklog = a.order - b.order
  if (byBacklog !== 0) return byBacklog
  const byDate = safePriorityDate(a.dueDate) - safePriorityDate(b.dueDate)
  if (byDate !== 0) return byDate
  return a.name.localeCompare(b.name)
}

function compareWorkItemPriority<
  T extends {
    deliverableOrder?: number
    order: number
    classOfServiceRef?: SimClassOfService
    dueDate?: string
  },
>(a: T, b: T) {
  const byDeliverable = (a.deliverableOrder ?? Number.MAX_SAFE_INTEGER) - (b.deliverableOrder ?? Number.MAX_SAFE_INTEGER)
  if (byDeliverable !== 0) return byDeliverable
  const byBacklog = a.order - b.order
  if (byBacklog !== 0) return byBacklog
  const byCos = (a.classOfServiceRef?.order ?? Number.MAX_SAFE_INTEGER) -
    (b.classOfServiceRef?.order ?? Number.MAX_SAFE_INTEGER)
  if (byCos !== 0) return byCos
  const byDate = safePriorityDate(a.dueDate) - safePriorityDate(b.dueDate)
  if (byDate !== 0) return byDate
  return 0
}

function sampleColumnDuration(column: SimColumn, item: KanbanItem, phaseEstimateMultiplier = 1) {
  // COS column overrides take highest precedence, then item overrides, then column defaults.
  const cosOverride = item.classOfServiceRef?.columnOverrides.find(
    (candidate) => candidate.columnId === column.id,
  )
  const itemOverride = item.columnOverrides.find(
    (candidate) => candidate.columnId === column.id,
  )
  const override = cosOverride ?? itemOverride

  // Skip percentage: override level
  if (override?.skipPercentage != null && Math.random() * 100 < override.skipPercentage) {
    return 0
  }
  // Skip percentage: base column level (only when no override skip set)
  if (override?.skipPercentage == null && column.skipPercentage > 0 && Math.random() * 100 < column.skipPercentage) {
    return 0
  }

  let low: number
  let high: number

  if (override?.estimateLowBound != null && override.estimateHighBound != null) {
    low = override.estimateLowBound
    high = override.estimateHighBound
  } else {
    const spread = column.estimateHighBound - column.estimateLowBound
    low = column.estimateLowBound + spread * (item.percentageLowBound / 100)
    high = column.estimateLowBound + spread * (item.percentageHighBound / 100)
  }

  if (low > high) [low, high] = [high, low]
  const raw = Math.max(0, high <= low ? high : sampleBetween(low, high))
  return raw * phaseEstimateMultiplier
}

function findNextColumnIndex(columns: SimColumn[], currentIndex: number, item: KanbanItem) {
  for (let index = currentIndex + 1; index < columns.length; index += 1) {
    const column = columns[index]
    const override = item.columnOverrides.find((candidate) => candidate.columnId === column.id)
    const skipChance = override?.skipPercentage ?? column.skipPercentage
    if (skipChance > 0 && Math.random() * 100 < skipChance) {
      continue
    }
    return index
  }
  return null
}

function pushKanbanSnapshot(
  snapshots: BoardSnapshot[],
  step: number,
  columns: SimColumn[],
  items: KanbanItem[],
  doneItems: KanbanItem[],
  phase: SimPhase | null = null,
) {
  snapshots.push({
    step,
    label: step === 0 ? 'Start' : `Interval ${step}`,
    backlogCount: items.filter((item) => !item.done && item.currentColumn < 0).length,
    doneCount: doneItems.length,
    activePhase: phase?.name || undefined,
    columns: columns.map((column, index) => ({
      id: String(column.id),
      label: column.name,
      wipLimit: getEffectiveWipLimit(column, phase),
      cards: items
        .filter((item) => !item.done && item.currentColumn === index)
        .map((item) => ({
          id: item.id,
          label: item.label,
          deliverable: item.deliverable,
          kind: toBoardCardKind(item.cardType),
          status: item.status === 'backlog' ? 'active' as const : item.status,
          isBlocked: item.status === 'blocked',
          blockerLabel: item.blockerLabel,
        })),
    })),
  })
}

function pushScrumSnapshot(
  snapshots: BoardSnapshot[],
  step: number,
  backlog: ScrumItem[],
  committed: ScrumItem[],
  done: ScrumItem[],
  committedPoints: number,
  phase: SimPhase | null = null,
) {
  snapshots.push({
    step,
    label: step === 0 ? 'Backlog' : `Iteration ${step}`,
    backlogCount: backlog.filter((item) => !item.done).length,
    doneCount: done.length,
    activePhase: phase?.name || undefined,
    columns: [
      {
        id: 'backlog',
        label: 'Remaining backlog',
        cards: backlog.filter((item) => !item.done).slice(0, 24).map((item) => ({
          id: item.id,
          label: `${item.label} (${item.estimate})`,
          deliverable: item.deliverable,
          kind: toBoardCardKind(item.storyType),
          isBlocked: item.blockedPoints > item.burnedBlockedPoints,
          blockerLabel: item.blockerLabel,
        })),
      },
      {
        id: 'committed',
        label: `Committed (${round(committedPoints)} pts)`,
        cards: committed.map((item) => ({
          id: item.id,
          label: `${item.label} (${item.estimate})`,
          deliverable: item.deliverable,
          kind: toBoardCardKind(item.storyType),
          isBlocked: item.blockedPoints > item.burnedBlockedPoints,
          blockerLabel: item.blockerLabel,
        })),
      },
      {
        id: 'done',
        label: `Done (${done.length})`,
        cards: done.slice(-24).map((item) => ({
          id: item.id,
          label: `${item.label} (${item.estimate})`,
          deliverable: item.deliverable,
          kind: toBoardCardKind(item.storyType),
          isBlocked: false,
        })),
      },
    ],
  })
}

function shouldBlockingApply(
  blockingEvent: SimBlockingEvent,
  cardType: KanbanItem['cardType'] | ScrumItem['storyType'],
) {
  if (cardType === 'defect') return blockingEvent.blockDefects
  if (cardType === 'addedScope') return blockingEvent.blockAddedScope
  return blockingEvent.blockWork
}

function blockingEventApplies(
  blockingEvent: SimBlockingEvent,
  item:
    | Pick<KanbanItem, 'cardType' | 'deliverable' | 'templateName'>
    | Pick<ScrumItem, 'storyType' | 'deliverable' | 'templateName'>,
  phase: SimPhase | null,
) {
  if (blockingEvent.phases.length > 0 && (!phase || !blockingEvent.phases.includes(phase.name))) {
    return false
  }

  const cardType = 'cardType' in item ? item.cardType : item.storyType
  if (!shouldBlockingApply(blockingEvent, cardType)) return false

  if (blockingEvent.targetDeliverable && item.deliverable !== blockingEvent.targetDeliverable) {
    return false
  }

  if (blockingEvent.targetCustomBacklog && item.templateName !== blockingEvent.targetCustomBacklog) {
    return false
  }

  return true
}

function resolveKanbanStartColumn(columns: SimColumn[], startsInColumnId?: number) {
  if (startsInColumnId == null) return -1
  return columns.findIndex((column) => column.id === startsInColumnId)
}

function toBoardCardKind(
  cardType: KanbanItem['cardType'] | ScrumItem['storyType'],
): BoardCard['kind'] {
  if (cardType === 'defect') return 'defect'
  if (cardType === 'addedScope') return 'addedScope'
  return 'story'
}

function formatSpawnedLabel(name: string, id: number) {
  return name.includes('{0}') ? name.replace('{0}', String(id)) : `${name} ${id}`
}

function forecastStepToDate(model: SimModel, steps: number) {
  const forecast = model.setup.forecastDate
  if (!forecast?.startDate) {
    return null
  }

  const startDate = parseSimDate(forecast.startDate)
  if (!startDate) {
    return null
  }

  const excludedSet = new Set(
    forecast.excludedDates
      .map((d) => parseSimDate(d))
      .filter((d): d is Date => d !== null)
      .map((d) => d.toDateString()),
  )

  const workUnits =
    model.execute.simulationType === 'scrum'
      ? steps * forecast.workDaysPerIteration
      : Math.ceil(steps / Math.max(1, forecast.intervalsToOneDay))

  const date = addWorkDays(startDate, workUnits, forecast.workDays, excludedSet)
  return date.toLocaleDateString(model.locale || 'en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function estimateTotalCost(model: SimModel, steps: number) {
  const forecast = model.setup.forecastDate
  if (!forecast?.costPerDay) {
    return 0
  }
  const workDays =
    model.execute.simulationType === 'scrum'
      ? steps * forecast.workDaysPerIteration
      : Math.ceil(steps / Math.max(1, forecast.intervalsToOneDay))
  return round(workDays * forecast.costPerDay)
}

function parseSimDate(value: string) {
  const normalized = value
    .replace(/(\d{2})([A-Za-z]{3})(\d{4})/, '$1 $2 $3')
    .replace(/-/g, ' ')
    .trim()
  const date = new Date(normalized)
  return Number.isNaN(date.getTime()) ? null : date
}

function addWorkDays(startDate: Date, workDaysToAdd: number, allowedDays: number[], excludedDates?: Set<string>) {
  const date = new Date(startDate)
  let remaining = Math.max(0, workDaysToAdd)
  while (remaining > 0) {
    date.setDate(date.getDate() + 1)
    if (allowedDays.includes(date.getDay()) && (!excludedDates || !excludedDates.has(date.toDateString()))) {
      remaining -= 1
    }
  }
  return date
}

function stepMatchesInterval(step: number, interval?: number) {
  if (!interval || interval <= 1) return true
  return step % interval === 0
}


function normalizeOccurrence(scale: number, value: number) {
  if (value <= 0) return Number.POSITIVE_INFINITY
  if (scale === 1) return value
  return scale / value
}

function sampleOccurrenceTrigger(event: SimBlockingEvent, phaseOccurrenceMultiplier: number) {
  if (event.occurrenceType === 'percentage') {
    const pct = Math.max(0, Math.min(100, sampleBetween(event.occurrenceLowBound, event.occurrenceHighBound)))
    const trigger = normalizeOccurrence(100, pct)
    return Math.max(1, Math.round(trigger * phaseOccurrenceMultiplier))
  }
  const low = normalizeOccurrence(event.scale, event.occurrenceLowBound)
  const high = normalizeOccurrence(event.scale, event.occurrenceHighBound)
  const sampled = sampleBetween(Math.min(low, high), Math.max(low, high))
  return Math.max(1, Math.round(sampled * phaseOccurrenceMultiplier))
}

function calculateCostOfDelay(model: SimModel, completionDate: string | null) {
  const forecast = model.setup.forecastDate
  if (!forecast?.targetDate || !completionDate || forecast.revenue <= 0) return 0
  const target = parseSimDate(forecast.targetDate)
  const completion = parseSimDate(completionDate)
  if (!target || !completion || completion <= target) return 0
  const daysLate = Math.max(0, Math.ceil((completion.getTime() - target.getTime()) / (1000 * 60 * 60 * 24)))
  const perDay =
    forecast.revenueUnit === 'day'
      ? forecast.revenue
      : forecast.revenueUnit === 'week'
      ? forecast.revenue / 7
      : forecast.revenueUnit === 'year'
      ? forecast.revenue / 365
      : forecast.revenue / 30
  return round(daysLate * perDay)
}

function sampleBetween(low: number, high: number) {
  if (high <= low) return low
  return Math.random() * (high - low) + low
}

/** Fisher-Yates shuffle (in-place). */
function shuffleArray<T>(arr: T[]): T[] {
  for (let i = arr.length - 1; i > 0; i -= 1) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[arr[i], arr[j]] = [arr[j], arr[i]]
  }
  return arr
}

/**
 * Determine the active phase based on the unit type.
 * - percentage: based on share of original backlog items pulled / started.
 * - interval / iteration: based on the current step number.
 */
function resolvePhase(
  phases: SimPhases | undefined,
  step: number,
  itemsPulled: number,
  originalItemCount: number,
): SimPhase | null {
  if (!phases || phases.phases.length === 0) return null

  if (phases.unit === 'interval' || phases.unit === 'iteration') {
    return phases.phases.find((p) => step >= p.start && step <= p.end) ?? null
  }

  // percentage
  const pct =
    originalItemCount <= 0
      ? 100
      : Math.min(100, (itemsPulled / originalItemCount) * 100)
  return phases.phases.find((p) => pct >= p.start && pct <= p.end) ?? null
}

/** Return the effective WIP limit for a column, accounting for phase overrides. */
function getEffectiveWipLimit(column: SimColumn, phase: SimPhase | null): number {
  if (phase) {
    const override = phase.columns.find((c) => c.columnId === column.id)
    if (override && override.wipLimit > 0) return override.wipLimit
  }
  return column.wipLimit <= 0 ? Number.POSITIVE_INFINITY : column.wipLimit
}

/** Resolve a class-of-service reference by name, falling back to the default COS. */
function resolveCos(
  classOfServices: SimClassOfService[],
  cosName?: string,
): SimClassOfService | undefined {
  if (!classOfServices.length) return undefined
  if (cosName) {
    const match = classOfServices.find(
      (cos) => cos.name.toLowerCase() === cosName.toLowerCase(),
    )
    if (match) return match
  }
  return classOfServices.find((cos) => cos.isDefault) || undefined
}

/** Buffer passthrough – try to push an item through buffer columns immediately. */
function tryPassThroughBuffer(
  item: KanbanItem,
  bufferIdx: number,
  columns: SimColumn[],
  allItems: KanbanItem[],
  doneItems: KanbanItem[],
  step: number,
  phase: SimPhase | null = null,
) {
  const nextIdx = findNextColumnIndex(columns, bufferIdx, item)
  if (nextIdx === null) {
    item.done = true
    item.status = 'done'
    doneItems.push(item)
    return
  }
  const nextColumn = columns[nextIdx]
  if (!stepMatchesInterval(step, nextColumn.replenishInterval)) return
  const activeInTarget = allItems.filter((c) => !c.done && c.currentColumn === nextIdx).length
  const wipLimit = getEffectiveWipLimit(nextColumn, phase)
  if (activeInTarget >= wipLimit) return

  item.currentColumn = nextIdx
  item.calculatedWork = nextColumn.isBuffer ? 0 : sampleColumnDuration(nextColumn, item, phase?.estimateMultiplier ?? 1)
  item.timeSoFar = 0
  item.blockedTime = 0
  item.status = nextColumn.isBuffer ? 'queued' : 'active'
  // Recursive for chained buffers
  if (nextColumn.isBuffer) {
    tryPassThroughBuffer(item, nextIdx, columns, allItems, doneItems, step, phase)
  }
}

/**
 * When a WIP-violating card enters a column, block the lowest-priority active card
 * in that column to compensate (excluding other WIP violators).
 */
function blockLowestPriorityCard(allItems: KanbanItem[], columnIdx: number, excludeItem: KanbanItem) {
  const candidates = allItems.filter(
    (c) =>
      !c.done &&
      c.currentColumn === columnIdx &&
      c !== excludeItem &&
      c.status === 'active' &&
      !c.classOfServiceRef?.violateWip,
  )
  if (candidates.length === 0) return
  // Lowest priority = highest COS order number
  candidates.sort(
    (a, b) => (b.classOfServiceRef?.order ?? 999) - (a.classOfServiceRef?.order ?? 999),
  )
  candidates[0].status = 'blocked'
  candidates[0].blockedTime += 1
}

/** Fire added-scope and defect events on Scrum story completion. */
function fireScrumCompletionEvents(
  addedScopeProcessors: { def: SimAddedScope; completedSeen: number; trigger: number }[],
  defectProcessors: { def: SimDefect; storiesSeen: number; trigger: number }[],
  backlog: ScrumItem[],
  nextItemId: number,
) {
  let id = nextItemId
  addedScopeProcessors.forEach((asp) => {
    asp.completedSeen += 1
    if (asp.completedSeen >= asp.trigger) {
      asp.completedSeen = 0
      asp.trigger = Math.round(sampleBetween(asp.def.occurrenceLowBound, asp.def.occurrenceHighBound))
      for (let c = 0; c < asp.def.count; c += 1) {
        backlog.push({
          id: `as-${id++}`,
          label: formatSpawnedLabel(asp.def.name, id),
          estimate: sampleBetween(asp.def.estimateLowBound, asp.def.estimateHighBound),
          completedPoints: 0,
          blockedPoints: 0,
          burnedBlockedPoints: 0,
          blockerLabel: undefined,
          done: false,
          storyType: 'addedScope',
          order: Number.MAX_SAFE_INTEGER,
        })
      }
    }
  })
  defectProcessors.forEach((dp) => {
    dp.storiesSeen += 1
    if (dp.storiesSeen >= dp.trigger) {
      dp.storiesSeen = 0
      dp.trigger = Math.round(sampleBetween(dp.def.occurrenceLowBound, dp.def.occurrenceHighBound))
      for (let c = 0; c < dp.def.count; c += 1) {
        backlog.push({
          id: `def-${id++}`,
          label: formatSpawnedLabel(dp.def.name, id),
          estimate: sampleBetween(dp.def.estimateLowBound, dp.def.estimateHighBound),
          completedPoints: 0,
          blockedPoints: 0,
          burnedBlockedPoints: 0,
          blockerLabel: undefined,
          done: false,
          storyType: 'defect',
          order: Number.MAX_SAFE_INTEGER,
        })
      }
    }
  })
}

/**
 * Compute the percentile of a SORTED array using the Aczel algorithm
 * (same as the C# `computePercentile` method).
 */
function computePercentile(sortedValues: number[], pct: number): number {
  if (sortedValues.length === 0) return 0
  if (pct >= 100) return sortedValues[sortedValues.length - 1]
  if (pct <= 0) return sortedValues[0]

  const count = sortedValues.length
  const n = (pct / 100) * (count - 1) + 1
  const leftIndex = Math.max(0, Math.floor(n) - 1)
  const rightIndex = Math.min(count - 1, Math.floor(n))
  const leftValue = sortedValues[leftIndex]
  const rightValue = sortedValues[rightIndex]
  if (leftValue === rightValue) return leftValue
  const fraction = n - Math.floor(n)
  return leftValue + fraction * (rightValue - leftValue)
}

/** Sample standard deviation. */
function sampleStdDev(values: number[]): number {
  if (values.length <= 1) return 0
  const n = values.length
  let sum = 0
  let sumSq = 0
  for (const v of values) {
    sum += v
    sumSq += v * v
  }
  const variance = (sumSq - (sum * sum) / n) / (n - 1)
  return Math.sqrt(Math.max(0, variance))
}


function applyDecimalRoundingToRunResult(result: SimulationRunResult, decimals: number): SimulationRunResult {
  const roundToDecimals = (value: number) => round(value, decimals)
  return {
    ...result,
    totalCost: roundToDecimals(result.totalCost),
    costOfDelay: roundToDecimals(result.costOfDelay),
    valueDeliveredByStep: result.valueDeliveredByStep.map((entry) => ({
      ...entry,
      cumulativeValue: roundToDecimals(entry.cumulativeValue),
    })),
  }
}

function applyDecimalRoundingToMonteCarloResult(result: MonteCarloResult, decimals: number): MonteCarloResult {
  const roundToDecimals = (value: number) => round(value, decimals)
  return {
    ...result,
    averageSteps: roundToDecimals(result.averageSteps),
    medianSteps: roundToDecimals(result.medianSteps),
    minSteps: roundToDecimals(result.minSteps),
    maxSteps: roundToDecimals(result.maxSteps),
    standardDeviation: roundToDecimals(result.standardDeviation),
    summarySteps: roundToDecimals(result.summarySteps),
    percentileSteps: result.percentileSteps.map((row) => ({
      ...row,
      steps: roundToDecimals(row.steps),
    })),
  }
}

function applyDecimalRoundingToSensitivityResult(
  result: SensitivityResult,
  decimals: number,
): SensitivityResult {
  const roundToDecimals = (value: number) => round(value, decimals)
  return {
    ...result,
    baselineAverageSteps: roundToDecimals(result.baselineAverageSteps),
    tests: result.tests.map((test) => ({
      ...test,
      averageSteps: roundToDecimals(test.averageSteps),
      deltaSteps: roundToDecimals(test.deltaSteps),
      deltaPercent: roundToDecimals(test.deltaPercent),
    })),
  }
}

function round(value: number, decimals = 2) {
  const factor = 10 ** Math.max(0, decimals)
  return Math.round(value * factor) / factor
}
