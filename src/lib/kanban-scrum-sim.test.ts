// @vitest-environment jsdom

import { describe, expect, it } from 'vitest'
import {
  parseSimMl,
  runMonteCarlo,
  runSensitivityAnalysis,
  runVisualSimulation,
} from './kanban-scrum-sim'

describe('kanban-scrum sim parser and runtime', () => {
  it('parses and simulates a simple kanban board', () => {
    const model = parseSimMl(`
      <simulation name="Simple" locale="en-US">
        <execute dateFormat="dd-MMM-yyyy">
          <visual />
          <monteCarlo cycles="50" />
          <sensitivity cycles="30" estimateMultiplier="1.2" />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="8" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Design</column>
            <column id="2" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Build</column>
          </columns>
          <forecastDate startDate="01-May-2012" costPerDay="1000" />
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const monteCarlo = runMonteCarlo(model, 25)
    const sensitivity = runSensitivityAnalysis(model)

    expect(model.execute.simulationType).toBe('kanban')
    expect(visual.totalSteps).toBeGreaterThan(0)
    expect(visual.completedItems).toBe(8)
    expect(monteCarlo.percentileSteps).toHaveLength(4)
    expect(monteCarlo.medianSteps).toBeGreaterThan(0)
    expect(monteCarlo.standardDeviation).toBeGreaterThanOrEqual(0)
    expect(monteCarlo.minSteps).toBeLessThanOrEqual(monteCarlo.maxSteps)
    expect(sensitivity.tests.length).toBe(2)
  })

  it('supports basic scrum iteration forecasting', () => {
    const model = parseSimMl(`
      <simulation name="Scrum Example" locale="en-US">
        <execute type="scrum">
          <visual />
          <monteCarlo cycles="20" />
          <sensitivity cycles="10" iterationMultiplier="1.1" estimateMultiplier="1.2" />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <custom name="Feature" count="4" estimateLowBound="5" estimateHighBound="5" />
          </backlog>
          <forecastDate startDate="01Jan2012" workDaysPerIteration="10" costPerDay="500" />
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)

    expect(model.execute.simulationType).toBe('scrum')
    expect(visual.totalSteps).toBe(2)
    expect(visual.completedItems).toBe(4)
    expect(visual.completionDate).toBeTruthy()
  })
})

