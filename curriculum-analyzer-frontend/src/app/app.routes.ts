import { Routes } from '@angular/router';
import { authGuard } from './shared/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./modules/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./modules/upload/upload.component').then(m => m.UploadComponent)
  },
  {
    path: 'history',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./modules/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'analysis/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./modules/analysis-report/analysis-report.component').then(m => m.AnalysisReportComponent)
  },
  { path: '**', redirectTo: '' }
];
