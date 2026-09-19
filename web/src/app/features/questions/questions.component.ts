import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { ExamService } from '../../core/services/exam.service';
import { QuestionService } from '../../core/services/question.service';
import { ExamDetail } from '../../core/models/exam.models';
import { AttemptResult, QuestionAlternative, QuestionSummary } from '../../core/models/question.models';

@Component({
  selector: 'app-questions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './questions.component.html',
  styleUrl: './questions.component.scss'
})
export class QuestionsComponent implements OnInit {
  private readonly appState = inject(AppStateService);
  private readonly examService = inject(ExamService);
  private readonly questionService = inject(QuestionService);
  private readonly router = inject(Router);

  readonly exam = signal<ExamDetail | null>(null);
  readonly selectedTopicId = signal<string>('');
  readonly questions = signal<QuestionSummary[]>([]);
  readonly currentIndex = signal(0);
  readonly selectedAnswer = signal<string>('');
  readonly lastResult = signal<AttemptResult | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly showCreateForm = signal(false);
  newStatement = '';
  newAlternatives: QuestionAlternative[] = [{ key: 'A', text: '' }, { key: 'B', text: '' }];
  newCorrectAnswer = 'A';

  get currentQuestion(): QuestionSummary | null {
    return this.questions()[this.currentIndex()] ?? null;
  }

  get currentAlternatives(): QuestionAlternative[] {
    const q = this.currentQuestion;
    if (!q?.alternativesJson) return [];
    try {
      return JSON.parse(q.alternativesJson);
    } catch {
      return [];
    }
  }

  ngOnInit(): void {
    const examId = this.appState.currentExamId();
    if (!examId) {
      this.router.navigate(['/exam-setup']);
      return;
    }

    this.examService.getExam(examId).subscribe({
      next: (exam) => {
        this.exam.set(exam);
        const firstTopic = exam.subjects.flatMap((s) => s.topics)[0];
        if (firstTopic) this.selectTopic(firstTopic.id);
      },
      error: () => this.errorMessage.set('Não foi possível carregar o concurso.')
    });
  }

  selectTopic(topicId: string): void {
    this.selectedTopicId.set(topicId);
    this.lastResult.set(null);
    this.selectedAnswer.set('');
    this.currentIndex.set(0);
    this.loadQuestions();
  }

  submitAnswer(): void {
    const q = this.currentQuestion;
    if (!q || !this.selectedAnswer()) return;

    this.questionService.attempt(q.id, this.appState.userId(), this.selectedAnswer()).subscribe({
      next: (result) => this.lastResult.set(result),
      error: () => this.errorMessage.set('Não foi possível registrar a resposta.')
    });
  }

  nextQuestion(): void {
    this.lastResult.set(null);
    this.selectedAnswer.set('');
    if (this.currentIndex() < this.questions().length - 1) {
      this.currentIndex.update((i) => i + 1);
    } else {
      this.loadQuestions();
      this.currentIndex.set(0);
    }
  }

  addAlternative(): void {
    const nextKey = String.fromCharCode(65 + this.newAlternatives.length);
    this.newAlternatives.push({ key: nextKey, text: '' });
  }

  removeAlternative(index: number): void {
    this.newAlternatives.splice(index, 1);
  }

  createQuestion(): void {
    const topicId = this.selectedTopicId();
    if (!topicId || !this.newStatement) return;

    const hasAlternatives = this.newAlternatives.some((a) => a.text.trim().length > 0);

    this.questionService
      .create({
        statement: this.newStatement,
        alternativesJson: hasAlternatives ? JSON.stringify(this.newAlternatives) : null,
        correctAnswer: this.newCorrectAnswer,
        source: 'UserCreated',
        boardId: null,
        year: null,
        createdByUserId: this.appState.userId(),
        topicIds: [topicId]
      })
      .subscribe({
        next: () => {
          this.newStatement = '';
          this.newAlternatives = [{ key: 'A', text: '' }, { key: 'B', text: '' }];
          this.newCorrectAnswer = 'A';
          this.showCreateForm.set(false);
          this.loadQuestions();
        },
        error: () => this.errorMessage.set('Não foi possível criar a questão.')
      });
  }

  private loadQuestions(): void {
    const topicId = this.selectedTopicId();
    if (!topicId) return;

    this.questionService.getByTopic(topicId, this.appState.userId()).subscribe({
      next: (questions) => this.questions.set(questions),
      error: () => this.errorMessage.set('Não foi possível carregar as questões.')
    });
  }
}
