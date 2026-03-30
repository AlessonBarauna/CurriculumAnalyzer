import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AnalysisService } from '../../shared/services/analysis.service';
import { CurriculumAnalysis, ActionItem } from '../../shared/models/analysis.model';

@Component({
  selector: 'app-analysis-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analysis-report.component.html',
  styleUrls: ['./analysis-report.component.scss']
})
export class AnalysisReportComponent implements OnInit {
  analysis: CurriculumAnalysis | null = null;
  loading = true;
  errorMessage = '';
  activeTab: 'overview' | 'strengths' | 'weaknesses' | 'plan' | 'jobs' = 'overview';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private analysisService: AnalysisService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage = 'ID de análise não encontrado.';
      this.loading = false;
      return;
    }

    this.analysisService.getAnalysis(id).subscribe({
      next: (analysis) => {
        this.analysis = analysis;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = `Erro: ${err?.status ?? ''} ${err?.message ?? ''}`;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getSectionEntries(): { key: string; value: any }[] {
    if (!this.analysis?.sections) return [];
    return Object.entries(this.analysis.sections).map(([key, value]) => ({ key, value }));
  }

  getActionPlanByTimeline(timeline: string): ActionItem[] {
    return this.analysis?.actionPlan?.filter(item => item.timeline === timeline) ?? [];
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

  analyzeAnother(): void {
    this.router.navigate(['/']);
  }
}
