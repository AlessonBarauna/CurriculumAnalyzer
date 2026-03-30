export interface SectionScore {
  name?: string;
  score: number;
  weight?: number;
  feedback: string;
}

export interface ExampleFeedback {
  bad: string;
  good: string;
}

export interface Feedback {
  id?: string;
  priority: 'critical' | 'high' | 'medium' | 'low';
  title: string;
  description: string;
  impact: string;
  example?: ExampleFeedback;
  solutionSteps?: string[];
}

export interface Opportunity {
  id?: string;
  title: string;
  description: string;
  timelineWeeks: number;
  estimatedSalaryImpact: number;
  difficulty: 'easy' | 'medium' | 'hard';
}

export interface ActionItem {
  id?: string;
  priority: 'urgent' | 'high' | 'medium';
  timeline: 'short-term' | 'medium-term' | 'long-term';
  title: string;
  description: string;
  checklist: string[];
  estimatedDuration: string;
}

export interface JobRecommendation {
  type: 'startup' | 'big-tech' | 'consulting' | 'freelance';
  fit: 'strong' | 'good' | 'potential';
  strengths: string[];
  improvements: string[];
  preparationTips: string[];
}

export interface SalaryRange {
  min: number;
  max: number;
  currency: string;
}

export interface CurriculumAnalysis {
  id: string;
  curriculumId: string;
  analysisDate: Date;
  overallScore: number;
  scoreExplanation: string;
  sections: { [key: string]: SectionScore };
  strengths: Feedback[];
  weaknesses: Feedback[];
  opportunities: Opportunity[];
  actionPlan: ActionItem[];
  jobRecommendations: JobRecommendation[];
  estimatedSalaryRange: SalaryRange;
}
