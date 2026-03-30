import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CurriculumAnalysis } from '../models/analysis.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AnalysisService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAnalysis(id: string): Observable<CurriculumAnalysis> {
    return this.http.get<CurriculumAnalysis>(`${this.apiUrl}/analysis/${id}`);
  }

  exportPDF(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/analysis/${id}/export-pdf`, { responseType: 'blob' });
  }
}
