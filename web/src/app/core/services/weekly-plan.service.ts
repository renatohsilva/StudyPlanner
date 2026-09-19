import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { WeeklyPlan } from '../models/weekly-plan.models';

@Injectable({ providedIn: 'root' })
export class WeeklyPlanService {
  private readonly http = inject(HttpClient);

  getActive(examId: string, userId: string): Observable<WeeklyPlan> {
    return this.http.get<WeeklyPlan>(`${API_BASE_URL}/exams/${examId}/study-plan`, { params: { userId } });
  }

  generate(examId: string, userId: string, weekStartDate: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/exams/${examId}/study-plan/generate`, { userId, weekStartDate });
  }
}
