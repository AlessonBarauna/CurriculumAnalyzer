import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AnalysisService } from '../../shared/services/analysis.service';
import { AnalysisHistoryItem } from '../../shared/models/history.model';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  history = signal<AnalysisHistoryItem[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  deletingId = signal<string | null>(null);

  searchQuery = signal('');
  filterLevel = signal('');
  filterSpec = signal('');

  filteredHistory = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const level = this.filterLevel();
    const spec = this.filterSpec();

    return this.history().filter(item => {
      const matchesQuery = !query || item.fileName.toLowerCase().includes(query);
      const matchesLevel = !level || item.experienceLevel === level;
      const matchesSpec = !spec || item.specialization === spec;
      return matchesQuery && matchesLevel && matchesSpec;
    });
  });

  constructor(
    private analysisService: AnalysisService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.analysisService.getHistory().subscribe({
      next: (data) => {
        this.history.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar histórico.');
        this.loading.set(false);
      }
    });
  }

  openAnalysis(id: string): void {
    this.router.navigate(['/analysis', id]);
  }

  confirmDelete(event: Event, id: string): void {
    event.stopPropagation();
    if (!confirm('Excluir esta análise? Esta ação não pode ser desfeita.')) return;

    this.deletingId.set(id);
    this.analysisService.deleteAnalysis(id).subscribe({
      next: () => {
        this.history.update(list => list.filter(item => item.id !== id));
        this.deletingId.set(null);
      },
      error: () => {
        this.deletingId.set(null);
        alert('Erro ao excluir análise. Tente novamente.');
      }
    });
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
