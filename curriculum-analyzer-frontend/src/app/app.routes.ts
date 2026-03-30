import { Routes } from '@angular/router';
import { UploadComponent } from './modules/upload/upload.component';
import { AnalysisReportComponent } from './modules/analysis-report/analysis-report.component';
import { HistoryComponent } from './modules/history/history.component';

export const routes: Routes = [
  { path: '', component: UploadComponent },
  { path: 'history', component: HistoryComponent },
  { path: 'analysis/:id', component: AnalysisReportComponent },
  { path: '**', redirectTo: '' }
];
