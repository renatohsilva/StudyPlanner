import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';

export interface CreateStudySessionRequest {
  userId: string;
  topicId: string;
  studyPlanItemId: string | null;
  durationMinutes: number;
}

@Injectable({ providedIn: 'root' })
export class StudySessionService {
  private readonly http = inject(HttpClient);

  create(request: CreateStudySessionRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/study-sessions`, request);
  }
}
