import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ExamMetrics } from '../models/metrics.models';

@Injectable({ providedIn: 'root' })
export class MetricsService {
  private readonly http = inject(HttpClient);

  getExamMetrics(examId: string, userId: string): Observable<ExamMetrics> {
    return this.http.get<ExamMetrics>(`${API_BASE_URL}/exams/${examId}/metrics`, { params: { userId } });
  }
}
