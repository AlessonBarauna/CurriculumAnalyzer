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
      import('./modules/shell/shell.component').then(m => m.ShellComponent),
    children: [
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
        path: 'compare',
        loadComponent: () =>
          import('./modules/compare/compare.component').then(m => m.CompareComponent)
      },
      {
        path: 'analysis/:id',
        loadComponent: () =>
          import('./modules/analysis-report/analysis-report.component').then(m => m.AnalysisReportComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
