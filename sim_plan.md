# Simulation Engine Parity Plan

Browser TypeScript implementation vs C# desktop engine (`KanbanAndScrumSim/`).

**Status legend**: ⬜ Not started · 🔄 In progress · ✅ Done

> Distributions are excluded — will be handled separately.

---

## HIGH — Affects simulation correctness

### 1. Pull Order / FIFO ✅

**What**: C# supports 5 processing modes via `pullOrder` on `<execute>`: `randomAfterOrdering`, `random`, `indexSequence`, `FIFO`, `FIFOStrict`. Controls whether lower-index cards complete first and whether strict ordering prevents out-of-order completion. TS always processes positions sequentially (implicit `indexSequence`).

**C# files**: `PullOrderEnum.cs`, `KanbanSimulation.cs` (`positionOrderToProcessList`, `isStrictFIFOAllowsComplete`), `ExecuteData.cs`

**Implemented**:
1. Parsed `pullOrder` from `<execute>`
2. Added `parsePullOrder()` with legacy alias support for `index` and `afterOrdering`
3. Implemented `orderCardsForProcessing()` and `isStrictFifoAllowsComplete()`
4. Wired ordering into Kanban completion processing and pull-order sequence tracking
5. Added tests for `FIFOStrict`, default handling, and legacy aliases

---

### 2. Prerequisite Deliverables ✅

**What**: Deliverables can declare pipe-separated `preRequisiteDeliverables`; `nextAllowedBacklogCard()` skips cards whose prerequisite deliverables haven't all completed yet.

**C# files**: `SetupBacklogDeliverableData.cs`, `KanbanSimulation.cs` (`nextAllowedBacklogCard`)

**Implemented**:
1. Parsed `preRequisiteDeliverables` on `<deliverable>` into string arrays
2. Propagated prerequisites into created Kanban items
3. Added backlog eligibility checks in `nextAllowedBacklogCard()`
4. Tightened behavior so missing prerequisite deliverables do not silently pass
5. Added tests for satisfied and missing prerequisite scenarios

---

### 3. Earliest Start Date ✅

**What**: Per-deliverable `earliestStartDate` prevents cards from leaving backlog until that simulation date is reached.

**C# files**: `SetupBacklogDeliverableData.cs`, `KanbanSimulation.cs` (`nextAllowedBacklogCard`)

**Implemented**:
1. Parsed `earliestStartDate` on `<deliverable>`
2. Propagated the value into created Kanban items
3. Added simulation-date gating in `nextAllowedBacklogCard()`
4. Aligned gating date math with forecast-date handling, including `excludeDate`
5. Added tests for future start dates and excluded-date handling

---

### 4. Completed Flag on Custom Backlog ✅

**What**: `completed="true"` on `<custom>` elements marks items as already done. `buildBacklog()` adds them directly to the completed list and decrements remaining work. Needed for "forecast from current state" (e.g., "18 of 50 done, forecast the rest").

**C# files**: `SetupBacklogCustomData.cs`, `KanbanSimulation.cs` (`buildBacklog`), `ScrumSimulation.cs`

**Implemented**:
1. Parsed `completed` on `<custom>` backlog items
2. Seeded completed custom items into the initial done state for both Kanban and Scrum
3. Ensured visual simulations start with correct `doneCount` / remaining backlog counts
4. Added tests covering parser behavior plus Kanban and Scrum forecast-from-current-state cases

---

### 5. Due Date Priority Ordering ✅

**What**: `dueDate` on deliverables and custom items enters the multi-key priority sort: deliverable order → backlog order → COS order → **dueDate** → sortOrder.

**C# files**: `SetupBacklogDeliverableData.cs`, `SetupBacklogCustomData.cs`, `Card.cs` (`CardPriorityComparer`), `Story.cs` (`StoryPriorityComparer`)

**Implemented**:
1. Parsed `dueDate` on both `<deliverable>` and `<custom>`, with deliverable dates inherited by nested custom items when not overridden
2. Added `deliverableOrder` and `dueDate` to work-item templates and runtime Kanban/Scrum items
3. Updated priority sorting to use deliverable order → backlog order → COS order → due date, while preserving random/index tie-breaking behavior
4. Added tests covering parsing, Kanban deliverable-order precedence, and Scrum due-date tie-breaking

---

### 6. Blocking Event Targeting ✅

**What**: C# blocking events can target specific deliverables (`targetDeliverable`), specific custom items (`targetCustomBacklog`), specific card types (`blockWork`, `blockDefects`, `blockAddedScope`), and be scoped to specific `phases`.

**C# files**: `SetupBlockingEventData.cs`, `BlockingEventProcessor.cs` (`pickNextCandidate`)

**Implemented**:
1. Parsed `targetDeliverable`, `targetCustomBacklog`, and `phases` onto `SimBlockingEvent` alongside existing card-type flags
2. Added phase and target filtering to Kanban and Scrum blocking-event application logic
3. Scoped blocking candidates by deliverable name, custom backlog name, card type, and active phase
4. Added tests covering parser behavior, Kanban deliverable targeting, and Scrum phase-targeted blocking

---

### 7. Execute Deliverables Filter ✅

**What**: `deliverables` attribute on `<execute>` lets a run include only a subset of deliverables from the backlog definition, so one SimML can power multiple forecasts.

**C# files**: `ExecuteData.cs` (`_deliverables`), `KanbanSimulation.cs` (`buildBacklog`)

**Implemented**:
1. Parsed `deliverables` from `<execute>` into `SimExecute.deliverables`
2. Filtered backlog templates before Kanban and Scrum item creation
3. Matched desktop behavior: when deliverables are specified, only named deliverables are included and standalone custom backlog items are excluded
4. Added tests covering parser behavior plus Kanban and Scrum filtering

---

### 8. Complete Percentage Early Exit ✅

**What**: `completePercentage` (default 100) and `activePositionsCompletePercentage` (default 100) allow the sim to stop early when a threshold of items are done and board utilisation drops.

**C# files**: `ExecuteData.cs`, `KanbanSimulation.cs` (`RunSimulation` loop)

**Implemented**:
1. Parsed `completePercentage` and `activePositionsCompletePercentage` from `<execute>`
2. Added Kanban loop checks for completion percentage and active board-position percentage after each interval
3. Exit now occurs when both thresholds are satisfied, matching desktop early-stop semantics
4. Added a test verifying tail-end early exit with partial board utilisation

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


### 21. Throughput-Based Scrum ⬜

**What**: `SetupThroughputData` replaces story-point estimation with `itemsPerIterationLowBound/HighBound`, counting raw items per iteration.

**C# files**: `SetupThroughputData.cs`

**Plan**:
1. Parse `<throughput>` element as alternative to `<iteration>`
2. In Scrum sim, count items per iteration instead of story points
3. Tests: throughput-based forecast with 5 items/iteration

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
