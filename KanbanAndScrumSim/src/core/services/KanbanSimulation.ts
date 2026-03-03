import { 
    SimulationData, 
    Card, 
    TimeInterval, 
    Column,
    CardStatusEnum,
    CardPosition
} from '../models/types';
import { OrderedList } from '../utils/OrderedList';
import { CardPriorityComparer } from '../utils/CardPriorityComparer';
import { Distribution } from '../utils/Distribution';

export class KanbanSimulation {
    private disposed = false;
    private simulationData: SimulationData;
    private backlogList: OrderedList<Card>;
    private completedWorkList: Card[] = [];
    private allCardsList: Card[] = [];
    private intervals: TimeInterval[] = [];
    private columnDistributions = new Map<Column, Distribution>();
    private backlogColumnDistributions = new Map<Column, Distribution>();
    private defectBacklog = new Map<number, Card[]>();
    private cardMoveSequence = 0;

    constructor(data: SimulationData) {
        this.simulationData = data;
    }

    public runSimulation(): boolean {
        const intervalTime = 1.0;
        this.completedWorkList = [];
        this.allCardsList = [];
        this.intervals = [];

        // Calculate and cache column max WIPs including phases
        this.simulationData.setup.columns.forEach(column => {
            column.highestWipLimit = this.findMaximumColumnWip(column);
        });

        this.backlogList = this.buildBacklog();
        this.buildDistributions();

        try {
            // Create first interval
            const firstInterval = this.createInitialInterval();
            this.intervals.push(firstInterval);

            // Run simulation iterations
            for (let iter = 1; iter < this.simulationData.execute.limitIntervalsTo; iter++) {
                const thisInterval = this.stepOneInterval(iter, intervalTime);
                
                const completePct = 
                    (thisInterval.countCompletedCards / this.allCardsList.length) * 100;
                
                const currentActivePositionsPct = 100.0 - 
                    ((thisInterval.totalWipLimitBoardPositions - 
                      thisInterval.countTotalCardsOnBoard()) / 
                     thisInterval.totalWipLimitBoardPositions) * 100;

                if (completePct >= this.simulationData.execute.completePercentage &&
                    currentActivePositionsPct <= this.simulationData.execute.activePositionsCompletePercentage) {
                    break;
                }
            }
        } finally {
            this.dispose();
        }

        return this.intervals.length < this.simulationData.execute.limitIntervalsTo;
    }

    private stepOneInterval(intervalSequence: number, intervalTime: number): TimeInterval {
        // Create new interval
        const thisInterval = this.createTimeInterval(intervalSequence, intervalTime);
        this.intervals.push(thisInterval);

        // Process each column in reverse order (from last to first)
        for (const column of [...this.simulationData.setup.columns].sort((a, b) => b.id - a.id)) {
            this.incrementTimeSoFarForColumn(thisInterval, column, intervalTime);
            const wipViolatorsAddedThisInterval: Card[] = [];

            // Get next and prior columns
            const nextColumn = this.getNextColumnForColumnAndCard(column);
            const priorColumn = this.getNextColumnForColumnAndCard(column, -1);

            // Get positions to process in pull order
            const positionsToProcess = this.getPositionOrderToProcessList(thisInterval, column);

            // Process each position
            for (const position of positionsToProcess) {
                if (thisInterval.previousTimeInterval) {
                    const card = thisInterval.previousTimeInterval.getCardInPositionForColumn(column, position);
                    
                    if (card) {
                        const nextColumnForCard = this.getNextColumnForColumnAndCard(column, 1, card);
                        const timeSoFar = card.timeSoFar.get(column) || 0;
                        const requiredTime = card.getCalculatedRandomWorkTimeForColumn(column) + 
                                          this.expediteBlockingEventProcessor.getBlockTimeForCard(column, card);

                        if (timeSoFar >= requiredTime) {
                            // Card has spent enough time in column
                            if (this.isCardBlocked(column, card, intervalTime)) {
                                this.moveCardToNextInterval(thisInterval, column, position, card);
                                card.status = CardStatusEnum.Blocked;
                            } else if (this.isColumnAllowedToCompleteWork(column) && 
                                     this.isStrictFIFOAllowsComplete(thisInterval, column, card)) {
                                // Handle card completion or movement
                                if (!nextColumnForCard || column === this.getLastColumn()) {
                                    this.completeCard(thisInterval, column, position);
                                    card.status = CardStatusEnum.Completed;
                                } else {
                                    if (card.classOfService?.violateWIP) {
                                        this.moveCard(thisInterval, column, position, nextColumnForCard, -1);
                                        wipViolatorsAddedThisInterval.push(card);
                                    } else {
                                        const availablePosition = this.nextAvailablePositionInColumn(
                                            thisInterval, 
                                            nextColumnForCard
                                        );
                                        
                                        if (availablePosition > -1) {
    // ... implementing other methods
} 