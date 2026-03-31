import { Feedback, SectionScore } from './analysis.model';

export interface CompareSnapshot {
  sections: { [key: string]: SectionScore };
  strengths: Feedback[];
  weaknesses: Feedback[];
}

export interface CompareResult {
  before: CompareSnapshot;
  after: CompareSnapshot;
}
