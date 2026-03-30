import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CurriculumAnalysis } from '../models/analysis.model';
import { AnalysisHistoryItem } from '../models/history.model';
import { ApiConfigService } from './api-config.service';

@Injectable({ providedIn: 'root' })
export class AnalysisService {
  constructor(private http: HttpClient, private config: ApiConfigService) {}

  private get apiUrl() { return this.config.apiUrl; }

  getHistory(): Observable<AnalysisHistoryItem[]> {
    return this.http.get<AnalysisHistoryItem[]>(`${this.apiUrl}/analysis`);
  }

  getAnalysis(id: string): Observable<CurriculumAnalysis> {
    return this.http.get<CurriculumAnalysis>(`${this.apiUrl}/analysis/${id}`);
  }

  exportPDF(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/analysis/${id}/export-pdf`, { responseType: 'blob' });
  }
}
