# Simulation Engine Parity Plan

Browser TypeScript implementation vs C# desktop engine (`KanbanAndScrumSim/`).

**Status legend**: ⬜ Not started · 🔄 In progress · ✅ Done

> Distributions are excluded — will be handled separately.

---

## HIGH — Affects simulation correctness

### 1. Pull Order / FIFO ⬜

**What**: C# supports 5 processing modes via `pullOrder` on `<execute>`: `randomAfterOrdering`, `random`, `indexSequence`, `FIFO`, `FIFOStrict`. Controls whether lower-index cards complete first and whether strict ordering prevents out-of-order completion. TS always processes positions sequentially (implicit `indexSequence`).

**C# files**: `PullOrderEnum.cs`, `KanbanSimulation.cs` (`positionOrderToProcessList`, `isStrictFIFOAllowsComplete`), `ExecuteData.cs`

**Plan**:
1. Parse `pullOrder` attribute from `<execute>` element
2. Implement `positionOrderToProcessList()` to shuffle/sort column positions per mode
3. Add `isStrictFIFOAllowsComplete()` guard for `FIFOStrict` — block completion if a lower-index card in the same column isn't done
4. Wire into `runKanbanSimulation` column processing loop
5. Tests: verify each mode produces expected ordering behaviour

---

### 2. Prerequisite Deliverables ⬜

**What**: Deliverables can declare pipe-separated `preRequisiteDeliverables`; `nextAllowedBacklogCard()` skips cards whose prerequisite deliverables haven't all completed yet.

**C# files**: `SetupBacklogDeliverableData.cs`, `KanbanSimulation.cs` (`nextAllowedBacklogCard`)

**Plan**:
1. Parse `preRequisiteDeliverables` attribute on `<deliverable>` elements (pipe-delimited string → string array)
2. During backlog pull, check if all prerequisite deliverable names have had every card complete
3. Skip blocked deliverable cards and try the next eligible card
4. Tests: two deliverables where B depends on A; verify B cards don't start until A finishes

---

### 3. Earliest Start Date ⬜

**What**: Per-deliverable `earliestStartDate` prevents cards from leaving backlog until that simulation date is reached.

**C# files**: `SetupBacklogDeliverableData.cs`, `KanbanSimulation.cs` (`nextAllowedBacklogCard`)

**Plan**:
1. Parse `earliestStartDate` attribute on `<deliverable>` elements
2. In backlog pull logic, compare current simulation date against earliest start date
3. Skip cards whose deliverable hasn't reached its start date
4. Tests: deliverable with future start date; verify cards stay in backlog until that date

---

### 4. Completed Flag on Custom Backlog ⬜

**What**: `completed="true"` on `<custom>` elements marks items as already done. `buildBacklog()` adds them directly to the completed list and decrements remaining work. Needed for "forecast from current state" (e.g., "18 of 50 done, forecast the rest").

**C# files**: `SetupBacklogCustomData.cs`, `KanbanSimulation.cs` (`buildBacklog`), `ScrumSimulation.cs`

**Plan**:
1. Parse `completed` boolean attribute on `<custom>` elements
2. In `createKanbanItems` / `createScrumItems`, add completed items directly to the done list
3. Ensure Monte Carlo and visual sim correctly report them as pre-completed
4. Tests: 10 items with 4 completed; verify sim only processes 6 and reports 10 total done

---

### 5. Due Date Priority Ordering ⬜

**What**: `dueDate` on deliverables and custom items enters the multi-key priority sort: deliverable order → backlog order → COS order → **dueDate** → sortOrder.

**C# files**: `SetupBacklogDeliverableData.cs`, `SetupBacklogCustomData.cs`, `Card.cs` (`CardPriorityComparer`), `Story.cs` (`StoryPriorityComparer`)

**Plan**:
1. Parse `dueDate` attribute on `<deliverable>` and `<custom>` elements
2. Add `dueDate` field to `KanbanItem` and `ScrumItem`
3. Update item sorting to use the full C# priority chain: deliverable order → backlog order → COS order → dueDate → sortOrder
4. Tests: items with different due dates; verify pull order matches expected priority

---

### 6. Blocking Event Targeting ⬜

**What**: C# blocking events can target specific deliverables (`targetDeliverable`), specific custom items (`targetCustomBacklog`), specific card types (`blockWork`, `blockDefects`, `blockAddedScope`), and be scoped to specific `phases`.

**C# files**: `SetupBlockingEventData.cs`, `BlockingEventProcessor.cs` (`pickNextCandidate`)

**Plan**:
1. Parse targeting attributes: `targetDeliverable`, `targetCustomBacklog`, `blockWork`, `blockDefects`, `blockAddedScope`, `phases`
2. Add targeting fields to `SimBlockingEvent` interface
3. In blocking event processing, filter eligible cards by target criteria before selecting victim
4. Check phase scope when applying blocking events
5. Tests: blocking event targeting only defects in a specific deliverable; verify work cards are unaffected

