import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { MaterialStatus, MaterialTopicLink } from '../models/material.models';

@Injectable({ providedIn: 'root' })
export class MaterialService {
  private readonly http = inject(HttpClient);

  upload(examId: string, userId: string, file: File): Observable<MaterialStatus> {
    const formData = new FormData();
    formData.append('userId', userId);
    formData.append('file', file);
    return this.http.post<MaterialStatus>(`${API_BASE_URL}/exams/${examId}/materials`, formData);
  }

  getTopics(materialId: string): Observable<MaterialTopicLink[]> {
    return this.http.get<MaterialTopicLink[]>(`${API_BASE_URL}/materials/${materialId}/topics`);
  }

  getByExam(examId: string): Observable<MaterialStatus[]> {
    return this.http.get<MaterialStatus[]>(`${API_BASE_URL}/exams/${examId}/materials`);
  }
}
