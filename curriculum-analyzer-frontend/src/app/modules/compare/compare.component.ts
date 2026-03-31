import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AnalysisService } from '../../shared/services/analysis.service';
import { AnalysisHistoryItem } from '../../shared/models/history.model';
import { CompareResult, CompareSnapshot } from '../../shared/models/compare.model';

@Component({
  selector: 'app-compare',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './compare.component.html',
  styleUrls: ['./compare.component.scss']
})
export class CompareComponent implements OnInit {
  history = signal<AnalysisHistoryItem[]>([]);
  selected = signal<string[]>([]);
  loadingHistory = signal(true);
  loadingCompare = signal(false);
  errorMessage = signal('');
  result = signal<CompareResult | null>(null);

  canCompare = computed(() => this.selected().length === 2);

  constructor(
    private analysisService: AnalysisService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.analysisService.getHistory().subscribe({
      next: (data) => {
        this.history.set(data);
        this.loadingHistory.set(false);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar histórico.');
        this.loadingHistory.set(false);
      }
    });
  }

  toggleSelect(id: string): void {
    const current = this.selected();
    if (current.includes(id)) {
      this.selected.set(current.filter(s => s !== id));
    } else if (current.length < 2) {
      this.selected.set([...current, id]);
    }
  }

  isSelected(id: string): boolean {
    return this.selected().includes(id);
  }

  selectionIndex(id: string): number {
    return this.selected().indexOf(id) + 1;
  }

  compare(): void {
    if (!this.canCompare()) return;
    const [id1, id2] = this.selected();
    this.loadingCompare.set(true);
    this.errorMessage.set('');
    this.result.set(null);

    this.analysisService.compareAnalyses(id1, id2).subscribe({
      next: (data) => {
        this.result.set(data);
        this.loadingCompare.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.error || 'Erro ao comparar análises.');
        this.loadingCompare.set(false);
      }
    });
  }

  getSectionKeys(snapshot: CompareSnapshot): string[] {
    return Object.keys(snapshot.sections);
  }

  scoreDelta(key: string): number {
    const r = this.result();
    if (!r) return 0;
    const before = r.before.sections[key]?.score ?? 0;
    const after = r.after.sections[key]?.score ?? 0;
    return after - before;
  }

  getScoreColor(score: number): string {
    if (score >= 90) return '#4CAF50';
    if (score >= 75) return '#8BC34A';
    if (score >= 60) return '#FFC107';
    if (score >= 40) return '#FF9800';
    return '#F44336';
  }

  selectedItems = computed(() => {
    const ids = this.selected();
    return this.history().filter(h => ids.includes(h.id));
  });

  goBack(): void {
    this.router.navigate(['/history']);
  }
}
