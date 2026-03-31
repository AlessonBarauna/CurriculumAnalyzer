import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { AnalysisService } from '../../shared/services/analysis.service';
import { CurriculumAnalysis, ActionItem } from '../../shared/models/analysis.model';

type Tab = 'overview' | 'strengths' | 'weaknesses' | 'plan' | 'jobs';

@Component({
  selector: 'app-analysis-report',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './analysis-report.component.html',
  styleUrls: ['./analysis-report.component.scss']
})
export class AnalysisReportComponent implements OnInit {
  analysis = signal<CurriculumAnalysis | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  activeTab = signal<Tab>('overview');

  readonly circumference = 2 * Math.PI * 52;
  scoreOffset = signal(2 * Math.PI * 52);

  constructor(
    private route: ActivatedRoute,
    private analysisService: AnalysisService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage.set('ID de análise não encontrado.');
      this.loading.set(false);
      return;
    }

    this.analysisService.getAnalysis(id).subscribe({
      next: (data) => {
        this.analysis.set(data);
        this.loading.set(false);
        setTimeout(() => {
          this.scoreOffset.set(this.circumference * (1 - data.overallScore / 100));
        }, 60);
      },
      error: (err) => {
        this.errorMessage.set(`Erro: ${err?.status ?? ''} ${err?.message ?? ''}`);
        this.loading.set(false);
      }
    });
  }

  getSectionEntries(): { key: string; value: any }[] {
    const sections = this.analysis()?.sections;
    if (!sections) return [];
    return Object.entries(sections).map(([key, value]) => ({ key, value }));
  }

  getActionPlanByTimeline(timeline: string): ActionItem[] {
    return this.analysis()?.actionPlan?.filter(item => item.timeline === timeline) ?? [];
  }

  getScoreColor(score: number): string {
    if (score >= 90) return '#4CAF50';
    if (score >= 75) return '#8BC34A';
    if (score >= 60) return '#FFC107';
    if (score >= 40) return '#FF9800';
    return '#F44336';
  }

  getScoreLabel(score: number): string {
    if (score >= 90) return 'EXCELENTE';
    if (score >= 75) return 'BOM';
    if (score >= 60) return 'REGULAR';
    if (score >= 40) return 'FRACO';
    return 'CRÍTICO';
  }

  getSectionLabel(key: string): string {
    const labels: Record<string, string> = {
      structure: 'Estrutura',
      contact: 'Contato',
      summary: 'Resumo Profissional',
      experience: 'Experiência',
      skills: 'Habilidades',
      education: 'Educação',
      projects: 'Projetos'
    };
    return labels[key] ?? key;
  }

  getJobTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      startup: '🚀 Startup',
      'big-tech': '🏢 Big Tech',
      consulting: '🔧 Consultoria',
      freelance: '💻 Freelance'
    };
    return labels[type] ?? type;
  }

}
