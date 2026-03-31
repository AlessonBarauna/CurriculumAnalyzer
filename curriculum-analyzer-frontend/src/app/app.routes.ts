import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./modules/upload/upload.component').then(m => m.UploadComponent)
  },
  {
    path: 'history',
    loadComponent: () =>
      import('./modules/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'analysis/:id',
    loadComponent: () =>
      import('./modules/analysis-report/analysis-report.component').then(m => m.AnalysisReportComponent)
  },
  { path: '**', redirectTo: '' }
];
