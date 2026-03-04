export type SimulationKind = 'kanban' | 'scrum'

export interface SimulationExampleFile {
  id: string
  title: string
  group: string
  section: string
  path: string
  source: string
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
  order: number
  estimateLowBound?: number
  estimateHighBound?: number
  percentageLowBound: number
  percentageHighBound: number
  initialColumn?: number
  deliverable?: string
  preRequisiteDeliverables: string[]
  earliestStartDate?: string
  classOfService?: string
  columnOverrides: SimColumnOverride[]
}

export type SimPullOrder = 'randomAfterOrdering' | 'random' | 'indexSequence' | 'FIFO' | 'FIFOStrict'

export interface SimBacklog {
  type: 'simple' | 'custom'
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
  excludedDates: string[]
}

export interface SimExecute {
  simulationType: SimulationKind
  dateFormat: string
  limitIntervalsTo: number
  monteCarloCycles: number
  sensitivityCycles: number
  sensitivityEstimateMultiplier: number
  sensitivityOccurrenceMultiplier: number
  sensitivityIterationMultiplier: number
  pullOrder: SimPullOrder
}

export interface SimIteration {
  storyPointsPerIterationLowBound: number
  storyPointsPerIterationHighBound: number
  allowedToOverAllocate: boolean
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
    .map(([path, source]) => {
      const normalized = path.replace(/\\/g, '/')
      const relative = normalized.split('FocusedObjective.KanbanAndScrumSim/')[1] ?? normalized
      const segments = relative.split('/')
      const title = segments.at(-1)?.replace(/\.[^.]+$/, '') ?? relative
      return {
        id: relative.toLowerCase(),
        title,
        group: segments[0] ?? 'Examples',
        section: segments.slice(1, -1).join(' / ') || 'Root',
        path: relative,
        source,
      }
    })
    .sort((a, b) => a.path.localeCompare(b.path))
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
  const forecastDate = parseForecastDate(setup)
  const defects = parseDefects(setup)
  const blockingEvents = parseBlockingEvents(setup)
  const addedScopes = parseAddedScopes(setup)
  const phases = parsePhases(setup)
  const classOfServices = parseClassOfServices(setup)

  if (simulationType === 'kanban' && columns.length === 0) {
    throw new Error('Kanban simulations require at least one <column>.')
  }

  if (simulationType === 'scrum' && !iteration) {
    throw new Error('Scrum simulations require an <iteration> definition.')
  }

