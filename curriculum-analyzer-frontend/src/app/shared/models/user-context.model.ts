export interface UserContext {
  experienceLevel: 'junior' | 'mid-level' | 'senior';
  specialization: string;
  marketObjective: string;
  targetSalary?: number;
  currentLocation: string;
}
