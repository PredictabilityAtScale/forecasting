export interface SimulationConfig {
    // Convert C# properties to TypeScript
    numberOfIterations: number;
    teamSize: number;
    // ... other configuration options
}

export interface SimulationResult {
    // Define result structure
    iterationResults: IterationResult[];
    summary: SimulationSummary;
}

// Add other necessary interfaces 