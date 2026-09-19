import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AttemptResult, QuestionSummary } from '../models/question.models';

export interface CreateQuestionRequest {
  statement: string;
  alternativesJson: string | null;
  correctAnswer: string;
  source: 'Official' | 'AiGenerated' | 'UserCreated';
  boardId: string | null;
  year: number | null;
  createdByUserId: string | null;
  topicIds: string[];
}

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private readonly http = inject(HttpClient);

  getByTopic(topicId: string, userId: string): Observable<QuestionSummary[]> {
    return this.http.get<QuestionSummary[]>(`${API_BASE_URL}/topics/${topicId}/questions`, { params: { userId } });
  }

  create(request: CreateQuestionRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${API_BASE_URL}/questions`, request);
  }

  attempt(questionId: string, userId: string, chosenAnswer: string): Observable<AttemptResult> {
    return this.http.post<AttemptResult>(`${API_BASE_URL}/questions/${questionId}/attempt`, {
      userId,
      chosenAnswer,
      timeSpentSeconds: null
    });
  }
}
