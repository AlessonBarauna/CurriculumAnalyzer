import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ApiConfigService {
  get apiUrl(): string {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5080/api'
      : 'https://curriculumanalyzer-production.up.railway.app/api';
  }
}