describe('kanban: buffer columns', () => {
  it('passes items through buffer columns without delay', () => {
    const model = parseSimMl(`
      <simulation name="Buffer Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="1" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
            <column id="2" buffer="true" wipLimit="5">Buffer</column>
            <column id="3" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Final</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(1)
    // Buffer column shouldn't add to step count beyond work columns
    expect(visual.totalSteps).toBeGreaterThan(0)
  })
})

describe('backlog: completed custom items', () => {
  it('parses completed custom backlog items', () => {
    const model = parseSimMl(`
      <simulation name="Completed Parse">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="custom">
            <custom name="Done item" count="2" completed="true" />
            <custom name="Open item" count="1" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.setup.backlog.items[0].completed).toBe(true)
    expect(model.setup.backlog.items[1].completed).toBe(false)
  })

  it('starts kanban simulations with completed custom items already done', () => {
    const model = parseSimMl(`
      <simulation name="Completed Kanban" locale="en-US">
        <execute limitIntervalsTo="20">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <custom name="Done item" count="4" completed="true" estimateLowBound="1" estimateHighBound="1" />
            <custom name="Open item" count="6" estimateLowBound="1" estimateHighBound="1" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const start = visual.snapshots[0]

    expect(start?.doneCount).toBe(4)
    expect(start?.backlogCount).toBe(6)
    expect(visual.completedItems).toBe(10)
    expect(visual.totalSteps).toBe(7)
  })

  it('starts scrum simulations with completed custom items already done', () => {
    const model = parseSimMl(`
      <simulation name="Completed Scrum" locale="en-US">
        <execute type="scrum" limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom" shuffle="false">
            <custom name="Done story" count="2" completed="true" estimateLowBound="5" estimateHighBound="5" />
            <custom name="Open story" count="2" estimateLowBound="5" estimateHighBound="5" />
          </backlog>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const start = visual.snapshots[0]

    expect(start?.doneCount).toBe(2)
    expect(start?.backlogCount).toBe(2)
    expect(visual.completedItems).toBe(4)
    expect(visual.totalSteps).toBe(1)
  })
})

describe('backlog: due date priority', () => {
  it('parses deliverable and custom due dates', () => {
    const model = parseSimMl(`
      <simulation name="Due Date Parse">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="A" dueDate="10-Jan-2020" order="2">
              <custom name="Inherited due" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <custom name="Explicit due" count="1" dueDate="05-Jan-2020" estimateLowBound="1" estimateHighBound="1" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.setup.backlog.items[0].dueDate).toBe('10-Jan-2020')
    expect(model.setup.backlog.items[0].deliverableOrder).toBe(2)
    expect(model.setup.backlog.items[1].dueDate).toBe('05-Jan-2020')
  })

  it('uses deliverable order before due date for kanban pull priority', () => {
    const model = parseSimMl(`
      <simulation name="Kanban Deliverable Order Priority" locale="en-US">
        <execute limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="Later due sooner priority" order="1" dueDate="20-Jan-2020">
              <custom name="Priority first" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="Earlier due lower priority" order="2" dueDate="05-Jan-2020">
              <custom name="Due date second" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.snapshots.find((s) => s.step === 1)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Priority first 1'])
    expect(visual.snapshots.find((s) => s.step === 2)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Due date second 1'])
  })

  it('uses earlier due dates to break equal scrum priorities', () => {
    const model = parseSimMl(`
      <simulation name="Scrum Due Date Priority" locale="en-US">
        <execute type="scrum" limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="5" storyPointsPerIterationHighBound="5" />
          <backlog type="custom" shuffle="false">
            <custom name="Late due" count="1" order="1" dueDate="20-Jan-2020" estimateLowBound="5" estimateHighBound="5" />
            <custom name="Early due" count="1" order="1" dueDate="05-Jan-2020" estimateLowBound="5" estimateHighBound="5" />
          </backlog>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.snapshots[0]?.columns[0]?.cards.map((c) => c.label)).toEqual(['Early due 1 (5)', 'Late due 1 (5)'])
    expect(visual.snapshots.find((s) => s.step === 1)?.doneCount).toBe(1)
    expect(visual.snapshots.find((s) => s.step === 1)?.backlogCount).toBe(1)
    expect(visual.snapshots.find((s) => s.step === 1)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Late due 1 (5)'])
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
  })
})

describe('execute: deliverables filtering', () => {
  it('parses execute deliverables filter', () => {
    const model = parseSimMl(`
      <simulation name="Deliverables Parse">
        <execute deliverables="A|B">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="A">
              <custom name="A item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="B">
              <custom name="B item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.execute.deliverables).toEqual(['A', 'B'])
  })

  it('filters kanban backlog items to selected deliverables only', () => {
    const model = parseSimMl(`
      <simulation name="Deliverables Kanban" locale="en-US">
        <execute deliverables="A|C" limitIntervalsTo="20">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <custom name="Standalone" count="1" estimateLowBound="1" estimateHighBound="1" />
            <deliverable name="A">
              <custom name="A item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="B">
              <custom name="B item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="C">
              <custom name="C item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(2)
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
    expect(visual.snapshots[0]?.backlogCount).toBe(2)
    expect(visual.snapshots.find((s) => s.step === 1)?.columns[0]?.cards.map((c) => c.label)).toEqual(['A item 1'])
    expect(visual.snapshots.find((s) => s.step === 2)?.columns[0]?.cards.map((c) => c.label)).toEqual(['C item 1'])
  })

  it('filters scrum backlog items to selected deliverables only', () => {
    const model = parseSimMl(`
      <simulation name="Deliverables Scrum" locale="en-US">
        <execute type="scrum" deliverables="B" limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="5" storyPointsPerIterationHighBound="5" />
          <backlog type="custom" shuffle="false">
            <custom name="Standalone" count="1" estimateLowBound="5" estimateHighBound="5" />
            <deliverable name="A">
              <custom name="A item" count="1" estimateLowBound="5" estimateHighBound="5" />
            </deliverable>
            <deliverable name="B">
              <custom name="B item" count="1" estimateLowBound="5" estimateHighBound="5" />
            </deliverable>
          </backlog>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(1)
    expect(visual.snapshots[0]?.backlogCount).toBe(1)
    expect(visual.snapshots[0]?.columns[0]?.cards.map((c) => c.label)).toEqual(['B item 1 (5)'])
    expect(visual.snapshots.at(-1)?.doneCount).toBe(1)
  })
})

describe('kanban: complete percentage early exit', () => {
  it('stops once completion and active-position thresholds are both satisfied', () => {
    const model = parseSimMl(`
      <simulation name="Kanban Early Exit">
        <execute
          limitIntervalsTo="20"
          completePercentage="80"
          activePositionsCompletePercentage="50"
        >
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="10" shuffle="false" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="4">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)

    expect(model.execute.completePercentage).toBe(80)
    expect(model.execute.activePositionsCompletePercentage).toBe(50)
    expect(visual.totalSteps).toBe(3)
    expect(visual.completedItems).toBe(8)
    expect(visual.snapshots.at(-1)?.doneCount).toBe(8)
    expect(visual.snapshots.at(-1)?.columns[0]?.cards).toHaveLength(2)
  })
})

describe('parsing: defects, blocking events, added scopes', () => {
  it('parses defects from SimML', () => {
    const model = parseSimMl(`
      <simulation name="Defect Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="5" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <defects>
            <defect name="Bug" columnId="1" occurrenceLowBound="2" occurrenceHighBound="3"
                    estimateLowBound="1" estimateHighBound="2" count="1" />
          </defects>
        </setup>
      </simulation>
    `)

    expect(model.setup.defects).toHaveLength(1)
    expect(model.setup.defects[0].name).toBe('Bug')
    expect(model.setup.defects[0].count).toBe(1)
    expect(model.setup.defects[0].startsInColumnId).toBeUndefined()
    // Defects should no longer produce a warning
    expect(model.warnings.some((w) => w.includes('Defect'))).toBe(false)
  })

  it('parses blocking events from SimML', () => {
    const model = parseSimMl(`
      <simulation name="Blocking Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <blockingEvents>
            <blockingEvent name="Review" columnId="1" occurrenceLowBound="2" occurrenceHighBound="4"
                           estimateLowBound="1" estimateHighBound="3" />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    expect(model.setup.blockingEvents).toHaveLength(1)
    expect(model.setup.blockingEvents[0].name).toBe('Review')
    expect(model.setup.blockingEvents[0].blockWork).toBe(true)
    expect(model.setup.blockingEvents[0].blockDefects).toBe(true)
    expect(model.setup.blockingEvents[0].blockAddedScope).toBe(true)
    expect(model.warnings.some((w) => w.includes('Blocking'))).toBe(false)
  })

  it('parses blocking event targeting attributes and phases', () => {
    const model = parseSimMl(`
      <simulation name="Blocking Target Parse">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="1" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
          <blockingEvents>
            <blockingEvent
              name="Targeted"
              columnId="1"
              occurrenceLowBound="1"
              occurrenceHighBound="1"
              estimateLowBound="1"
              estimateHighBound="1"
              targetDeliverable="A"
              targetCustomBacklog="Feature"
              phases="Alpha|Beta"
            />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    expect(model.setup.blockingEvents[0].targetDeliverable).toBe('A')
    expect(model.setup.blockingEvents[0].targetCustomBacklog).toBe('Feature')
    expect(model.setup.blockingEvents[0].phases).toEqual(['Alpha', 'Beta'])
  })

  it('parses added scope events from SimML', () => {
    const model = parseSimMl(`
      <simulation name="AddedScope Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <addedScopes>
            <addedScope name="Extra" occurrenceLowBound="2" occurrenceHighBound="3"
                        estimateLowBound="1" estimateHighBound="2" count="1" />
          </addedScopes>
        </setup>
      </simulation>
    `)

    expect(model.setup.addedScopes).toHaveLength(1)
    expect(model.setup.addedScopes[0].name).toBe('Extra')
    expect(model.warnings.some((w) => w.includes('Added-scope'))).toBe(false)
  })
})

describe('parsing: backlog shuffle', () => {
  it('parses the shuffle attribute on backlog', () => {
    const model = parseSimMl(`
      <simulation name="Shuffle Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="5" shuffle="true" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.setup.backlog.shuffle).toBe(true)
  })

  it('defaults shuffle to true when not specified', () => {
    const model = parseSimMl(`
      <simulation name="Default Shuffle">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.setup.backlog.shuffle).toBe(true)
  })
})

describe('parsing: sensitivity occurrence multiplier', () => {
  it('parses occurrenceMultiplier from sensitivity element', () => {
    const model = parseSimMl(`
      <simulation name="Occ Mult Test">
        <execute>
          <visual />
          <sensitivity cycles="10" estimateMultiplier="1.3" occurrenceMultiplier="1.5" />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.execute.sensitivityOccurrenceMultiplier).toBe(1.5)
  })

  it('defaults occurrenceMultiplier to 1.2', () => {
    const model = parseSimMl(`
      <simulation name="Default Occ Mult">
        <execute>
          <visual />
          <sensitivity cycles="10" />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.execute.sensitivityOccurrenceMultiplier).toBe(1.2)
  })
})

describe('parsing: deliverable skip percentage', () => {
  it('skips entire deliverable when skipPercentage triggers', () => {
    // Use 100% skip to guarantee the skip
    const model = parseSimMl(`
      <simulation name="Skip Test">
        <execute type="scrum">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <deliverable name="Required" skipPercentage="0">
              <custom name="Must Do" count="1" estimateLowBound="5" estimateHighBound="5" />
            </deliverable>
            <deliverable name="Skippable" skipPercentage="100">
              <custom name="Might Skip" count="3" estimateLowBound="5" estimateHighBound="5" />
            </deliverable>
          </backlog>
        </setup>
      </simulation>
    `)

    // Only the Required deliverable should have items
    expect(model.setup.backlog.items.length).toBe(1)
    expect(model.setup.backlog.items[0].deliverable).toBe('Required')
  })
})

describe('parsing: excluded dates', () => {
  it('parses excludeDate elements from forecastDate', () => {
    const model = parseSimMl(`
      <simulation name="Excluded Dates Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <forecastDate startDate="01-May-2012">
            <excludeDate date="07-May-2012" />
            <excludeDate date="14-May-2012" />
          </forecastDate>
        </setup>
      </simulation>
    `)

    expect(model.setup.forecastDate?.excludedDates).toHaveLength(2)
  })
})

describe('scrum: carry-over logic', () => {
  it('carries over stories that cannot complete in one iteration', () => {
    const model = parseSimMl(`
      <simulation name="Carry-over Test">
        <execute type="scrum" limitIntervalsTo="100">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="5" storyPointsPerIterationHighBound="5" />
          <backlog type="custom">
            <custom name="Big Story" count="1" estimateLowBound="12" estimateHighBound="12" />
          </backlog>
        </setup>
      </simulation>
    `)

    const result = runVisualSimulation(model)

    // 12 points / 5 per iteration = 3 iterations needed (5+5+2)
    expect(result.completedItems).toBe(1)
    expect(result.totalSteps).toBe(3)
  })

  it('handles multiple stories with carry-over', () => {
    const model = parseSimMl(`
      <simulation name="Multi Carry-over Test">
        <execute type="scrum" limitIntervalsTo="100">
          <visual />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" allowedToOverAllocate="true" />
          <backlog type="custom">
            <custom name="Story" count="3" estimateLowBound="8" estimateHighBound="8" />
          </backlog>
        </setup>
      </simulation>
    `)

    const result = runVisualSimulation(model)
    expect(result.completedItems).toBe(3)
    // 3 stories x 8 pts = 24 pts, 10 per iteration
    // Iteration 1: start story1 (8pts), start story2 (2pts done). story1 completes. 
    // The exact steps depend on carry-over handling, but should be <= 4
    expect(result.totalSteps).toBeLessThanOrEqual(4)
    expect(result.totalSteps).toBeGreaterThanOrEqual(2)
  })
})

describe('kanban: defect event processing', () => {
  it('defects flowing through simulation increase total work items', () => {
    const model = parseSimMl(`
      <simulation name="Defect Sim">
        <execute limitIntervalsTo="200">
          <visual />
          <monteCarlo cycles="10" />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="5" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <defects>
            <defect name="Bug" columnId="1" occurrenceLowBound="1" occurrenceHighBound="2"
                    estimateLowBound="1" estimateHighBound="1" count="1" />
          </defects>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // Defects should cause more items to be completed than the original 5
    expect(visual.completedItems).toBeGreaterThanOrEqual(5)
  })

  it('emits typed board cards for stories, defects, and added scope', () => {
    const model = parseSimMl(`
      <simulation name="Typed Cards">
        <execute limitIntervalsTo="100">
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="2" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Build</column>
            <column id="2" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Test</column>
          </columns>
          <defects>
            <defect columnId="2" startsInColumnId="2" occurrenceLowBound="1" occurrenceHighBound="1" count="1">
              Bug found ({0})
            </defect>
          </defects>
          <addedScopes>
            <addedScope occurrenceLowBound="1" occurrenceHighBound="1" count="1">
              Scope split ({0})
            </addedScope>
          </addedScopes>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const cards = visual.snapshots.flatMap((snapshot) =>
      snapshot.columns.flatMap((column) => column.cards),
    )

    expect(cards.some((card) => card.kind === 'story')).toBe(true)
    expect(cards.some((card) => card.kind === 'defect')).toBe(true)
    expect(cards.some((card) => card.kind === 'addedScope')).toBe(true)
  })
})

describe('kanban: blocking event processing', () => {
  it('blocking events do not prevent eventual completion', () => {
    const model = parseSimMl(`
      <simulation name="Block Sim">
        <execute limitIntervalsTo="500">
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="3" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <blockingEvents>
            <blockingEvent name="Review" columnId="1" occurrenceLowBound="1" occurrenceHighBound="2"
                           estimateLowBound="1" estimateHighBound="2" />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(3)
    // Blocking should make it take longer than without blocking
    expect(visual.totalSteps).toBeGreaterThan(0)
  })

  it('targets blocking events to a specific deliverable', () => {
    const model = parseSimMl(`
      <simulation name="Block Target Deliverable">
        <execute limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="A" order="1">
              <custom name="A item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="B" order="2">
              <custom name="B item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
          <blockingEvents>
            <blockingEvent
              name="Review"
              columnId="1"
              occurrenceLowBound="1"
              occurrenceHighBound="1"
              estimateLowBound="1"
              estimateHighBound="1"
              targetDeliverable="A"
            />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const blocked = visual.snapshots.find((s) => s.step === 2)?.columns[0]?.cards[0]
    const bBlocked = visual.snapshots.some((snapshot) =>
      snapshot.columns.some((column) =>
        column.cards.some((card) => card.label === 'B item 1' && card.blockerLabel === 'Review'),
      ),
    )

    expect(blocked?.label).toBe('A item 1')
    expect(blocked?.isBlocked).toBe(true)
    expect(blocked?.blockerLabel).toBe('Review')
    expect(bBlocked).toBe(false)
  })
})

describe('sensitivity: event-based tests', () => {
  it('includes defect and blocking event sensitivity for kanban', () => {
    const model = parseSimMl(`
      <simulation name="Events Sensitivity">
        <execute limitIntervalsTo="200">
          <visual />
          <sensitivity cycles="10" estimateMultiplier="1.2" occurrenceMultiplier="1.3" />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="5" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="2" wipLimit="3">Work</column>
          </columns>
          <defects>
            <defect name="Bug" columnId="1" occurrenceLowBound="2" occurrenceHighBound="3"
                    estimateLowBound="1" estimateHighBound="2" count="1" />
          </defects>
          <blockingEvents>
            <blockingEvent name="Review" columnId="1" occurrenceLowBound="2" occurrenceHighBound="3"
                           estimateLowBound="1" estimateHighBound="2" />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    const sensitivity = runSensitivityAnalysis(model)
    const testNames = sensitivity.tests.map((t) => t.name)
    // Should have column test + defect test + blocking occurrence + blocking estimate
    expect(testNames).toContain('Work')
    expect(testNames).toContain('Bug')
    expect(testNames.some((n) => n.includes('Review'))).toBe(true)
    expect(sensitivity.tests.length).toBeGreaterThanOrEqual(4)
  })

  it('includes defect/blocking sensitivity for scrum', () => {
    const model = parseSimMl(`
      <simulation name="Scrum Events Sensitivity">
        <execute type="scrum" limitIntervalsTo="100">
          <visual />
          <sensitivity cycles="10" estimateMultiplier="1.2" occurrenceMultiplier="1.3" iterationMultiplier="1.1" />
        </execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <custom name="Feature" count="3" estimateLowBound="5" estimateHighBound="5" />
          </backlog>
          <defects>
            <defect name="Bug" columnId="1" occurrenceLowBound="2" occurrenceHighBound="3"
                    estimateLowBound="1" estimateHighBound="2" count="1" />
          </defects>
        </setup>
      </simulation>
    `)

    const sensitivity = runSensitivityAnalysis(model)
    const testTypes = sensitivity.tests.map((t) => t.type)
    expect(testTypes).toContain('Velocity')
    expect(testTypes).toContain('Backlog type')
    expect(testTypes).toContain('Defect')
  })
})

describe('parsing: phases', () => {
  it('parses percentage-based phases with multipliers', () => {
    const model = parseSimMl(`
      <simulation name="Phases Test">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <phases unit="percentage">
            <phase start="0" end="50" estimateMultiplier="2" occurrenceMultiplier="1.5">Early</phase>
            <phase start="51" end="100" estimateMultiplier="0.5" costPerDay="500">Late</phase>
          </phases>
        </setup>
      </simulation>
    `)

    expect(model.setup.phases).toBeDefined()
    expect(model.setup.phases!.unit).toBe('percentage')
    expect(model.setup.phases!.phases).toHaveLength(2)

    const early = model.setup.phases!.phases[0]
    expect(early.name).toBe('Early')
    expect(early.start).toBe(0)
    expect(early.end).toBe(50)
    expect(early.estimateMultiplier).toBe(2)
    expect(early.occurrenceMultiplier).toBe(1.5)
    expect(early.iterationMultiplier).toBe(1) // default

    const late = model.setup.phases!.phases[1]
    expect(late.name).toBe('Late')
    expect(late.costPerDay).toBe(500)
    expect(late.estimateMultiplier).toBe(0.5)
  })

  it('parses interval-based phases with column WIP overrides', () => {
    const model = parseSimMl(`
      <simulation name="Interval Phases">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <phases unit="interval">
            <phase start="1" end="10">
              <column id="1" wipLimit="5" />
              Ramp Up
            </phase>
          </phases>
        </setup>
      </simulation>
    `)

    expect(model.setup.phases!.unit).toBe('interval')
    const phase = model.setup.phases!.phases[0]
    expect(phase.columns).toHaveLength(1)
    expect(phase.columns[0].columnId).toBe(1)
    expect(phase.columns[0].wipLimit).toBe(5)
  })

  it('parses legacy startPercentage/endPercentage attributes', () => {
    const model = parseSimMl(`
      <simulation name="Legacy Phases">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <phases>
            <phase startPercentage="0" endPercentage="100">Full</phase>
          </phases>
        </setup>
      </simulation>
    `)

    const phase = model.setup.phases!.phases[0]
    expect(phase.start).toBe(0)
    expect(phase.end).toBe(100)
  })

  it('returns no phases when element is absent', () => {
    const model = parseSimMl(`
      <simulation name="No Phases">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="2" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
        </setup>
      </simulation>
    `)
    expect(model.setup.phases).toBeUndefined()
  })
})

describe('parsing: classOfServices', () => {
  it('parses class of service with all attributes', () => {
    const model = parseSimMl(`
      <simulation name="COS Test">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="true">Standard</classOfService>
            <classOfService order="0" violateWIP="true" maximumAllowedOnBoard="2" skipPercentage="10">Expedite</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    expect(model.setup.classOfServices).toHaveLength(2)

    const standard = model.setup.classOfServices.find((c) => c.name === 'Standard')!
    expect(standard.order).toBe(1)
    expect(standard.isDefault).toBe(true)
    expect(standard.violateWip).toBe(false)
    expect(standard.skipPercentage).toBe(0)

    const expedite = model.setup.classOfServices.find((c) => c.name === 'Expedite')!
    expect(expedite.order).toBe(0)
    expect(expedite.violateWip).toBe(true)
    expect(expedite.maximumAllowedOnBoard).toBe(2)
    expect(expedite.skipPercentage).toBe(10)
  })

  it('parses COS column overrides', () => {
    const model = parseSimMl(`
      <simulation name="COS Overrides">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
            <column id="2" estimateLowBound="3" estimateHighBound="5" wipLimit="2">Test</column>
          </columns>
          <classOfServices>
            <classOfService order="0" violateWIP="true">
              <column id="2" estimateLowBound="1" estimateHighBound="1" />
              Expedite
            </classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const cos = model.setup.classOfServices[0]
    expect(cos.name).toContain('Expedite')
    expect(cos.columnOverrides).toHaveLength(1)
    expect(cos.columnOverrides[0].columnId).toBe(2)
    expect(cos.columnOverrides[0].estimateLowBound).toBe(1)
    expect(cos.columnOverrides[0].estimateHighBound).toBe(1)
  })

  it('returns empty array when no COS defined', () => {
    const model = parseSimMl(`
      <simulation name="No COS">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="2" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
        </setup>
      </simulation>
    `)
    expect(model.setup.classOfServices).toEqual([])
  })
})

describe('parsing: backlog classOfService assignment', () => {
  it('assigns classOfService from custom backlog items', () => {
    const model = parseSimMl(`
      <simulation name="Backlog COS">
        <execute><visual /></execute>
        <setup>
          <backlog type="custom">
            <custom name="Feature" count="2" estimateLowBound="1" estimateHighBound="1" classOfService="Expedite" />
            <custom name="Task" count="1" estimateLowBound="1" estimateHighBound="1" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="true">Standard</classOfService>
            <classOfService order="0" violateWIP="true">Expedite</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const feature = model.setup.backlog.items.find((i) => i.name === 'Feature')!
    expect(feature.classOfService).toBe('Expedite')
    const task = model.setup.backlog.items.find((i) => i.name === 'Task')!
    expect(task.classOfService).toBeUndefined()
  })
})

describe('kanban: phases affect simulation', () => {
  it('phase estimate multiplier makes work take longer', () => {
    const baseXml = `
      <simulation name="Phase Est">
        <execute limitIntervalsTo="200"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="6" />
          <columns>
            <column id="1" estimateLowBound="2" estimateHighBound="2" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>`

    const phaseXml = `
      <simulation name="Phase Est">
        <execute limitIntervalsTo="500"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="6" />
          <columns>
            <column id="1" estimateLowBound="2" estimateHighBound="2" wipLimit="3">Work</column>
          </columns>
          <phases unit="percentage">
            <phase start="0" end="100" estimateMultiplier="3">Slow</phase>
          </phases>
        </setup>
      </simulation>`

    const baseResult = runVisualSimulation(parseSimMl(baseXml))
    const phaseResult = runVisualSimulation(parseSimMl(phaseXml))

    // With 3x estimate multiplier, the phase version should take more steps
    expect(phaseResult.totalSteps).toBeGreaterThan(baseResult.totalSteps)
  })

  it('snapshots carry activePhase name for percentage-based phases', () => {
    const model = parseSimMl(`
      <simulation name="Phase Snapshot">
        <execute limitIntervalsTo="500"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="10" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <phases>
            <phase startPercentage="0" endPercentage="50">Ramp-up</phase>
            <phase startPercentage="51" endPercentage="100">Full Speed</phase>
          </phases>
        </setup>
      </simulation>
    `)

    const result = runVisualSimulation(model)
    // The very first snapshot (step 0) should have activePhase "Ramp-up" (0% pulled)
    expect(result.snapshots[0].activePhase).toBe('Ramp-up')
    // At some later point we should see "Full Speed"
    const fullSpeedSnapshot = result.snapshots.find((s) => s.activePhase === 'Full Speed')
    expect(fullSpeedSnapshot).toBeDefined()
  })

  it('phase WIP limit override is respected', () => {
    // Phase overrides column WIP from 2 to 1, so items move through one at a time
    const model = parseSimMl(`
      <simulation name="Phase WIP">
        <execute limitIntervalsTo="200"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="4" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
          <phases unit="percentage">
            <phase start="0" end="100">
              <column id="1" wipLimit="1" />
              Restricted
            </phase>
          </phases>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // Should complete all items
    expect(visual.completedItems).toBe(4)
    // With WIP=1, the simulation should take at least 4 steps (one item per step in Work)
    expect(visual.totalSteps).toBeGreaterThanOrEqual(4)
    // Snapshot columns should reflect the phase-overridden WIP of 1, not the base WIP of 2
    const midSnapshot = visual.snapshots[1]
    expect(midSnapshot.columns[0].wipLimit).toBe(1)
  })
})

describe('scrum: phases affect simulation', () => {
  it('phase iteration multiplier reduces velocity', () => {
    const baseXml = `
      <simulation name="Scrum Phase Base">
        <execute type="scrum" limitIntervalsTo="50"><visual /></execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <custom name="Story" count="5" estimateLowBound="10" estimateHighBound="10" />
          </backlog>
        </setup>
      </simulation>`

    const phaseXml = `
      <simulation name="Scrum Phase Slow">
        <execute type="scrum" limitIntervalsTo="100"><visual /></execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <custom name="Story" count="5" estimateLowBound="10" estimateHighBound="10" />
          </backlog>
          <phases unit="iteration">
            <phase start="1" end="100" iterationMultiplier="0.5">Half Speed</phase>
          </phases>
        </setup>
      </simulation>`

    const baseResult = runVisualSimulation(parseSimMl(baseXml))
    const phaseResult = runVisualSimulation(parseSimMl(phaseXml))

    // With 0.5x iteration multiplier, should take more iterations
    expect(phaseResult.totalSteps).toBeGreaterThan(baseResult.totalSteps)
  })
})

describe('scrum: blocking event targeting', () => {
  it('only applies blocking events during targeted phases', () => {
    const model = parseSimMl(`
      <simulation name="Scrum Blocking Phase Target">
        <execute type="scrum" limitIntervalsTo="10"><visual /></execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="5" storyPointsPerIterationHighBound="5" />
          <backlog type="custom" shuffle="false">
            <custom name="Story" count="1" estimateLowBound="5" estimateHighBound="5" />
          </backlog>
          <phases unit="iteration">
            <phase start="1" end="1">Alpha</phase>
            <phase start="2" end="10">Beta</phase>
          </phases>
          <blockingEvents>
            <blockingEvent
              name="Late blocker"
              occurrenceLowBound="1"
              occurrenceHighBound="1"
              estimateLowBound="1"
              estimateHighBound="1"
              phases="Beta"
            />
          </blockingEvents>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.totalSteps).toBe(1)
    expect(visual.snapshots.find((s) => s.step === 1)?.doneCount).toBe(1)
  })
})

describe('kanban: classOfService behaviour', () => {
  it('COS skipPercentage auto-completes items', () => {
    // 100% skip means all items skip the board entirely
    const model = parseSimMl(`
      <simulation name="COS Skip">
        <execute limitIntervalsTo="10"><visual /></execute>
        <setup>
          <backlog type="custom">
            <custom name="Feature" count="5" estimateLowBound="10" estimateHighBound="10" classOfService="Skippable" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="10" estimateHighBound="10" wipLimit="1">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="false" skipPercentage="100">Skippable</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // All items should be completed (skipped) very quickly
    expect(visual.completedItems).toBe(5)
    // Should complete in 0 or 1 steps since all are skipped
    expect(visual.totalSteps).toBeLessThanOrEqual(1)
  })

  it('COS violateWip allows bypassing WIP limits', () => {
    // Expedite items should be able to enter a full column
    const model = parseSimMl(`
      <simulation name="COS ViolateWip">
        <execute limitIntervalsTo="200"><visual /></execute>
        <setup>
          <backlog type="custom">
            <custom name="Normal" count="2" estimateLowBound="5" estimateHighBound="5" classOfService="Standard" />
            <custom name="Urgent" count="1" estimateLowBound="1" estimateHighBound="1" classOfService="Expedite" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="5" estimateHighBound="5" wipLimit="1">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="true">Standard</classOfService>
            <classOfService order="0" violateWIP="true">Expedite</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // All items should eventually complete
    expect(visual.completedItems).toBe(3)
  })

  it('COS maximumAllowedOnBoard limits active items', () => {
    const model = parseSimMl(`
      <simulation name="COS MaxOnBoard">
        <execute limitIntervalsTo="200"><visual /></execute>
        <setup>
          <backlog type="custom">
            <custom name="Ltd" count="5" estimateLowBound="1" estimateHighBound="1" classOfService="Limited" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="5">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="false" maximumAllowedOnBoard="1">Limited</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // All items should complete eventually
    expect(visual.completedItems).toBe(5)
    // With maxOnBoard=1, it should take more steps than unrestricted (at least 5)
    expect(visual.totalSteps).toBeGreaterThanOrEqual(5)
  })

  it('COS column overrides take precedence over column defaults', () => {
    // COS override: estimate 1-1 on column 1 (normally 10-10)
    const model = parseSimMl(`
      <simulation name="COS Column Override">
        <execute limitIntervalsTo="50"><visual /></execute>
        <setup>
          <backlog type="custom">
            <custom name="Fast" count="3" estimateLowBound="1" estimateHighBound="1" classOfService="Quick" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="10" estimateHighBound="10" wipLimit="3">Work</column>
          </columns>
          <classOfServices>
            <classOfService order="1" default="false">
              <column id="1" estimateLowBound="1" estimateHighBound="1" />
              Quick
            </classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    // With COS override, work takes 1 step per item instead of 10
    expect(visual.completedItems).toBe(3)
    // Should complete much faster than 10 steps per item
    expect(visual.totalSteps).toBeLessThanOrEqual(5)
  })
})

describe('scrum: classOfService behaviour', () => {
  it('COS skipPercentage auto-completes scrum stories', () => {
    const model = parseSimMl(`
      <simulation name="Scrum COS Skip">
        <execute type="scrum" limitIntervalsTo="10"><visual /></execute>
        <setup>
          <iteration storyPointsPerIterationLowBound="10" storyPointsPerIterationHighBound="10" />
          <backlog type="custom">
            <custom name="Story" count="5" estimateLowBound="10" estimateHighBound="10" classOfService="AutoDone" />
          </backlog>
          <classOfServices>
            <classOfService order="1" default="false" skipPercentage="100">AutoDone</classOfService>
          </classOfServices>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(5)
    // All skipped, should finish in 1 iteration
    expect(visual.totalSteps).toBeLessThanOrEqual(1)
  })
})

describe('variable and parameter preprocessing', () => {
  it('substitutes variables in SimML', () => {
    const model = parseSimMl(`
      <?variable name="@count" value="6"?>
      <simulation name="Var Test">
        <execute>
          <visual />
        </execute>
        <setup>
          <backlog type="simple" simpleCount="@count" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="3">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.setup.backlog.simpleCount).toBe(6)
    expect(model.setup.backlog.items).toHaveLength(6)
  })
})

describe('kanban: pull order', () => {
  it('parses FIFOStrict pull order and keeps earlier cards ahead', () => {
    const model = parseSimMl(`
      <simulation name="FIFO strict">
        <execute pullOrder="FIFOStrict" limitIntervalsTo="20">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <custom name="Card A" count="1" estimateLowBound="1" estimateHighBound="1" />
            <custom name="Card B" count="1" estimateLowBound="1" estimateHighBound="1" />
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="2">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.execute.pullOrder).toBe('FIFOStrict')

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(2)
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
    expect(visual.snapshots.find((s) => s.step === 1)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Card A 1', 'Card B 1'])
  })
})

describe('kanban: prerequisite deliverables', () => {
  it('does not pull dependent deliverables before prerequisites are complete', () => {
    const model = parseSimMl(`
      <simulation name="Prerequisites" locale="en-US">
        <execute limitIntervalsTo="30">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="A">
              <custom name="A item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="B" preRequisiteDeliverables="A">
              <custom name="B item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
    expect(visual.snapshots.find((s) => s.step === 1)?.columns[0]?.cards.map((c) => c.label)).toEqual(['A item 1'])
    expect(visual.snapshots.find((s) => s.step === 2)?.columns[0]?.cards.map((c) => c.label)).toEqual(['B item 1'])
  })

  it('keeps a deliverable blocked when its prerequisite deliverable does not exist', () => {
    const model = parseSimMl(`
      <simulation name="Missing Prerequisite" locale="en-US">
        <execute limitIntervalsTo="10">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="B" preRequisiteDeliverables="A">
              <custom name="B item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.completedItems).toBe(0)
    expect(visual.snapshots.at(-1)?.backlogCount).toBe(1)
    expect(visual.snapshots.every((s) => s.columns[0]?.cards.length === 0)).toBe(true)
  })
})

describe('kanban: earliest start date', () => {
  it('holds deliverable cards in backlog until earliestStartDate', () => {
    const model = parseSimMl(`
      <simulation name="Earliest Start" locale="en-US">
        <execute limitIntervalsTo="30">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="Early" earliestStartDate="01-Jan-2020">
              <custom name="Early item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="Late" earliestStartDate="10-Jan-2020">
              <custom name="Late item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
          <forecastDate startDate="01-Jan-2020" workDays="monday|tuesday|wednesday|thursday|friday" intervalsToOneDay="1" />
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    const stepFive = visual.snapshots.find((s) => s.step === 5)
    expect(stepFive?.backlogCount).toBeGreaterThan(0)
    expect(visual.snapshots.find((s) => s.step === 6)?.backlogCount).toBeGreaterThan(0)
    expect(visual.snapshots.find((s) => s.step === 7)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Late item 1'])
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
  })

  it('respects excluded dates when evaluating earliestStartDate', () => {
    const model = parseSimMl(`
      <simulation name="Earliest Start With Excluded Date" locale="en-US">
        <execute limitIntervalsTo="30">
          <visual />
        </execute>
        <setup>
          <backlog type="custom" shuffle="false">
            <deliverable name="Early" earliestStartDate="01-Jan-2020">
              <custom name="Early item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
            <deliverable name="Late" earliestStartDate="10-Jan-2020">
              <custom name="Late item" count="1" estimateLowBound="1" estimateHighBound="1" />
            </deliverable>
          </backlog>
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
          <forecastDate
            startDate="01-Jan-2020"
            workDays="monday|tuesday|wednesday|thursday|friday"
            intervalsToOneDay="1"
          >
            <excludeDate date="09-Jan-2020" />
          </forecastDate>
        </setup>
      </simulation>
    `)

    const visual = runVisualSimulation(model)
    expect(visual.snapshots.find((s) => s.step === 5)?.backlogCount).toBeGreaterThan(0)
    expect(visual.snapshots.find((s) => s.step === 6)?.columns[0]?.cards.map((c) => c.label)).toEqual(['Late item 1'])
    expect(visual.snapshots.at(-1)?.doneCount).toBe(2)
  })
})


describe('kanban: pull order parsing defaults and aliases', () => {
  it('defaults pullOrder to randomAfterOrdering', () => {
    const model = parseSimMl(`
      <simulation name="PullOrder Default">
        <execute><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="1" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(model.execute.pullOrder).toBe('randomAfterOrdering')
  })

  it('supports legacy pullOrder aliases', () => {
    const indexModel = parseSimMl(`
      <simulation name="PullOrder Alias Index">
        <execute pullOrder="index"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="1" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    const afterOrderingModel = parseSimMl(`
      <simulation name="PullOrder Alias AfterOrdering">
        <execute pullOrder="afterOrdering"><visual /></execute>
        <setup>
          <backlog type="simple" simpleCount="1" />
          <columns>
            <column id="1" estimateLowBound="1" estimateHighBound="1" wipLimit="1">Work</column>
          </columns>
        </setup>
      </simulation>
    `)

    expect(indexModel.execute.pullOrder).toBe('indexSequence')
    expect(afterOrderingModel.execute.pullOrder).toBe('randomAfterOrdering')
  })
})
