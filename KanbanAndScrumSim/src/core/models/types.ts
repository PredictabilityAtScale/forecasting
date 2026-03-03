export interface Card {
    name: string;
    index: number;
    status: CardStatusEnum;
    cardType: CardTypeEnum;
    classOfService?: ClassOfService;
    customBacklog?: CustomBacklog;
    deliverable?: Deliverable;
    pullOrder?: number;
    timeSoFar: Map<Column, number>;
    statusHistory: Map<number, CardStatusEnum>;
}

export interface Column {
    id: number;
    name: string;
    sequence: number;
    wipLimit: number;
    highestWipLimit: number;
    isBuffer: boolean;
    skipPercentage: number;
    replenishInterval: number;
    completeInterval: number;
}

export interface TimeInterval {
    simulator: KanbanSimulation;
    sequence: number;
    elapsedTime: number;
    previousTimeInterval: TimeInterval | null;
    cardPositions: Map<Column, CardPosition[]>;
    countCardsInBacklog: number;
    countCompletedCards: number;
    totalWipLimitBoardPositions: number;
    valueDeliveredSoFar?: number;
    currentDate?: Date;
}

export interface CardPosition {
    position: number;
    card: Card;
    hasViolatedWIP: boolean;
}

export enum CardStatusEnum {
    InBacklog = 'InBacklog',
    NewStatusThisInterval = 'NewStatusThisInterval',
    SameStatusThisInterval = 'SameStatusThisInterval',
    Blocked = 'Blocked',
    CompletedButWaitingForFreePosition = 'CompletedButWaitingForFreePosition',
    Completed = 'Completed'
}

export enum CardTypeEnum {
    Work = 'Work',
    Defect = 'Defect'
}

export interface SimulationData {
    setup: SimulationSetup;
    execute: SimulationExecute;
}

// ... other interfaces will be added as needed 