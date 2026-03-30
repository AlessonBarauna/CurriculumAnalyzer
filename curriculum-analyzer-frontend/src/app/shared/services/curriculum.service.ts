import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from './api-config.service';

@Injectable({ providedIn: 'root' })
export class CurriculumService {
  constructor(private http: HttpClient, private config: ApiConfigService) {}

  private get apiUrl() { return this.config.apiUrl; }

  uploadAndAnalyze(formData: FormData): Observable<{ analysisId: string }> {
    return this.http.post<{ analysisId: string }>(`${this.apiUrl}/curriculum/upload-and-analyze`, formData);
  }
}
