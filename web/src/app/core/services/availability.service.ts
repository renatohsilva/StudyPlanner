import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AvailabilitySlot } from '../models/availability.models';

@Injectable({ providedIn: 'root' })
export class AvailabilityService {
  private readonly http = inject(HttpClient);

  get(examId: string, userId: string): Observable<AvailabilitySlot[]> {
    return this.http.get<AvailabilitySlot[]>(`${API_BASE_URL}/exams/${examId}/availability`, { params: { userId } });
  }

  set(examId: string, userId: string, slots: AvailabilitySlot[]): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/exams/${examId}/availability`, { userId, slots });
  }
}
