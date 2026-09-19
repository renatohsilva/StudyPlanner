import { Routes } from '@angular/router';
import { ExamSetupComponent } from './features/exam-setup/exam-setup.component';
import { TodayPlanComponent } from './features/today-plan/today-plan.component';
import { MaterialsComponent } from './features/materials/materials.component';
import { QuestionsComponent } from './features/questions/questions.component';
import { WeeklyPlanComponent } from './features/weekly-plan/weekly-plan.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'exam-setup', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'exam-setup', component: ExamSetupComponent, canActivate: [authGuard] },
  { path: 'dashboard', component: TodayPlanComponent, canActivate: [authGuard] },
  { path: 'weekly-plan', component: WeeklyPlanComponent, canActivate: [authGuard] },
  { path: 'materials', component: MaterialsComponent, canActivate: [authGuard] },
  { path: 'questions', component: QuestionsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'exam-setup' }
];
