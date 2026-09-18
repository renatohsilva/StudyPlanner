import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { ExamService } from '../../core/services/exam.service';
import { ExamDetail } from '../../core/models/exam.models';

@Component({
  selector: 'app-exam-setup',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './exam-setup.component.html',
  styleUrl: './exam-setup.component.scss'
})
export class ExamSetupComponent implements OnInit {
  private readonly appState = inject(AppStateService);
  private readonly examService = inject(ExamService);
  private readonly router = inject(Router);

  readonly exam = signal<ExamDetail | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Formulário: novo concurso
  newExamName = '';
  newExamDate = '';

  // Formulário: nova disciplina
  newSubjectName = '';
  newSubjectWeight = 0.1;

  // Formulário: novo tópico, por disciplina (mapa subjectId -> valores do form)
  newTopicName: Record<string, string> = {};
  newTopicIncidence: Record<string, number> = {};

  ngOnInit(): void {
    const examId = this.appState.currentExamId();
    if (examId) {
      this.loadExam(examId);
    }
  }

  createExam(): void {
    if (!this.newExamName || !this.newExamDate) return;

    this.loading.set(true);
    this.examService
      .createExam({
        userId: this.appState.userId(),
        boardId: null,
        name: this.newExamName,
        examDate: this.newExamDate
      })
      .subscribe({
        next: ({ id }) => {
          this.appState.setCurrentExam(id);
          this.newExamName = '';
          this.newExamDate = '';
          this.loadExam(id);
        },
        error: () => {
          this.errorMessage.set('Não foi possível criar o concurso.');
          this.loading.set(false);
        }
      });
  }

  createSubject(): void {
    const examId = this.appState.currentExamId();
    if (!examId || !this.newSubjectName || this.newSubjectWeight <= 0) return;

    this.examService
      .createSubject({ examId, name: this.newSubjectName, weight: this.newSubjectWeight })
      .subscribe({
        next: () => {
          this.newSubjectName = '';
          this.newSubjectWeight = 0.1;
          this.loadExam(examId);
        },
        error: () => this.errorMessage.set('Não foi possível criar a disciplina.')
      });
  }

  createTopic(subjectId: string): void {
    const examId = this.appState.currentExamId();
    const name = this.newTopicName[subjectId];
    const incidence = this.newTopicIncidence[subjectId];
    if (!examId || !name || !incidence) return;

    this.examService
      .createTopic({ subjectId, parentTopicId: null, name, estimatedIncidence: incidence })
      .subscribe({
        next: () => {
          this.newTopicName[subjectId] = '';
          this.newTopicIncidence[subjectId] = 0;
          this.loadExam(examId);
        },
        error: () => this.errorMessage.set('Não foi possível criar o tópico.')
      });
  }

  goToTodayPlan(): void {
    this.router.navigate(['/dashboard']);
  }

  private loadExam(examId: string): void {
    this.loading.set(true);
    this.examService.getExam(examId).subscribe({
      next: (exam) => {
        this.exam.set(exam);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar o concurso.');
        this.loading.set(false);
      }
    });
  }
}
