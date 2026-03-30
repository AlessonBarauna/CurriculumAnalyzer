import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CurriculumService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  uploadAndAnalyze(formData: FormData): Observable<{ analysisId: string }> {
    return this.http.post<{ analysisId: string }>(`${this.apiUrl}/curriculum/upload-and-analyze`, formData);
  }
}
