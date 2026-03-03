import React, { useState } from 'react';
import { SimulationService } from '../core/services/SimulationService';
import { SimulationConfig, SimulationResult } from '../core/models/SimulationTypes';

export const SimulationRunner: React.FC = () => {
    const [result, setResult] = useState<SimulationResult | null>(null);

    const runSimulation = () => {
        const config: SimulationConfig = {
            numberOfIterations: 1000,
            teamSize: 5,
            // ... other config
        };

        const simulator = new SimulationService(config);
        const simulationResult = simulator.runSimulation();
        setResult(simulationResult);
    };

    return (
        <div>
            <button onClick={runSimulation}>Run Simulation</button>
            {result && (
                <div>
                    {/* Display simulation results */}
                </div>
            )}
        </div>
    );
}; 