---

### 7. Execute Deliverables Filter ⬜

**What**: `deliverables` attribute on `<execute>` lets a run include only a subset of deliverables from the backlog definition, so one SimML can power multiple forecasts.

**C# files**: `ExecuteData.cs` (`_deliverables`), `KanbanSimulation.cs` (`buildBacklog`)

**Plan**:
1. Parse `deliverables` attribute from `<execute>` element (pipe-delimited string)
2. Store as `string[]` on `SimExecute`
3. In backlog building, filter items to only those belonging to listed deliverables (or all if empty)
4. Tests: SimML with 3 deliverables, execute filtering to 2; verify only 2 deliverables' items are simulated

---

### 8. Complete Percentage Early Exit ⬜

**What**: `completePercentage` (default 100) and `activePositionsCompletePercentage` (default 100) allow the sim to stop early when a threshold of items are done and board utilisation drops.

**C# files**: `ExecuteData.cs`, `KanbanSimulation.cs` (`RunSimulation` loop)

**Plan**:
1. Parse `completePercentage` and `activePositionsCompletePercentage` from `<execute>`
2. In the main simulation loop, check both thresholds each step
3. Exit early when both conditions are satisfied
4. Tests: 100 items with `completePercentage="80"`; verify sim stops around 80 completed

---

## MEDIUM — Affects reporting or advanced use cases

### 9. Value Tracking ⬜

**What**: Custom backlog items carry `valueLowBound` / `valueHighBound`. `ValueAndDateProcessor` tallies cumulative value delivered each interval for cost-of-delay and ROI analysis.

**C# files**: `SetupBacklogCustomData.cs`, `ValueAndDateProcessor.cs`

**Plan**:
1. Parse `valueLowBound` / `valueHighBound` on `<custom>` elements
2. Assign random value to each item at creation
3. Track cumulative value delivered per step in visual sim results
4. Include value data in Monte Carlo result summaries

---

### 10. Cumulative Flow Diagram Data ⬜

**What**: `GetCumulativeFlowData()` emits per-interval card counts by column, value delivered, cost, and dates for CFD charting.

**C# files**: `KanbanSimulation.cs` (`GetCumulativeFlowData`), `ResultsVisual.cs`

