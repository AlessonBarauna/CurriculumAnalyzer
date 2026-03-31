import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CurriculumAnalysis } from '../models/analysis.model';
import { AnalysisHistoryItem } from '../models/history.model';
import { CompareResult } from '../models/compare.model';
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

  deleteAnalysis(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/analysis/${id}`);
  }

  compareAnalyses(id1: string, id2: string): Observable<CompareResult> {
    return this.http.get<CompareResult>(`${this.apiUrl}/analysis/compare?id1=${id1}&id2=${id2}`);
  }
}
