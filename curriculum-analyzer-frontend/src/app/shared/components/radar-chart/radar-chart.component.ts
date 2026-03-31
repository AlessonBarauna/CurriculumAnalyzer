import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SectionScore } from '../../models/analysis.model';

interface RadarPoint { x: number; y: number; }

const CX = 160;
const CY = 150;
const R  = 110;
const SECTION_LABELS: Record<string, string> = {
  structure:  'Estrutura',
  contact:    'Contato',
  summary:    'Resumo',
  experience: 'Experiência',
  skills:     'Habilidades',
  education:  'Educação',
  projects:   'Projetos'
};

@Component({
  selector: 'app-radar-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './radar-chart.component.html',
  styleUrls: ['./radar-chart.component.scss']
})
export class RadarChartComponent {
  sections = input.required<{ [key: string]: SectionScore }>();

  axes = computed(() => {
    const keys = Object.keys(this.sections());
    const n = keys.length;
    return keys.map((key, i) => {
      const angle = (i / n) * 2 * Math.PI - Math.PI / 2;
      return {
        key,
        label: SECTION_LABELS[key] ?? key,
        score: this.sections()[key].score,
        tip:   this.point(R, angle),
        label_pos: this.labelPos(angle)
      };
    });
  });

  gridPolygons = computed(() =>
    [25, 50, 75, 100].map(pct => this.polygonPoints(pct / 100))
  );

  dataPolygon = computed(() => {
    const keys = Object.keys(this.sections());
    const n = keys.length;
    return keys.map((key, i) => {
      const angle = (i / n) * 2 * Math.PI - Math.PI / 2;
      return this.point(R * (this.sections()[key].score / 100), angle);
    });
  });

  toSvgPoints(pts: RadarPoint[]): string {
    return pts.map(p => `${p.x},${p.y}`).join(' ');
  }

  private point(r: number, angle: number): RadarPoint {
    return { x: CX + r * Math.cos(angle), y: CY + r * Math.sin(angle) };
  }

  private polygonPoints(fraction: number): string {
    const keys = Object.keys(this.sections());
    const n = keys.length;
    return keys.map((_, i) => {
      const angle = (i / n) * 2 * Math.PI - Math.PI / 2;
      const p = this.point(R * fraction, angle);
      return `${p.x},${p.y}`;
    }).join(' ');
  }

  private labelPos(angle: number): { x: number; y: number; anchor: string } {
    const margin = 26;
    const x = CX + (R + margin) * Math.cos(angle);
    const y = CY + (R + margin) * Math.sin(angle);
    const anchor = Math.cos(angle) > 0.1 ? 'start'
                 : Math.cos(angle) < -0.1 ? 'end'
                 : 'middle';
    return { x, y, anchor };
  }
}
