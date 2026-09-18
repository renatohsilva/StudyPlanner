import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { PendingReview, WeakPoint } from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly http = inject(HttpClient);

  getPending(userId: string, examId: string): Observable<PendingReview[]> {
    return this.http.get<PendingReview[]>(`${API_BASE_URL}/reviews/pending`, { params: { userId, examId } });
  }

  getWeakPoints(examId: string, userId: string, top = 5): Observable<WeakPoint[]> {
    return this.http.get<WeakPoint[]>(`${API_BASE_URL}/exams/${examId}/weak-points`, { params: { userId, top } });
  }
}