  return {
    name: readAttr(root, 'name') || 'Untitled simulation',
    locale: readAttr(root, 'locale') || 'en-US',
    example: readChildText(root, 'example'),
    execute: {
      simulationType,
      dateFormat: readAttr(execute, 'dateFormat') || 'yyyyMMdd',
      limitIntervalsTo: readNumber(execute, 'limitIntervalsTo', 9000),
      monteCarloCycles: readNumber(execute.querySelector(':scope > monteCarlo'), 'cycles', 500),
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
    },
    setup: {
      backlog,
      columns,
      iteration,
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

  if (backlogType === 'simple') {
    const count = readNumber(backlog, 'simpleCount', 0)
    return {
      type: 'simple',
      simpleCount: count,
      shuffle,
      items: Array.from({ length: count }, (_, index) => ({
        id: `simple-${index + 1}`,
        name: `Story ${index + 1}`,
        count: 1,
        order: index + 1,
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
    context?: { deliverable?: string; preRequisiteDeliverables: string[]; earliestStartDate?: string },
  ) => {
    const count = Math.max(1, readNumber(node, 'count', 1))
    items.push({
      id: `${context?.deliverable || 'backlog'}-${sequence++}`,
      name: readAttr(node, 'name') || node.textContent.trim() || `Item ${sequence}`,
      count,
      order: readNumber(node, 'order', Number.MAX_SAFE_INTEGER),
      estimateLowBound: readOptionalNumber(node, 'estimateLowBound'),
      estimateHighBound: readOptionalNumber(node, 'estimateHighBound'),
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
    const skipPct = readNumber(deliverable, 'skipPercentage', 0)
    const earliestStartDate = readAttr(deliverable, 'earliestStartDate') || undefined
    const preRequisiteDeliverables = (readAttr(deliverable, 'preRequisiteDeliverables') || '')
      .split('|')
      .map((part) => part.trim())
      .filter(Boolean)
    if (skipPct > 0 && Math.random() * 100 < skipPct) return
    Array.from(deliverable.querySelectorAll(':scope > custom')).forEach((node) =>
      pushItem(node, { deliverable: name, preRequisiteDeliverables, earliestStartDate }),
    )
  })

  if (simulationType === 'scrum' && items.length === 0) {
    throw new Error('Scrum custom backlogs need at least one <custom> entry.')
  }

  return {
    type: 'custom',
    simpleCount: 0,
    shuffle,
    items: items.sort((a, b) => a.order - b.order || a.name.localeCompare(b.name)),
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
  order: number
  pullOrder: number
  preRequisiteDeliverables: string[]
  earliestStartDate?: string
}

interface ScrumItem {
  id: string
  label: string
  deliverable?: string
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
  order: number
}

export function runVisualSimulation(model: SimModel): SimulationRunResult {
  return model.execute.simulationType === 'kanban'
    ? runKanbanSimulation(model)
    : runScrumSimulation(model)
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
  return {
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
  }
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

  return {
    baselineAverageSteps: baseline.averageSteps,
    tests: tests.sort((a, b) => a.deltaSteps - b.deltaSteps),
  }
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
  const doneItems: KanbanItem[] = []
  const snapshots: BoardSnapshot[] = []
  let step = 0
  let nextItemId = items.length + 1
  let nextPullOrder = items.length + 1
  let itemsPulled = 0
  let currentPhase: SimPhase | null = null
  let accumulatedCost = 0

  // -- Event processor state -------------------------------------------------
  const defectProcessors = model.setup.defects.map((d) => ({
    def: d,
    cardsSeen: 0,
    trigger: Math.round(sampleBetween(d.occurrenceLowBound, d.occurrenceHighBound)),
  }))
  const blockingProcessors = model.setup.blockingEvents.map((b) => ({
    def: b,
    cardsSeen: 0,
    trigger: Math.round(sampleBetween(b.occurrenceLowBound, b.occurrenceHighBound)),
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

  // Place items with initialColumn
  items.forEach((item) => {
    if (item.currentColumn >= 0) {
      const col = columns[item.currentColumn]
      item.calculatedWork = col.isBuffer ? 0 : sampleColumnDuration(col, item, phaseEstMult())
      item.timeSoFar = 0
      item.status = 'active'
      item.pullOrder = nextPullOrder++
    }
  })

  pushKanbanSnapshot(snapshots, step, columns, items, doneItems, currentPhase)

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
          if (bp.def.columnId === column.id && shouldBlockingApply(bp.def, item.cardType)) {
            bp.cardsSeen += 1
            if (bp.cardsSeen >= bp.trigger) {
              bp.cardsSeen = 0
              bp.trigger = Math.round(
                sampleBetween(bp.def.occurrenceLowBound, bp.def.occurrenceHighBound) * phaseOccMult(),
              )
              item.blockedTime += sampleBetween(bp.def.estimateLowBound, bp.def.estimateHighBound) * phaseEstMult()
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
  }
}

function runScrumSimulation(model: SimModel): SimulationRunResult {
  const iteration = model.setup.iteration
  if (!iteration) {
    throw new Error('Scrum simulations require iteration settings.')
  }

  const backlog = createScrumItems(model)
  const originalStoryCount = backlog.length
  const done: ScrumItem[] = []
  const snapshots: BoardSnapshot[] = []
  let step = 0
  let nextItemId = backlog.length + 1
  let storiesStarted = 0
  let currentPhase: SimPhase | null = null
  let accumulatedCost = 0
  const cosActive = model.setup.classOfServices.length > 0

  // -- Event processors for scrum ------------------------------------------
  const defectProcessors = model.setup.defects.map((d) => ({
    def: d,
    storiesSeen: 0,
    trigger: Math.round(sampleBetween(d.occurrenceLowBound, d.occurrenceHighBound)),
  }))
  const blockingProcessors = model.setup.blockingEvents.map((b) => ({
    def: b,
    storiesSeen: 0,
    trigger: Math.round(sampleBetween(b.occurrenceLowBound, b.occurrenceHighBound)),
  }))
  const addedScopeProcessors = model.setup.addedScopes.map((a) => ({
    def: a,
    completedSeen: 0,
    trigger: Math.round(sampleBetween(a.occurrenceLowBound, a.occurrenceHighBound)),
  }))

  // Resolve initial phase
  currentPhase = resolvePhase(model.setup.phases, 0, 0, originalStoryCount)

  pushScrumSnapshot(snapshots, step, backlog, [], done, 0, currentPhase)

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

    // Accumulate cost
    const forecast = model.setup.forecastDate
    if (forecast) {
      const phaseCost = currentPhase?.costPerDay && currentPhase.costPerDay > 0 ? currentPhase.costPerDay : forecast.costPerDay
      if (phaseCost > 0) {
        accumulatedCost += phaseCost * forecast.workDaysPerIteration
      }
    }

    const rawCapacity = sampleBetween(
      iteration.storyPointsPerIterationLowBound,
      iteration.storyPointsPerIterationHighBound,
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

      if (pointsRemaining >= storyRemaining || iteration.allowedToOverAllocate) {
        inProgress.push(story)

        // Track story start for percentage-based phases
        storiesStarted += 1
        if (model.setup.phases?.unit === 'percentage') {
          currentPhase = resolvePhase(model.setup.phases, step, storiesStarted, originalStoryCount)
        }

        // Check blocking events on story start (apply phase occurrence multiplier)
        for (const bp of blockingProcessors) {
          if (!shouldBlockingApply(bp.def, story.storyType)) continue
          bp.storiesSeen += 1
          if (bp.storiesSeen >= bp.trigger) {
            bp.storiesSeen = 0
            bp.trigger = Math.round(
              sampleBetween(bp.def.occurrenceLowBound, bp.def.occurrenceHighBound) * phaseOccMult,
            )
            story.blockedPoints += sampleBetween(bp.def.estimateLowBound, bp.def.estimateHighBound) * phaseEstMult
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
  }
}

function createKanbanItems(model: SimModel) {
  const result: KanbanItem[] = []
  let sequence = 0
  const cos = model.setup.classOfServices
  model.setup.backlog.items.forEach((template) => {
    const cosRef = resolveCos(cos, template.classOfService)
    for (let count = 0; count < template.count; count += 1) {
      result.push({
        id: `k-${sequence + 1}`,
        label: template.name.includes('{0}') ? template.name.replace('{0}', String(count + 1)) : `${template.name} ${count + 1}`,
        deliverable: template.deliverable,
        templateName: template.name,
        currentColumn: template.initialColumn
          ? Math.max(0, model.setup.columns.findIndex((column) => column.id === template.initialColumn))
          : -1,
        timeSoFar: 0,
        calculatedWork: 0,
        blockedTime: 0,
        blockerLabel: undefined,
        done: false,
        status: template.initialColumn ? 'active' : 'backlog',
        columnOverrides: template.columnOverrides,
        percentageLowBound: template.percentageLowBound,
        percentageHighBound: template.percentageHighBound,
        cardType: 'work',
        classOfServiceRef: cosRef,
        order: template.order,
        pullOrder: sequence,
        preRequisiteDeliverables: template.preRequisiteDeliverables,
        earliestStartDate: template.earliestStartDate,
      })
      sequence += 1
    }
  })
  if (model.setup.backlog.shuffle) shuffleArray(result)
  // Sort by COS order (lower = higher priority) for pull ordering
  if (cos.length > 0 && !model.setup.backlog.shuffle) {
    result.sort((a, b) => (a.classOfServiceRef?.order ?? 999) - (b.classOfServiceRef?.order ?? 999) || a.order - b.order)
  }
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
      const byCos = (a.item.classOfServiceRef?.order ?? Number.MAX_SAFE_INTEGER) -
        (b.item.classOfServiceRef?.order ?? Number.MAX_SAFE_INTEGER)
      if (byCos !== 0) return byCos
      const byRandom = a.random - b.random
      if (byRandom !== 0) return byRandom
      return a.item.order - b.item.order
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
        return deliverableCards.length === 0 || deliverableCards.every((candidate) => candidate.done)
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
  const workUnits =
    model.execute.simulationType === 'scrum'
      ? step * forecast.workDaysPerIteration
      : Math.ceil(step / Math.max(1, forecast.intervalsToOneDay))
  return addWorkDays(startDate, workUnits, forecast.workDays)
}

function createScrumItems(model: SimModel) {
  const result: ScrumItem[] = []
  let sequence = 0
  const cos = model.setup.classOfServices
  model.setup.backlog.items.forEach((template) => {
    const cosRef = resolveCos(cos, template.classOfService)
    for (let count = 0; count < template.count; count += 1) {
      result.push({
        id: `s-${sequence + 1}`,
        label: template.name.includes('{0}') ? template.name.replace('{0}', String(count + 1)) : `${template.name} ${count + 1}`,
        deliverable: template.deliverable,
        estimate: sampleBetween(template.estimateLowBound ?? 1, template.estimateHighBound ?? template.estimateLowBound ?? 1),
        completedPoints: 0,
        blockedPoints: 0,
        burnedBlockedPoints: 0,
        blockerLabel: undefined,
        done: false,
        storyType: 'work',
        classOfServiceRef: cosRef,
        order: template.order,
      })
      sequence += 1
    }
  })
  if (model.setup.backlog.shuffle) shuffleArray(result)
  if (cos.length > 0 && !model.setup.backlog.shuffle) {
    result.sort((a, b) => (a.classOfServiceRef?.order ?? 999) - (b.classOfServiceRef?.order ?? 999) || a.order - b.order)
  }
  return result
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

function round(value: number) {
  return Math.round(value * 100) / 100
}
