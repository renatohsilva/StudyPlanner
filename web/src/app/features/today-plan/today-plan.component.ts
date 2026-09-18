import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { StudyPlanService } from '../../core/services/study-plan.service';
import { StudySessionService } from '../../core/services/study-session.service';
import { ReviewService } from '../../core/services/review.service';
import { TodayPlan } from '../../core/models/study-plan.models';
import { PendingReview, WeakPoint } from '../../core/models/review.models';

@Component({
  selector: 'app-today-plan',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './today-plan.component.html',
  styleUrl: './today-plan.component.scss'
})
export class TodayPlanComponent implements OnInit {
  private readonly appState = inject(AppStateService);
  private readonly studyPlanService = inject(StudyPlanService);
  private readonly studySessionService = inject(StudySessionService);
  private readonly reviewService = inject(ReviewService);
  private readonly router = inject(Router);

  readonly plan = signal<TodayPlan | null>(null);
  readonly pendingReviews = signal<PendingReview[]>([]);
  readonly weakPoints = signal<WeakPoint[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly loggedTopicIds = signal<Set<string>>(new Set());

  ngOnInit(): void {
    const examId = this.appState.currentExamId();
    if (!examId) {
      this.router.navigate(['/exam-setup']);
      return;
    }

    this.loadPlan(examId);
    this.loadReviews(examId);
    this.loadWeakPoints(examId);
  }

  logSession(topicId: string): void {
    this.studySessionService
      .create({
        userId: this.appState.userId(),
        topicId,
        studyPlanItemId: null,
        durationMinutes: 30
      })
      .subscribe(() => {
        this.loggedTopicIds.update((set) => new Set(set).add(topicId));
      });
  }

  masteryPercent(mastery: number): number {
    return Math.round(mastery * 100);
  }

  private loadPlan(examId: string): void {
    this.loading.set(true);
    this.studyPlanService.getToday(this.appState.userId(), examId).subscribe({
      next: (plan) => {
        this.plan.set(plan);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar o plano de hoje.');
        this.loading.set(false);
      }
    });
  }

  private loadReviews(examId: string): void {
    this.reviewService.getPending(this.appState.userId(), examId).subscribe({
      next: (reviews) => this.pendingReviews.set(reviews)
    });
  }

  private loadWeakPoints(examId: string): void {
    this.reviewService.getWeakPoints(examId, this.appState.userId()).subscribe({
      next: (weakPoints) => this.weakPoints.set(weakPoints)
    });
  }
}
