import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ExtractedExamStructure, NoticeStatusDto } from '../models/notice.models';

@Injectable({ providedIn: 'root' })
export class NoticeService {
  private readonly http = inject(HttpClient);

  upload(examId: string, file: File): Observable<NoticeStatusDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<NoticeStatusDto>(`${API_BASE_URL}/exams/${examId}/notice`, formData);
  }

  getStatus(noticeId: string): Observable<NoticeStatusDto> {
    return this.http.get<NoticeStatusDto>(`${API_BASE_URL}/notices/${noticeId}/status`);
  }

  confirm(noticeId: string, structure: ExtractedExamStructure): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/notices/${noticeId}/confirm`, structure);
  }
}
