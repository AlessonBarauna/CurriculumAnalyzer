import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AnalysisService } from '../../shared/services/analysis.service';
import { AnalysisHistoryItem } from '../../shared/models/history.model';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  history: AnalysisHistoryItem[] = [];
  loading = true;
  errorMessage = '';

  constructor(
    private analysisService: AnalysisService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.analysisService.getHistory().subscribe({
      next: (data) => {
        this.history = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Erro ao carregar histórico.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openAnalysis(id: string): void {
    this.router.navigate(['/analysis', id]);
  }

  newAnalysis(): void {
    this.router.navigate(['/']);
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

  getLevelLabel(level: string): string {
    const map: Record<string, string> = {
      'junior': 'Junior',
      'mid-level': 'Mid-Level',
      'senior': 'Sênior'
    };
    return map[level] ?? level;
  }
}
