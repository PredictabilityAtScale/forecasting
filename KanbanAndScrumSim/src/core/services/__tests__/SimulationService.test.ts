import { SimulationService } from '../SimulationService';

describe('SimulationService', () => {
    it('should correctly simulate basic scenario', () => {
        const config = {
            numberOfIterations: 100,
            teamSize: 5
        };
        
        const service = new SimulationService(config);
        const result = service.runSimulation();
        
        expect(result.iterationResults.length).toBe(100);
        // Add more assertions
    });
}); 