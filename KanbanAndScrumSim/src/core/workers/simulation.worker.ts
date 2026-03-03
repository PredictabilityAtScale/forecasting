import { SimulationService } from '../services/SimulationService';

self.onmessage = (e: MessageEvent) => {
    const { config } = e.data;
    const simulator = new SimulationService(config);
    const result = simulator.runSimulation();
    
    self.postMessage(result);
}; 