import { Routes } from '@angular/router';
import { ExamSetupComponent } from './features/exam-setup/exam-setup.component';
import { TodayPlanComponent } from './features/today-plan/today-plan.component';
import { MaterialsComponent } from './features/materials/materials.component';

export const routes: Routes = [
  { path: '', redirectTo: 'exam-setup', pathMatch: 'full' },
  { path: 'exam-setup', component: ExamSetupComponent },
  { path: 'dashboard', component: TodayPlanComponent },
  { path: 'materials', component: MaterialsComponent },
  { path: '**', redirectTo: 'exam-setup' }
];
