import { Injectable, computed, inject, signal } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Estado local do app: o exame "ativo" que o usuário está montando/estudando, mais um atalho
 * para o userId do usuário autenticado (ver AuthService — é a fonte de verdade).
 */
@Injectable({ providedIn: 'root' })
export class AppStateService {
  private static readonly EXAM_ID_KEY = 'studyplanner.currentExamId';

  private readonly authService = inject(AuthService);

  readonly userId = computed(() => this.authService.userId() ?? '');
  readonly currentExamId = signal(localStorage.getItem(AppStateService.EXAM_ID_KEY));

  setCurrentExam(examId: string): void {
    localStorage.setItem(AppStateService.EXAM_ID_KEY, examId);
    this.currentExamId.set(examId);
  }
}
