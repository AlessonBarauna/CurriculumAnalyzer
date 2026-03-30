import { Routes } from '@angular/router';
import { UploadComponent } from './modules/upload/upload.component';
import { AnalysisReportComponent } from './modules/analysis-report/analysis-report.component';

export const routes: Routes = [
  { path: '', component: UploadComponent },
  { path: 'analysis/:id', component: AnalysisReportComponent },
  { path: '**', redirectTo: '' }
];