**Plan**:
1. Each step, record count of items per column (backlog, each work column, done)
2. Include value and cost if tracking is enabled (#9)
3. Return CFD data as part of visual sim result
4. Add CFD chart to the UI (separate task)

---

### 11. Aggregation Value Selector ⬜

**What**: MC result summarisation supports `Average`, `Median`, `Fifth`, `NinetyFifth`, `Max`, `Min`. Controls which percentile is used for final forecast numbers.

**C# files**: `ExecuteData.cs` (`_aggregationValue`), `MonteCarloResultSummary.cs`

**Plan**:
1. Parse `aggregationValue` from `<monteCarlo>` element
2. Implement percentile selection functions
3. Use selected aggregation when computing MC summary statistics

---

### 12. Name Format Placeholders ⬜

**What**: Backlog `nameFormat` supports `{0}` (sequence), `{1}` (deliverable), `{2}` (COS), `{3}` (column), `{4}` (remaining) for readable card labels.

**C# files**: `SetupBacklogData.cs` (`_nameFormat`), `KanbanSimulation.cs` (`buildBacklog`)

**Plan**:
1. Parse `nameFormat` from `<backlog>` element
2. Apply format string when generating item labels in `createKanbanItems` / `createScrumItems`

---

### 13. Target Date / Revenue / Cost of Delay ⬜

**What**: `forecastDate` supports `targetDate`, `revenue`, `revenueUnit`, `targetLikelihood` feeding cost-of-delay calculations.

**C# files**: `ForecastDateData.cs`, `ValueAndDateProcessor.cs`

**Plan**:
1. Parse additional `forecastDate` attributes
2. Calculate cost-of-delay when target date and revenue are provided
3. Include in MC summary output

---

### 14. Actuals Tracking ⬜

**What**: `<actual>` child elements of `<forecastDate>` with date, count, and annotation let users overlay real progress on forecast charts.

**C# files**: `ForecastDateActualData.cs`, `ForecastDateData.cs`

**Plan**:
1. Parse `<actual>` elements within `<forecastDate>`
2. Store as array on `SimForecastDate`
3. Pass through to UI for chart overlay rendering

---

### 15. Rich Per-Run Statistics ⬜

**What**: Per-run stats: EmptyPositions, QueuedPositions, BlockedPositions, ActivePositions, InActivePositions, PullTransactions, per-card-type cycle times, per-COS cycle times, per-column active positions.

**C# files**: `SimulationResultSummary.cs`

**Plan**:
1. Instrument the simulation loop to collect position-state counts each step
2. Compute cycle time breakdowns by card type and COS
3. Return as structured statistics alongside existing results

---

### 16. Blocking Event Occurrence Type ⬜

**What**: `occurrenceType` can be `count` (per-interval count), `percentage` (percentage chance), or `points` (story points). Changes how occurrence bounds are interpreted.

**C# files**: `SetupBlockingEventData.cs` (`_occurrenceType`), `OccurrenceTypeEnum`

**Plan**:
1. Parse `occurrenceType` attribute on `<blockingEvent>`
2. Branch occurrence sampling logic based on type
3. Tests: percentage-based blocking event triggers probabilistically

---

### 17. Blocking Event Scale ⬜

**What**: `scale` attribute applies a multiplier to the blocking event estimate range, distinct from occurrence bounds.

**C# files**: `SetupBlockingEventData.cs` (`_scale`)

**Plan**:
1. Parse `scale` attribute on `<blockingEvent>`
2. Multiply sampled estimate by scale factor when computing blocked time
3. Tests: scale=2 doubles blocking duration

---

### 18. Percentage-Based Estimates (wire up) ⬜

**What**: `percentageLowBound` / `percentageHighBound` on custom items scale column estimates by a per-item percentage. Already parsed in TS but not wired into `sampleColumnDuration`.

**C# files**: `SetupBacklogCustomData.cs`, `Card.cs` (`getLowBoundForColumn`, `getHighBoundForColumn`)

**Plan**:
1. In `sampleColumnDuration`, read item's percentage bounds
2. Sample a percentage multiplier and apply it to the column estimate range
3. Tests: item with percentageLowBound=50, percentageHighBound=50 takes half the time

---

## LOW — Specialised features

### 19. Add Staff Simulation ⬜

**What**: Dedicated simulation mode: runs repeated cycles adding 1 staff member at a time to each column, measuring which column benefits most. Outputs optimised staffing recommendations.

**C# files**: `ExecuteAddStaffData.cs`

**Plan**: Parse `<addStaff>` element. Run N cycles per column, incrementing WIP by 1 each cycle. Report improvement per column.

---

### 20. Ballot / Voting System ⬜

**What**: Schulze and Borda voting methods for backlog prioritisation. Entirely separate simulation mode.

**C# files**: `ExecuteBallotData.cs`, `VotingSystems/`

**Plan**: Implement as separate module. Parse `<ballot>` element. Implement Schulze and Borda algorithms. Return ranked ordering.

---

### 21. Throughput-Based Scrum ⬜

**What**: `SetupThroughputData` replaces story-point estimation with `itemsPerIterationLowBound/HighBound`, counting raw items per iteration.

**C# files**: `SetupThroughputData.cs`

**Plan**:
1. Parse `<throughput>` element as alternative to `<iteration>`
2. In Scrum sim, count items per iteration instead of story points
3. Tests: throughput-based forecast with 5 items/iteration

---

### 22. Card Flow Data Export ⬜

**What**: Per-interval, per-card position data for animation or detailed trace analysis beyond board snapshots.

**C# files**: `ResultsVisual.cs` (`cardFlow`)

**Plan**: Record each card's column at every step. Return as time-series data alongside visual results.

---

### 23. Initial Column for Custom Items ⬜

**What**: Custom items can specify `initialColumn` to start mid-board (already partially parsed in TS `SimWorkItemTemplate.initialColumn` but not used in simulation logic).

**C# files**: `SetupBacklogCustomData.cs`, `KanbanSimulation.cs` (`buildBacklog`)

**Plan**:
1. In `createKanbanItems`, place items with `initialColumn` directly into the specified column
2. Pre-calculate completed work for columns before the initial column
3. Tests: item starting in column 2 of 3; verify it skips column 1

---

### 24. Decimal Rounding Config ⬜

**What**: `decimalRounding` attribute on `<execute>` controls output precision for all statistics.

**C# files**: `ExecuteData.cs` (`_decimalRounding`)

**Plan**: Parse attribute. Apply `toFixed(n)` to all numeric outputs in results.

---

## Implementation Priority

Recommended order for maximum impact:

| Phase | Items | Rationale |
|-------|-------|-----------|
| **A** | #1, #4, #8, #18 | Correctness basics — pull order, completed flag, early exit, percentage estimates |
| **B** | #2, #3, #5, #7 | Deliverable features — prerequisites, earliest start, due dates, execute filter |
| **C** | #6, #16, #17 | Blocking event fidelity — targeting, occurrence types, scale |
| **D** | #9, #10, #13, #14 | Value & reporting — value tracking, CFD, cost-of-delay, actuals |
| **E** | #11, #12, #15 | Output polish — aggregation, name format, rich stats |
| **F** | #19–24 | Specialised — add staff, voting, throughput scrum, card flow, initial column, rounding |
