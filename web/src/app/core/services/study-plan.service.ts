import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { TodayPlan } from '../models/study-plan.models';

@Injectable({ providedIn: 'root' })
export class StudyPlanService {
  private readonly http = inject(HttpClient);

  getToday(userId: string, examId: string, top = 5): Observable<TodayPlan> {
    return this.http.get<TodayPlan>(`${API_BASE_URL}/study-plan/today`, {
      params: { userId, examId, top }
    });
  }
}
