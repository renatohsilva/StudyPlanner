import { Routes } from '@angular/router';
import { ExamSetupComponent } from './features/exam-setup/exam-setup.component';
import { TodayPlanComponent } from './features/today-plan/today-plan.component';

export const routes: Routes = [
  { path: '', redirectTo: 'exam-setup', pathMatch: 'full' },
  { path: 'exam-setup', component: ExamSetupComponent },
  { path: 'dashboard', component: TodayPlanComponent },
  { path: '**', redirectTo: 'exam-setup' }
];
