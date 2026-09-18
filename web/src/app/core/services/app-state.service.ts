import { Injectable, signal } from '@angular/core';

/**
 * Estado local mínimo para o MVP sem autenticação: um userId gerado e persistido
 * no browser, e o exame "ativo" que o usuário está montando/estudando.
 */
@Injectable({ providedIn: 'root' })
export class AppStateService {
  private static readonly USER_ID_KEY = 'studyplanner.userId';
  private static readonly EXAM_ID_KEY = 'studyplanner.currentExamId';

  readonly userId = signal(this.loadOrCreateUserId());
  readonly currentExamId = signal(localStorage.getItem(AppStateService.EXAM_ID_KEY));

  setCurrentExam(examId: string): void {
    localStorage.setItem(AppStateService.EXAM_ID_KEY, examId);
    this.currentExamId.set(examId);
  }

  private loadOrCreateUserId(): string {
    let id = localStorage.getItem(AppStateService.USER_ID_KEY);
    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem(AppStateService.USER_ID_KEY, id);
    }
    return id;
  }
}
