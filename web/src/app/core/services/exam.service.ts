import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ExamDetail } from '../models/exam.models';

export interface CreateExamRequest {
  userId: string;
  boardId: string | null;
  name: string;
  examDate: string;
}

export interface CreateSubjectRequest {
  examId: string;
  name: string;
  weight: number;
}

export interface CreateTopicRequest {
  subjectId: string;
  parentTopicId: string | null;
  name: string;
  estimatedIncidence: number;
}

@Injectable({ providedIn: 'root' })
export class ExamService {
  private readonly http = inject(HttpClient);

  createExam(request: CreateExamRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/exams`, request);
  }

  getExam(examId: string): Observable<ExamDetail> {
    return this.http.get<ExamDetail>(`${API_BASE_URL}/exams/${examId}`);
  }

  createSubject(request: CreateSubjectRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/subjects`, request);
  }

  createTopic(request: CreateTopicRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/topics`, request);
  }
}
