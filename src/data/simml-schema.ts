// ─── SimML Schema Reference ────────────────────────────────────────────────
// Reverse-engineered from C# [SimMLElement] and [SimMLAttribute] decorators
// in FocusedObjective.Contract (KanbanAndScrumSim project).
// ────────────────────────────────────────────────────────────────────────────

export interface SimMLSchemaAttribute {
  name: string
  description: string
  mandatory: boolean
  validValues?: string[]
  defaultValue?: string
  type?: string
  /** Indicates the attribute is only relevant for a specific sim type */
  simType?: 'kanban' | 'scrum'
}

export interface SimMLSchemaElement {
  /** XML tag name */
  tag: string
  /** Human-readable display name */
  displayName: string
  /** Full description from the SimMLElement decorator */
  description: string
  /** Whether this element is mandatory in its parent */
  mandatory: boolean
  /** C# class that implements this element */
  csharpClass: string
  /** C# source file path (relative to KanbanAndScrumSim/) */
  csharpFile: string
  /** All XML attributes on this element */
  attributes: SimMLSchemaAttribute[]
  /** Child elements (by tag name) */
  children: SimMLSchemaElement[]
  /** Notes / extra context */
  notes?: string
}

// ─── Full schema tree ──────────────────────────────────────────────────────

export const SIMML_SCHEMA: SimMLSchemaElement = {
  tag: 'simulation',
  displayName: 'simulation',
  description:
    'Root element of every SimML document. Defines a complete simulation model containing execution parameters and setup configuration.',
  mandatory: true,
  csharpClass: 'SimulationData',
  csharpFile: 'FocusedObjective.Contract/Data/SimulationData.cs',
  attributes: [
    {
      name: 'name',
      description: 'A user-defined name for this simulation model. For reference only.',
      mandatory: false,
      defaultValue: '""',
      type: 'string',
    },
    {
      name: 'locale',
      description:
        'ISO locale culture string (e.g. "en-US"). If omitted, the culture of the environment running the simulation engine will be used.',
      mandatory: false,
      defaultValue: 'System culture',
      type: 'string',
    },
  ],
  children: [
    // ── <?parameter ?> / <?variable ?> ─────────────────────────────────
    {
      tag: '?parameter',
      displayName: 'parameter (processing instruction)',
      description:
        'Declares a reusable variable that can be referenced by @name throughout the SimML document. Values are substituted before parsing. Also supports <?variable> syntax.',
      mandatory: false,
      csharpClass: 'VariableData',
      csharpFile: 'FocusedObjective.Contract/Data/VariableData.cs',
      attributes: [
        {
          name: 'name',
          description:
            'Unique name to identify this parameter. Referenced by prefixing with "@". Must not contain @, <, >, \\, or /.',
          mandatory: true,
          type: 'string',
        },
        {
          name: 'value',
          description: 'The value assigned when the parameter is referenced by @[name] elsewhere.',
          mandatory: true,
          type: 'string',
        },
        {
          name: 'type',
          description: 'Includes this parameter for interactive experiments.',
          mandatory: false,
          validValues: ['number', 'date'],
          type: 'string',
        },
        {
          name: 'lowest',
          description:
            'Lowest allowed numeric value for interactive experiments. Number types only.',
          mandatory: false,
          type: 'number',
        },
        {
          name: 'highest',
          description:
            'Highest allowed numeric value for interactive experiments. Number types only.',
          mandatory: false,
          type: 'number',
        },
        {
          name: 'step',
          description: 'Increment step size for interactive experiments. Number types only.',
          mandatory: false,
          type: 'number',
        },
      ],
      children: [],
      notes:
        'Processing instructions use the syntax <?parameter name="@x" value="5"?> or <?variable name="@x" value="5"?>. Evaluated before XML parsing. Values can reference other variables and contain simple arithmetic expressions.',
    },

    // ── <execute> ──────────────────────────────────────────────────────
    {
      tag: 'execute',
      displayName: 'execute',
      description:
        'Defines how the simulation is run: simulation type, cycle limits, output formatting, and which analysis modes (visual, Monte Carlo, sensitivity) to perform.',
      mandatory: true,
      csharpClass: 'ExecuteData',
      csharpFile: 'FocusedObjective.Contract/Data/ExecuteData.cs',
      attributes: [
        {
          name: 'type',
          description: 'Simulation model type.',
          mandatory: false,
          validValues: ['kanban', 'scrum'],
          defaultValue: 'kanban',
          type: 'enum',
        },
        {
          name: 'limitIntervalsTo',
          description:
            'Maximum number of simulation steps. The simulation returns even if not all work is completed by this limit.',
          mandatory: false,
          defaultValue: '9000',
          type: 'integer',
        },
        {
          name: 'intervalUnit',
          description:
            'Label describing what each simulation step represents (e.g. "days", "hours").',
          mandatory: false,
          defaultValue: '"days"',
          type: 'string',
        },
        {
          name: 'decimalRounding',
          description: 'Number of decimal places numbers are rounded to in output.',
          mandatory: false,
          defaultValue: '3',
          type: 'integer',
        },
        {
          name: 'dateFormat',
          description:
            'Date format string used to parse date values throughout the model (e.g. "dd-MMM-yyyy", "yyyyMMdd").',
          mandatory: false,
          defaultValue: '"yyyyMMdd"',
          type: 'string',
        },
        {
          name: 'currencyFormat',
          description: 'Currency format string for cost output.',
          mandatory: false,
          defaultValue: '"C2"',
          type: 'string',
        },
        {
          name: 'percentageFormat',
          description: 'Percentage format string for output.',
          mandatory: false,
          defaultValue: '"P"',
          type: 'string',
        },
        {
          name: 'aggregationValue',
          description:
            'Statistical aggregation method used to summarize Monte Carlo results into a single forecast figure.',
          mandatory: false,
          validValues: [
            'average',
            'median',
            'fifth',
            'twentyfifth',
            'seventyfifth',
            'ninetyfifth',
          ],
          defaultValue: 'average',
          type: 'enum',
        },
        {
          name: 'deliverables',
          description:
            'Pipe-separated list of deliverable names to include in the simulation. When empty (default), all deliverables are included. Allows one SimML to power multiple subset forecasts.',
          mandatory: false,
          defaultValue: '""',
          type: 'string',
        },
        {
          name: 'defaultDistribution',
          description:
            'Default distribution shape applied to estimate bounds when no distribution is explicitly specified.',
          mandatory: false,
          validValues: ['uniform', 'weibull'],
          defaultValue: '"uniform"',
          type: 'enum',
        },
        {
          name: 'completePercentage',
          description:
            'Lowest percentage of completed work items before the simulation is recorded as complete. Allows early termination (e.g. 85% done).',
          mandatory: false,
          defaultValue: '100.0',
          type: 'number',
        },
        {
          name: 'activePositionsCompletePercentage',
          description:
            'When active cards on the board fall below this percentage AND completePercentage is satisfied, the simulation ends. Useful for tail-end optimization.',
          mandatory: false,
          defaultValue: '0.0',
          type: 'number',
        },
        {
          name: 'pullOrder',
          description:
            'Controls how items are selected and processed when multiple items could move in the same interval.',
          mandatory: false,
          validValues: ['afterOrdering', 'random', 'index', 'fifo', 'fifoStrict'],
          defaultValue: 'afterOrdering',
          type: 'enum',
        },
        {
          name: 'basePath',
          description:
            'Base file path for resolving external references (distribution data files, includes).',
          mandatory: false,
          defaultValue: '""',
          type: 'string',
        },
      ],
      children: [
        // <visual>
        {
          tag: 'visual',
          displayName: 'visual',
          description:
            'Enables a single visual simulation run that produces step-by-step board snapshots for animation and inspection.',
          mandatory: true,
          csharpClass: 'ExecuteVisualData',
          csharpFile: 'FocusedObjective.Contract/Data/ExecuteVisualData.cs',
          attributes: [],
          children: [],
          notes: 'No attributes. Presence alone enables visual simulation output.',
        },
        // <monteCarlo>
        {
          tag: 'monteCarlo',
          displayName: 'monteCarlo',
          description:
            'Enables Monte Carlo simulation — runs multiple stochastic cycles to produce probabilistic forecast distributions.',
          mandatory: false,
          csharpClass: 'ExecuteMonteCarloData',
          csharpFile: 'FocusedObjective.Contract/Data/ExecuteMonteCarloData.cs',
          attributes: [
            {
              name: 'cycles',
              description: 'Number of Monte Carlo simulation cycles to execute.',
              mandatory: true,
              defaultValue: '1000',
              type: 'integer',
            },
            {
              name: 'rawResults',
              description:
                '"true" returns individual result data per cycle. "false" (default) returns only aggregate statistics.',
              mandatory: false,
              validValues: ['false', 'true'],
              defaultValue: 'false',
              type: 'boolean',
            },
          ],
          children: [],
        },
        // <sensitivity>
        {
          tag: 'sensitivity',
          displayName: 'sensitivity',
          description:
            'Enables one-factor sensitivity analysis — multiplies estimate and occurrence parameters to measure impact on forecast results.',
          mandatory: false,
          csharpClass: 'ExecuteSensitivityData',
          csharpFile: 'FocusedObjective.Contract/Data/ExecuteSensitivityData.cs',
          attributes: [
            {
              name: 'cycles',
              description: 'Number of Monte Carlo cycles per sensitivity test.',
              mandatory: false,
              defaultValue: '1000',
              type: 'integer',
            },
            {
              name: 'estimateMultiplier',
              description:
                'Factor applied to estimate bounds during sensitivity tests (e.g. 1.2 = +20%).',
              mandatory: false,
              defaultValue: '1.2',
              type: 'number',
            },
            {
              name: 'occurrenceMultiplier',
              description:
                'Factor applied to occurrence rate bounds during sensitivity tests.',
              mandatory: false,
              defaultValue: '1.2',
              type: 'number',
            },
            {
              name: 'iterationMultiplier',
              description:
                'Factor applied to iteration velocity bounds during sensitivity tests (Scrum only).',
              mandatory: false,
              defaultValue: '1.0',
              type: 'number',
            },
            {
              name: 'sensitivityType',
              description: 'Type of sensitivity analysis to run.',
              mandatory: false,
              defaultValue: '"oneFactor"',
              type: 'string',
            },
            {
              name: 'sortOrder',
              description: 'How sensitivity results are sorted in output.',
              mandatory: false,
              defaultValue: '"ascending"',
              type: 'string',
            },
          ],
          children: [],
          notes:
            'Not decorated with [SimMLElement] in C# — attributes parsed via fromXML/AsXML convention.',
        },
        // <addStaff>
        {
          tag: 'addStaff',
          displayName: 'addStaff',
          description:
            'Enables add-staff analysis — runs repeated cycles incrementally increasing WIP in each column to find the optimal staffing allocation.',
          mandatory: false,
          csharpClass: 'ExecuteAddStaffData',
          csharpFile: 'FocusedObjective.Contract/Data/ExecuteAddStaffData.cs',
          attributes: [
            {
              name: 'cycles',
              description: 'Number of Monte Carlo cycles per staffing test.',
              mandatory: false,
              defaultValue: '1000',
              type: 'integer',
            },
            {
              name: 'count',
              description: 'Number of staff increments to test per column.',
              mandatory: false,
              defaultValue: '1',
              type: 'integer',
            },
            {
              name: 'optimizeForLowest',
              description:
                'When true, finds the staffing allocation that minimizes time. When false, maximizes throughput.',
              mandatory: false,
              defaultValue: 'true',
              type: 'boolean',
            },
          ],
          children: [],
          notes:
            'Not decorated with [SimMLElement] — attributes parsed via fromXML. Specialized analysis mode.',
        },
        // <ballot>
        {
          tag: 'ballot',
          displayName: 'ballot',
          description:
            'Enables ballot/voting analysis — applies Schulze or Borda voting methods for backlog prioritization.',
          mandatory: false,
          csharpClass: 'ExecuteBallotData',
          csharpFile: 'FocusedObjective.Contract/Data/ExecuteBallotData.cs',
          attributes: [
            {
              name: 'type',
              description: 'Voting algorithm to use.',
              mandatory: false,
              validValues: ['borda', 'schulze'],
              defaultValue: '"borda"',
              type: 'enum',
            },
          ],
          children: [],
          notes:
            'Not decorated with [SimMLElement] — attributes parsed via fromXML. Specialized analysis mode.',
        },
      ],
    },

    // ── <setup> ────────────────────────────────────────────────────────
    {
      tag: 'setup',
      displayName: 'setup',
      description:
        'Contains the complete simulation configuration: backlog, columns, events, iterations, forecast dates, distributions, phases, and classes of service.',
      mandatory: true,
      csharpClass: 'SetupData',
      csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
      attributes: [],
      children: [
        // <backlog>
        {
          tag: 'backlog',
          displayName: 'backlog',
          description:
            'Defines the work items to be processed. Can be "simple" (a count of identical items) or "custom" (detailed item types grouped optionally by deliverable).',
          mandatory: true,
          csharpClass: 'SetupBacklogData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupBacklogData.cs',
          attributes: [
            {
              name: 'type',
              description:
                'Backlog specification type. "simple" creates identical items from a count. "custom" uses explicit deliverable and custom entries.',
              mandatory: false,
              validValues: ['simple', 'custom'],
              defaultValue: 'simple',
              type: 'enum',
            },
            {
              name: 'simpleCount',
              description: 'Number of backlog items when type="simple".',
              mandatory: false,
              defaultValue: '100',
              type: 'integer',
            },
            {
              name: 'nameFormat',
              description:
                'Format string for work item names. Placeholders: {0}=sequential index, {1}=custom name, {2}=custom order, {3}=deliverable name, {4}=deliverable order.',
              mandatory: false,
              defaultValue: '"Story {0}"',
              type: 'string',
            },
            {
              name: 'shuffle',
              description:
                'Whether the backlog is initially randomized for entries sharing the same order value. Set "false" to preserve declaration order.',
              mandatory: false,
              validValues: ['true', 'false'],
              defaultValue: 'true',
              type: 'boolean',
            },
          ],
          children: [
            // <deliverable>
            {
              tag: 'deliverable',
              displayName: 'deliverable',
              description:
                'Groups custom backlog items into a logical deliverable (epic, feature, project). Supports ordering, skip percentage, prerequisites, and earliest start date.',
              mandatory: false,
              csharpClass: 'SetupBacklogDeliverableData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupBacklogDeliverableData.cs',
              attributes: [
                {
                  name: 'name',
                  description:
                    'Unique name for this deliverable. Used as a reference in prerequisites and deliverable filters, and displayed via the {3} name-format placeholder.',
                  mandatory: true,
                  type: 'string',
                },
                {
                  name: 'dueDate',
                  description:
                    'Due date used for ordering work start alongside other deliverables with the same priority order. Format per <execute dateFormat=...>.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'date',
                },
                {
                  name: 'skipPercentage',
                  description:
                    'Percentage of simulation runs where this entire deliverable is omitted from the initial backlog (models risk or intangibility). Range 0–100.',
                  mandatory: false,
                  defaultValue: '0.0',
                  type: 'number',
                },
                {
                  name: 'order',
                  description:
                    'Sort order determining when this deliverable is started relative to others. Lowest value is started first. Omitting defaults to highest (started last).',
                  mandatory: false,
                  defaultValue: 'int.MaxValue',
                  type: 'integer',
                },
                {
                  name: 'preRequisiteDeliverables',
                  description:
                    'Pipe-separated list of deliverable names that must fully complete before any items in this deliverable can start.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'earliestStartDate',
                  description:
                    'Earliest simulation date at which items from this deliverable may be pulled from the backlog. Format per <execute dateFormat=...>.',
                  mandatory: false,
                  type: 'date',
                },
              ],
              children: [
                // nested <custom> inside <deliverable> — same structure, referenced below
              ],
              notes:
                'Contains one or more <custom> child elements that define the actual work items. The deliverable itself is a grouping container.',
            },
            // <custom>
            {
              tag: 'custom',
              displayName: 'custom',
              description:
                'Defines a type of work item in the backlog. Each custom entry generates one or more cards/stories with the specified estimate and attribute ranges.',
              mandatory: false,
              csharpClass: 'SetupBacklogCustomData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupBacklogCustomData.cs',
              attributes: [
                {
                  name: 'name',
                  description:
                    'Name identifying this custom backlog type. Used as a reference in event targeting and displayed via the {1} name-format placeholder.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'count',
                  description:
                    'Number of items generated from this entry and added to the backlog.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'completed',
                  description:
                    'If true, these items are considered already completed at simulation start. Used for "forecast from current state" (e.g. 18 of 50 done, forecast the rest).',
                  mandatory: false,
                  validValues: ['false', 'true'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'estimateLowBound',
                  description: 'Lowest story point estimate for these items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateHighBound',
                  description: 'Highest story point estimate for these items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateDistribution',
                  description:
                    'Named distribution for story point estimates (Scrum only). Must reference a distribution defined in <distributions>.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                  simType: 'scrum',
                },
                {
                  name: 'classOfService',
                  description:
                    'Class of service name assigned to these items. Must reference a class of service defined in <classOfServices>.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'dueDate',
                  description:
                    'Due date for priority ordering alongside items with the same sort order.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'date',
                },
                {
                  name: 'percentageLowBound',
                  description:
                    'Lowest percentage of column cycle-time range used for these items (Kanban only). 0 = column low bound, 100 = column high bound.',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'kanban',
                },
                {
                  name: 'percentageHighBound',
                  description:
                    'Highest percentage of column cycle-time range used for these items (Kanban only).',
                  mandatory: false,
                  defaultValue: '100',
                  type: 'number',
                  simType: 'kanban',
                },
                {
                  name: 'order',
                  description:
                    'Sort order determining when these items are started. Lowest value started first. Omitting defaults to highest.',
                  mandatory: false,
                  defaultValue: 'int.MaxValue',
                  type: 'integer',
                },
                {
                  name: 'valueLowBound',
                  description:
                    'Lowest random business value amount assigned to these items. Used for cost-of-delay and ROI analysis.',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'valueHighBound',
                  description: 'Highest random business value amount assigned to these items.',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'initialColumn',
                  description:
                    'Column id where these items start (Kanban only). Items skip all columns before this one. Default -1 means start in backlog.',
                  mandatory: false,
                  defaultValue: '-1',
                  type: 'integer',
                  simType: 'kanban',
                },
              ],
              children: [
                // <column> overrides
                {
                  tag: 'column',
                  displayName: 'column (item override)',
                  description:
                    'Overrides default column cycle-time estimates for items of this custom type. Allows different item types to move through columns at different speeds.',
                  mandatory: false,
                  csharpClass: 'SetupBacklogCustomColumnData',
                  csharpFile:
                    'FocusedObjective.Contract/Data/SetupBacklogCustomColumnData.cs',
                  attributes: [
                    {
                      name: 'id',
                      description:
                        'Column id to override default estimate values for (Kanban only).',
                      mandatory: true,
                      type: 'integer',
                    },
                    {
                      name: 'estimateLowBound',
                      description:
                        'Lowest cycle-time value for this column for these item types (Kanban only).',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateHighBound',
                      description:
                        'Highest cycle-time value for this column for these item types (Kanban only).',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateDistribution',
                      description:
                        'Named distribution for cycle-time estimates for this column (Kanban only).',
                      mandatory: false,
                      defaultValue: '""',
                      type: 'string',
                    },
                    {
                      name: 'skipPercentage',
                      description:
                        'How often items of this type skip this column (0–100). Default 0. (Kanban only)',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                  ],
                  children: [],
                },
              ],
              notes:
                'Can appear as a direct child of <backlog> or nested inside a <deliverable>. The same C# class handles both cases.',
            },
          ],
        },

        // <columns>
        {
          tag: 'columns',
          displayName: 'columns',
          description:
            'Container for the Kanban board columns. Each column represents a workflow step that work items progress through.',
          mandatory: true,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'column',
              displayName: 'column (board)',
              description:
                'Defines a workflow step on the Kanban board with cycle-time estimates, WIP limits, skip percentage, and buffer behavior.',
              mandatory: true,
              csharpClass: 'SetupColumnData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupColumnData.cs',
              attributes: [
                {
                  name: 'id',
                  description:
                    'Unique column id number. Referenced by defects, blocking events, phase overrides, and custom item overrides.',
                  mandatory: true,
                  type: 'integer',
                },
                {
                  name: 'displayWidth',
                  description: 'Width of this column in the visual simulation board. Default 1.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'estimateLowBound',
                  description:
                    'Lowest cycle-time value (number of intervals) for work in this column (Kanban only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'kanban',
                },
                {
                  name: 'estimateHighBound',
                  description:
                    'Highest cycle-time value (number of intervals) for work in this column (Kanban only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'kanban',
                },
                {
                  name: 'estimateDistribution',
                  description:
                    'Named distribution for cycle-time estimates in this column (Kanban only). Must reference a distribution in <distributions>.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                  simType: 'kanban',
                },
                {
                  name: 'wipLimit',
                  description:
                    'Maximum number of items allowed in this column simultaneously (Kanban only). Set to 0 for unlimited.',
                  mandatory: true,
                  defaultValue: '0',
                  type: 'integer',
                  simType: 'kanban',
                },
                {
                  name: 'skipPercentage',
                  description:
                    'Percentage of items that skip this column entirely (0–100). Default is 0.',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'kanban',
                },
                {
                  name: 'buffer',
                  description:
                    'If true, this is a buffer/queue column. Work entering is immediately flagged as complete for this column and passes through without delay.',
                  mandatory: false,
                  validValues: ['true', 'false'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'replenishInterval',
                  description:
                    'Number of simulation intervals that must pass before new work can enter this column. Default is 1 (every interval).',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'completeInterval',
                  description:
                    'Number of simulation intervals that must pass before completed work can exit this column. Default is 1.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
              ],
              children: [],
              notes:
                'The text content of the <column> element is used as the column display name.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },

        // <iteration>
        {
          tag: 'iteration',
          displayName: 'iteration',
          description:
            'Defines iteration/sprint parameters for Scrum simulations. Specifies the velocity range (story points per iteration) and allocation rules.',
          mandatory: false,
          csharpClass: 'SetupIterationData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupIterationData.cs',
          attributes: [
            {
              name: 'storyPointsPerIterationLowBound',
              description: 'Lowest story points delivered per iteration (Scrum only).',
              mandatory: false,
              defaultValue: '0',
              type: 'number',
              simType: 'scrum',
            },
            {
              name: 'storyPointsPerIterationHighBound',
              description: 'Highest story points delivered per iteration (Scrum only).',
              mandatory: false,
              defaultValue: '0',
              type: 'number',
              simType: 'scrum',
            },
            {
              name: 'storyPointsPerIterationDistribution',
              description:
                'Named distribution for iteration velocity (Scrum only). Must reference a distribution in <distributions>.',
              mandatory: false,
              defaultValue: '""',
              type: 'string',
              simType: 'scrum',
            },
            {
              name: 'itemsPerIterationLowBound',
              description:
                'Lowest number of items completed per iteration. Alternative to story points for throughput-based Scrum.',
              mandatory: false,
              defaultValue: '0',
              type: 'number',
              simType: 'scrum',
            },
            {
              name: 'itemsPerIterationHighBound',
              description:
                'Highest number of items completed per iteration. Alternative to story points.',
              mandatory: false,
              defaultValue: '0',
              type: 'number',
              simType: 'scrum',
            },
            {
              name: 'itemsPerIterationDistribution',
              description:
                'Named distribution for throughput-based iteration velocity. Must reference a distribution in <distributions>.',
              mandatory: false,
              defaultValue: '""',
              type: 'string',
              simType: 'scrum',
            },
            {
              name: 'allowedToOverAllocate',
              description:
                'Whether the sprint can allocate stories beyond the iteration target. When false, stories larger than remaining capacity are skipped.',
              mandatory: false,
              validValues: ['true', 'false'],
              defaultValue: 'true',
              type: 'boolean',
            },
          ],
          children: [],
          notes:
            'Supports two modes: story-point-based (storyPointsPerIteration*) and throughput-based (itemsPerIteration*). SetupThroughputData in C# handles the throughput variant.',
        },

        // <defects>
        {
          tag: 'defects',
          displayName: 'defects',
          description:
            'Container for defect event definitions. Defects represent additional work created because of existing work (bugs, failures, rework).',
          mandatory: false,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'defect',
              displayName: 'defect',
              description:
                'Defines a type of defect event. When triggered, creates new work items that flow through the board. Trigger rate is based on items in a specific column or overall activity.',
              mandatory: false,
              csharpClass: 'SetupDefectData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupDefectData.cs',
              attributes: [
                {
                  name: 'columnId',
                  description:
                    'Column id where items are counted toward the occurrence rate and where defect items originate (Kanban only). -1 = use backlog.',
                  mandatory: false,
                  defaultValue: '-1',
                  type: 'integer',
                  simType: 'kanban',
                },
                {
                  name: 'occurrenceType',
                  description:
                    'Measurement unit for occurrence rates. "count"/"cards"/"stories" = absolute item count trigger. "size"/"points" = story point trigger (Scrum). "percentage" = probability per interval.',
                  mandatory: true,
                  validValues: ['count', 'cards', 'stories', 'size', 'points', 'percentage'],
                  defaultValue: 'count',
                  type: 'enum',
                },
                {
                  name: 'occurrenceLowBound',
                  description:
                    'Lowest occurrence rate value in units specified by occurrenceType.',
                  mandatory: true,
                  defaultValue: '5',
                  type: 'number',
                },
                {
                  name: 'occurrenceHighBound',
                  description:
                    'Highest occurrence rate value in units specified by occurrenceType.',
                  mandatory: true,
                  defaultValue: '10',
                  type: 'number',
                },
                {
                  name: 'occurrenceDistribution',
                  description:
                    'Named distribution for occurrence rate (instead of low/high bounds). Must reference a distribution in <distributions>.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'startsInColumnId',
                  description:
                    'Column id where defect items start their journey (Kanban only). -1 = backlog (default).',
                  mandatory: false,
                  defaultValue: '-1',
                  type: 'integer',
                  simType: 'kanban',
                },
                {
                  name: 'count',
                  description:
                    'Number of defect items added each time this event triggers. Default 1.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'classOfService',
                  description:
                    'Class of service assigned to newly created defect items. Blank = same class as the triggering card.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'estimateLowBound',
                  description: 'Lowest story point estimate for defect items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateHighBound',
                  description: 'Highest story point estimate for defect items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateDistribution',
                  description:
                    'Named distribution for defect story point estimates (Scrum only).',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                  simType: 'scrum',
                },
                {
                  name: 'isCardMove',
                  description:
                    'If true, the triggering card is moved back to the defect start column rather than creating a new defect item.',
                  mandatory: false,
                  validValues: ['true', 'false'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'phases',
                  description:
                    'Pipe-separated phase names where this defect event is active. Blank = active in all phases.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'targetCustomBacklog',
                  description:
                    'Only items of this custom backlog type are counted toward the trigger. Blank = all item types.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'targetDeliverable',
                  description:
                    'Only items in this deliverable are counted toward the trigger. Blank = all deliverables.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
              ],
              children: [
                {
                  tag: 'column',
                  displayName: 'column (defect override)',
                  description:
                    'Overrides cycle-time estimates for defect items in a specific column. Allows defects to have different processing times than regular work.',
                  mandatory: false,
                  csharpClass: 'SetupDefectColumnData',
                  csharpFile: 'FocusedObjective.Contract/Data/SetupDefectColumnData.cs',
                  attributes: [
                    {
                      name: 'columnId',
                      description:
                        'Column id to apply the defect estimate override (Kanban only).',
                      mandatory: true,
                      type: 'integer',
                    },
                    {
                      name: 'estimateLowBound',
                      description: 'Lowest cycle-time for defects in this column.',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateHighBound',
                      description: 'Highest cycle-time for defects in this column.',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateDistribution',
                      description:
                        'Named distribution for defect cycle-time in this column.',
                      mandatory: false,
                      defaultValue: '""',
                      type: 'string',
                    },
                  ],
                  children: [],
                },
              ],
              notes:
                'The text content of the <defect> element is used as the defect name.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },

        // <blockingEvents>
        {
          tag: 'blockingEvents',
          displayName: 'blockingEvents',
          description:
            'Container for blocking event definitions. Blocking events represent delays and impediments that prevent items from progressing.',
          mandatory: false,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'blockingEvent',
              displayName: 'blockingEvent',
              description:
                'Defines a type of blocking event (impediment, delay, external dependency). When triggered, blocks an active item for a random duration.',
              mandatory: false,
              csharpClass: 'SetupBlockingEventData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupBlockingEventData.cs',
              attributes: [
                {
                  name: 'columnId',
                  description:
                    'Column id where items are counted toward occurrence and where blocking takes effect (Kanban only). -1 = all columns.',
                  mandatory: false,
                  defaultValue: '-1',
                  type: 'integer',
                  simType: 'kanban',
                },
                {
                  name: 'occurrenceType',
                  description:
                    'Measurement unit for occurrence rates.',
                  mandatory: true,
                  validValues: ['count', 'cards', 'stories', 'size', 'points', 'percentage'],
                  defaultValue: 'count',
                  type: 'enum',
                },
                {
                  name: 'occurrenceLowBound',
                  description: 'Lowest occurrence rate value.',
                  mandatory: true,
                  defaultValue: '5',
                  type: 'number',
                },
                {
                  name: 'occurrenceHighBound',
                  description: 'Highest occurrence rate value.',
                  mandatory: true,
                  defaultValue: '10',
                  type: 'number',
                },
                {
                  name: 'occurrenceDistribution',
                  description: 'Named distribution for occurrence rate.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'estimateLowBound',
                  description:
                    'Lowest blocking duration estimate (intervals for Kanban, points for Scrum).',
                  mandatory: true,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'estimateHighBound',
                  description: 'Highest blocking duration estimate.',
                  mandatory: true,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'estimateDistribution',
                  description: 'Named distribution for blocking duration.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'phases',
                  description:
                    'Pipe-separated phase names where this event is active. Blank = all phases.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'blockWork',
                  description: 'Whether regular work items can be blocked by this event.',
                  mandatory: false,
                  validValues: ['true', 'false'],
                  defaultValue: 'true',
                  type: 'boolean',
                },
                {
                  name: 'blockDefects',
                  description: 'Whether defect items can be blocked by this event.',
                  mandatory: false,
                  validValues: ['true', 'false'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'blockAddedScope',
                  description: 'Whether added-scope items can be blocked by this event.',
                  mandatory: false,
                  validValues: ['true', 'false'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'targetCustomBacklog',
                  description:
                    'Only items of this custom backlog type can be blocked. Blank = all types.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'targetDeliverable',
                  description:
                    'Only items in this deliverable can be blocked. Blank = all deliverables.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
              ],
              children: [],
              notes:
                'The text content of the <blockingEvent> element is used as the event name.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },

        // <addedScopes>
        {
          tag: 'addedScopes',
          displayName: 'addedScopes',
          description:
            'Container for added scope event definitions. Added scope represents new work discovered and added to the backlog after the project has started.',
          mandatory: false,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'addedScope',
              displayName: 'addedScope',
              description:
                'Defines a type of added scope event. When triggered, new items are added to the backlog (scope creep, discovered requirements).',
              mandatory: false,
              csharpClass: 'SetupAddedScopeData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupAddedScopeData.cs',
              attributes: [
                {
                  name: 'occurrenceType',
                  description: 'Measurement unit for occurrence rates.',
                  mandatory: true,
                  validValues: ['count', 'cards', 'stories', 'size', 'points', 'percentage'],
                  defaultValue: 'count',
                  type: 'enum',
                },
                {
                  name: 'occurrenceLowBound',
                  description: 'Lowest occurrence rate value.',
                  mandatory: true,
                  defaultValue: '5',
                  type: 'number',
                },
                {
                  name: 'occurrenceHighBound',
                  description: 'Highest occurrence rate value.',
                  mandatory: true,
                  defaultValue: '10',
                  type: 'number',
                },
                {
                  name: 'occurrenceDistribution',
                  description: 'Named distribution for occurrence rate.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'estimateLowBound',
                  description:
                    'Lowest story point estimate for added scope items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateHighBound',
                  description: 'Highest story point estimate for added scope items (Scrum only).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'estimateDistribution',
                  description:
                    'Named distribution for added scope story point estimates (Scrum only).',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                  simType: 'scrum',
                },
                {
                  name: 'count',
                  description:
                    'Number of items added to the backlog each time this event triggers. Default 1.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'phases',
                  description:
                    'Pipe-separated phase names where this event is active. Blank = all phases.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'classOfService',
                  description:
                    'Class of service for newly created items. Blank = same as triggering card.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'customBacklog',
                  description:
                    'Custom backlog type for newly created items. Blank = same as triggering card.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'deliverable',
                  description:
                    'Deliverable for newly created items. Blank = same as triggering card.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'targetCustomBacklog',
                  description:
                    'Only items of this custom backlog type count as triggers. Blank = all.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
                {
                  name: 'targetDeliverable',
                  description:
                    'Only items in this deliverable count as triggers. Blank = all.',
                  mandatory: false,
                  defaultValue: '""',
                  type: 'string',
                },
              ],
              children: [],
              notes:
                'The text content of the <addedScope> element is used as the event name.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },

        // <forecastDate>
        {
          tag: 'forecastDate',
          displayName: 'forecastDate',
          description:
            'Configures calendar date mapping, cost tracking, excluded dates, target dates for cost-of-delay analysis, and actuals overlay.',
          mandatory: false,
          csharpClass: 'ForecastDateData',
          csharpFile: 'FocusedObjective.Contract/Data/ForecastDateData.cs',
          attributes: [
            {
              name: 'startDate',
              description:
                'First calendar day the project starts. Format per <execute dateFormat=...>.',
              mandatory: true,
              type: 'date',
            },
            {
              name: 'intervalsToOneDay',
              description:
                'Number of simulation steps per calendar day. Use 8 for hourly intervals in an 8-hour workday.',
              mandatory: false,
              defaultValue: '1',
              type: 'integer',
            },
            {
              name: 'workDays',
              description:
                'Comma-separated working days of the week. Non-work days are skipped in date calculations.',
              mandatory: false,
              defaultValue: '"monday,tuesday,wednesday,thursday,friday"',
              type: 'string',
            },
            {
              name: 'costPerDay',
              description:
                'Monetary amount per working day used for total cost computation.',
              mandatory: false,
              defaultValue: '0',
              type: 'currency',
            },
            {
              name: 'workDaysPerIteration',
              description:
                'Number of work days per iteration (Scrum only). Used to map iterations to calendar dates.',
              mandatory: false,
              defaultValue: '10',
              type: 'integer',
              simType: 'scrum',
            },
            {
              name: 'targetDate',
              description:
                'Target completion date for cost-of-delay analysis. Format per <execute dateFormat=...>.',
              mandatory: false,
              type: 'date',
            },
            {
              name: 'targetLikelihood',
              description:
                'Desired probability (0–100) for the forecast date to meet the target.',
              mandatory: false,
              defaultValue: '85',
              type: 'number',
            },
            {
              name: 'revenue',
              description:
                'Expected revenue per revenueUnit once the target date is reached. Used for cost-of-delay calculations.',
              mandatory: false,
              defaultValue: '0',
              type: 'currency',
            },
            {
              name: 'revenueUnit',
              description: 'Time period for the revenue attribute.',
              mandatory: false,
              validValues: ['day', 'week', 'month', 'year'],
              defaultValue: 'month',
              type: 'enum',
            },
          ],
          children: [
            // <excludes>
            {
              tag: 'excludes',
              displayName: 'excludes',
              description:
                'Container for date exclusions (holidays, shutdowns, etc.).',
              mandatory: false,
              csharpClass: 'ForecastDateData',
              csharpFile: 'FocusedObjective.Contract/Data/ForecastDateData.cs',
              attributes: [],
              children: [
                {
                  tag: 'exclude',
                  displayName: 'exclude',
                  description:
                    'Excludes a specific calendar date from the forecast date calculation (holiday, shutdown).',
                  mandatory: false,
                  csharpClass: 'ForecastDateExcludeData',
                  csharpFile: 'FocusedObjective.Contract/Data/ForecastDateExcludeData.cs',
                  attributes: [
                    {
                      name: 'date',
                      description:
                        'Calendar date to exclude. Format per <execute dateFormat=...>.',
                      mandatory: true,
                      type: 'date',
                    },
                  ],
                  children: [],
                },
              ],
              notes:
                'Also supports the older <excludeDate date="..."/> syntax as a direct child of <forecastDate>.',
            },
            // <actuals>
            {
              tag: 'actuals',
              displayName: 'actuals',
              description:
                'Container for actual progress measurements. Overlays real data on forecast charts.',
              mandatory: false,
              csharpClass: 'ForecastDateData',
              csharpFile: 'FocusedObjective.Contract/Data/ForecastDateData.cs',
              attributes: [],
              children: [
                {
                  tag: 'actual',
                  displayName: 'actual',
                  description:
                    'Records an actual measurement at a specific date for overlay on forecast charts.',
                  mandatory: false,
                  csharpClass: 'ForecastDateActualData',
                  csharpFile: 'FocusedObjective.Contract/Data/ForecastDateActualData.cs',
                  attributes: [
                    {
                      name: 'date',
                      description:
                        'Calendar date of this actual measurement. Format per <execute dateFormat=...>.',
                      mandatory: true,
                      type: 'date',
                    },
                    {
                      name: 'count',
                      description:
                        'Cumulative number of completed items as of the specified date.',
                      mandatory: true,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'annotation',
                      description: 'Text annotation displayed on charts at this data point.',
                      mandatory: false,
                      defaultValue: '""',
                      type: 'string',
                    },
                  ],
                  children: [],
                },
              ],
            },
          ],
        },

        // <distributions>
        {
          tag: 'distributions',
          displayName: 'distributions',
          description:
            'Container for named probability distributions that can be referenced by estimate, occurrence, and velocity attributes throughout the model.',
          mandatory: false,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'distribution',
              displayName: 'distribution',
              description:
                'Defines a named probability distribution. Can be a parametric shape (uniform, weibull, normal, etc.) or empirical sample data.',
              mandatory: false,
              csharpClass: 'SetupDistributionData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupDistributionData.cs',
              attributes: [
                {
                  name: 'name',
                  description:
                    'Unique name for this distribution. Referenced by *Distribution attributes elsewhere.',
                  mandatory: true,
                  type: 'string',
                },
                {
                  name: 'shape',
                  description:
                    'Distribution shape. 25+ built-in types including: uniform, weibull, normal, lognormal, triangular, beta, gamma, exponential, poisson, binomial, SIP (sample data), histogram, and more.',
                  mandatory: true,
                  type: 'string',
                },
                {
                  name: 'parameters',
                  description:
                    'Comma-separated distribution parameters. Meaning depends on shape (e.g. alpha,beta for Weibull).',
                  mandatory: false,
                  type: 'string',
                },
                {
                  name: 'numberType',
                  description: 'Type of number returned by the distribution.',
                  mandatory: false,
                  validValues: ['double', 'integer'],
                  type: 'enum',
                },
                {
                  name: 'generator',
                  description: 'Underlying uniform random number generator algorithm.',
                  mandatory: false,
                  validValues: ['alf', 'mt19937', 'xordshift128'],
                  defaultValue: 'alf',
                  type: 'enum',
                },
                {
                  name: 'count',
                  description:
                    'Count of random numbers to pre-generate. Default 1000.',
                  mandatory: false,
                  defaultValue: '1000',
                  type: 'integer',
                },
                {
                  name: 'lowBound',
                  description: 'Lowest allowed value. Values below are clipped or stretched.',
                  mandatory: false,
                  type: 'number',
                },
                {
                  name: 'highBound',
                  description: 'Highest allowed value. Values above are clipped or stretched.',
                  mandatory: false,
                  type: 'number',
                },
                {
                  name: 'boundProcessing',
                  description:
                    'How out-of-bound values are handled. "clip" truncates at bounds, "stretch" rescales.',
                  mandatory: false,
                  validValues: ['clip', 'stretch'],
                  defaultValue: 'clip',
                  type: 'enum',
                },
                {
                  name: 'location',
                  description: 'Starting point (offset) on the X-axis for the distribution.',
                  mandatory: false,
                  type: 'number',
                },
                {
                  name: 'multiplier',
                  description: 'Multiplier applied to all generated values. Default 1.0.',
                  mandatory: false,
                  defaultValue: '1.0',
                  type: 'number',
                },
                {
                  name: 'separatorCharacter',
                  description: 'Entry separator character for sample-data distributions.',
                  mandatory: false,
                  type: 'string',
                },
                {
                  name: 'zeroHandling',
                  description: 'How zero values in sample data are processed.',
                  mandatory: false,
                  validValues: ['keep', 'remove', 'value'],
                  defaultValue: 'keep',
                  type: 'enum',
                },
                {
                  name: 'zeroValue',
                  description:
                    'Replacement value when zeroHandling="value".',
                  mandatory: false,
                  type: 'number',
                },
                {
                  name: 'path',
                  description: 'File path for external distribution data.',
                  mandatory: false,
                  type: 'string',
                },
                {
                  name: 'decimalSeparator',
                  description: 'Decimal separator character in sample data.',
                  mandatory: false,
                  type: 'string',
                },
                {
                  name: 'thousandsSeparator',
                  description: 'Thousands separator character in sample data.',
                  mandatory: false,
                  type: 'string',
                },
              ],
              children: [],
              notes:
                'The text content can hold inline sample data values (SIP distributions). The distribution engine supports parametric, empirical, and composite distribution types.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },

        // <phases>
        {
          tag: 'phases',
          displayName: 'phases',
          description:
            'Defines time-based phases that modify simulation behavior (WIP limits, estimate/occurrence multipliers, cost overrides) as the project progresses.',
          mandatory: false,
          csharpClass: 'SetupPhasesData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupPhasesData.cs',
          attributes: [
            {
              name: 'unit',
              description:
                'Unit system for phase start/end values. "percentage" = % of backlog items pulled, "interval" = simulation step number (Kanban), "iteration" = iteration number (Scrum).',
              mandatory: false,
              validValues: ['percentage', 'interval', 'iteration'],
              defaultValue: 'percentage',
              type: 'enum',
            },
          ],
          children: [
            {
              tag: 'phase',
              displayName: 'phase',
              description:
                'Defines a single phase with activation range and parameter overrides. When active, modifies simulation behavior for all items.',
              mandatory: false,
              csharpClass: 'SetupPhaseData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupPhaseData.cs',
              attributes: [
                {
                  name: 'start',
                  description:
                    'Lowest trigger value to activate this phase (in units defined by parent <phases unit=...>).',
                  mandatory: true,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'end',
                  description:
                    'Highest trigger value — phase deactivates above this value.',
                  mandatory: true,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'estimateMultiplier',
                  description:
                    'Multiplier applied to all cycle-time estimates while this phase is active. >1 = slower, <1 = faster.',
                  mandatory: false,
                  defaultValue: '1.0',
                  type: 'number',
                },
                {
                  name: 'occurrenceMultiplier',
                  description:
                    'Multiplier applied to defect and blocking event occurrence rates while active.',
                  mandatory: false,
                  defaultValue: '1.0',
                  type: 'number',
                },
                {
                  name: 'iterationMultiplier',
                  description:
                    'Multiplier applied to iteration velocity (Scrum only). <1 = reduced velocity.',
                  mandatory: false,
                  defaultValue: '1.0',
                  type: 'number',
                  simType: 'scrum',
                },
                {
                  name: 'costPerDay',
                  description:
                    'Cost per day override while this phase is active. Overrides the global forecastDate costPerDay.',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'currency',
                },
              ],
              children: [
                {
                  tag: 'column',
                  displayName: 'column (phase override)',
                  description:
                    'Overrides the WIP limit for a specific column while this phase is active.',
                  mandatory: false,
                  csharpClass: 'SetupPhaseColumnData',
                  csharpFile: 'FocusedObjective.Contract/Data/SetupPhaseColumnData.cs',
                  attributes: [
                    {
                      name: 'id',
                      description: 'Column id to override WIP limit for (Kanban only).',
                      mandatory: true,
                      type: 'integer',
                    },
                    {
                      name: 'wipLimit',
                      description: 'WIP limit override when this phase is active.',
                      mandatory: true,
                      defaultValue: '0',
                      type: 'integer',
                    },
                  ],
                  children: [],
                },
              ],
              notes:
                'The text content is used as the phase name. Legacy attributes startPercentage/endPercentage are also supported as aliases for start/end.',
            },
          ],
        },

        // <classOfServices>
        {
          tag: 'classOfServices',
          displayName: 'classOfServices',
          description:
            'Container for class of service definitions. Classes of service control work priority, WIP violation rules, skip probability, and per-column estimate overrides.',
          mandatory: false,
          csharpClass: 'SetupData',
          csharpFile: 'FocusedObjective.Contract/Data/SetupData.cs',
          attributes: [],
          children: [
            {
              tag: 'classOfService',
              displayName: 'classOfService',
              description:
                'Defines a class of service with priority ordering, WIP violation behavior, and optional per-column estimate overrides.',
              mandatory: false,
              csharpClass: 'SetupClassOfServiceData',
              csharpFile: 'FocusedObjective.Contract/Data/SetupClassOfServiceData.cs',
              attributes: [
                {
                  name: 'order',
                  description:
                    'Work priority order. Lowest order = highest priority (pulled first). Used in the multi-key sort: deliverable order → backlog order → COS order → due date → sort order.',
                  mandatory: false,
                  defaultValue: '1',
                  type: 'integer',
                },
                {
                  name: 'default',
                  description:
                    'Whether this is the default class of service applied to items without an explicit assignment.',
                  mandatory: false,
                  validValues: ['false', 'true'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'violateWIP',
                  description:
                    'When true, items of this class can enter a column even when it is at WIP limit. The lowest-priority active item in the column is blocked to make room.',
                  mandatory: false,
                  validValues: ['false', 'true'],
                  defaultValue: 'false',
                  type: 'boolean',
                },
                {
                  name: 'skipPercentage',
                  description:
                    'Percentage of items in this class that are automatically completed without entering the board (0–100).',
                  mandatory: false,
                  defaultValue: '0',
                  type: 'number',
                },
                {
                  name: 'maximumAllowedOnBoard',
                  description:
                    'Maximum number of active items of this class allowed on the board simultaneously.',
                  mandatory: false,
                  defaultValue: 'int.MaxValue',
                  type: 'integer',
                },
              ],
              children: [
                {
                  tag: 'column',
                  displayName: 'column (COS override)',
                  description:
                    'Overrides column cycle-time estimates for items of this class of service. Takes highest precedence over item-level and column-level defaults.',
                  mandatory: false,
                  csharpClass: 'SetupBacklogCustomColumnData',
                  csharpFile:
                    'FocusedObjective.Contract/Data/SetupBacklogCustomColumnData.cs',
                  attributes: [
                    {
                      name: 'id',
                      description: 'Column id to override (Kanban only).',
                      mandatory: true,
                      type: 'integer',
                    },
                    {
                      name: 'estimateLowBound',
                      description: 'Lowest cycle-time for this COS in this column.',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateHighBound',
                      description: 'Highest cycle-time for this COS in this column.',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                    {
                      name: 'estimateDistribution',
                      description:
                        'Named distribution for COS cycle-time in this column.',
                      mandatory: false,
                      defaultValue: '""',
                      type: 'string',
                    },
                    {
                      name: 'skipPercentage',
                      description:
                        'How often items of this COS skip this column (0–100).',
                      mandatory: false,
                      defaultValue: '0',
                      type: 'number',
                    },
                  ],
                  children: [],
                },
              ],
              notes:
                'The text content is used as the class of service name. The same <column> override class (SetupBacklogCustomColumnData) is shared with <custom> item overrides.',
            },
          ],
          notes: 'Container element with no attributes of its own.',
        },
      ],
      notes: 'Container element with no attributes of its own.',
    },
  ],
}

// ─── Utility: flatten tree ─────────────────────────────────────────────────

export function flattenSchema(
  node: SimMLSchemaElement,
  path: string[] = [],
): Array<{ element: SimMLSchemaElement; path: string[] }> {
  const currentPath = [...path, node.tag]
  const result: Array<{ element: SimMLSchemaElement; path: string[] }> = [
    { element: node, path: currentPath },
  ]
  for (const child of node.children) {
    result.push(...flattenSchema(child, currentPath))
  }
  return result
}

/** Count total elements and attributes in the schema */
export function schemaStats(node: SimMLSchemaElement): {
  elements: number
  attributes: number
} {
  let elements = 1
  let attributes = node.attributes.length
  for (const child of node.children) {
    const childStats = schemaStats(child)
    elements += childStats.elements
    attributes += childStats.attributes
  }
  return { elements, attributes }
}
