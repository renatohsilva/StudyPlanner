import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { AvailabilityService } from '../../core/services/availability.service';
import { WeeklyPlanService } from '../../core/services/weekly-plan.service';
import { AvailabilitySlot, DayOfWeekName } from '../../core/models/availability.models';
import { WeeklyPlan, WeeklyPlanItem } from '../../core/models/weekly-plan.models';

const DAY_ORDER: DayOfWeekName[] = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
const DAY_LABELS: Record<DayOfWeekName, string> = {
  Monday: 'Segunda',
  Tuesday: 'Terça',
  Wednesday: 'Quarta',
  Thursday: 'Quinta',
  Friday: 'Sexta',
  Saturday: 'Sábado',
  Sunday: 'Domingo'
};

@Component({
  selector: 'app-weekly-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './weekly-plan.component.html',
  styleUrl: './weekly-plan.component.scss'
})
export class WeeklyPlanComponent implements OnInit {
  private readonly appState = inject(AppStateService);
  private readonly availabilityService = inject(AvailabilityService);
  private readonly weeklyPlanService = inject(WeeklyPlanService);
  private readonly router = inject(Router);

  private examId!: string;

  readonly days = DAY_ORDER;
  readonly dayLabels = DAY_LABELS;
  readonly hours: Record<DayOfWeekName, number> = {
    Monday: 0, Tuesday: 0, Wednesday: 0, Thursday: 0, Friday: 0, Saturday: 0, Sunday: 0
  };

  readonly plan = signal<WeeklyPlan | null>(null);
  readonly saving = signal(false);
  readonly generating = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly savedMessage = signal<string | null>(null);

  ngOnInit(): void {
    const examId = this.appState.currentExamId();
    if (!examId) {
      this.router.navigate(['/exam-setup']);
      return;
    }
    this.examId = examId;

    this.loadAvailability();
    this.loadPlan();
  }

  saveAvailability(): void {
    const slots: AvailabilitySlot[] = this.days
      .filter((d) => this.hours[d] > 0)
      .map((d) => ({ dayOfWeek: d, hoursAvailable: this.hours[d] }));

    this.saving.set(true);
    this.errorMessage.set(null);
    this.savedMessage.set(null);

    this.availabilityService.set(this.examId, this.appState.userId(), slots).subscribe({
      next: () => {
        this.saving.set(false);
        this.savedMessage.set('Disponibilidade salva.');
      },
      error: () => {
        this.saving.set(false);
        this.errorMessage.set('Não foi possível salvar a disponibilidade.');
      }
    });
  }

  generatePlan(): void {
    this.generating.set(true);
    this.errorMessage.set(null);

    const weekStart = this.mondayOfCurrentWeek();
    this.weeklyPlanService.generate(this.examId, this.appState.userId(), weekStart).subscribe({
      next: () => {
        this.generating.set(false);
        this.loadPlan();
      },
      error: (err) => {
        this.generating.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Não foi possível gerar o plano semanal.');
      }
    });
  }

  itemsForDay(date: string): WeeklyPlanItem[] {
    return (this.plan()?.items ?? []).filter((i) => i.scheduledDate === date);
  }

  weekDates(): string[] {
    const monday = new Date(this.mondayOfCurrentWeek() + 'T00:00:00');
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + i);
      return d.toISOString().slice(0, 10);
    });
  }

  dayLabelForDate(date: string): string {
    const d = new Date(date + 'T00:00:00');
    return DAY_LABELS[DAY_ORDER[(d.getDay() + 6) % 7]];
  }

  private mondayOfCurrentWeek(): string {
    const today = new Date();
    const day = today.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    const monday = new Date(today);
    monday.setDate(today.getDate() + diff);
    return monday.toISOString().slice(0, 10);
  }

  private loadAvailability(): void {
    this.availabilityService.get(this.examId, this.appState.userId()).subscribe({
      next: (slots) => {
        for (const slot of slots) {
          this.hours[slot.dayOfWeek] = slot.hoursAvailable;
        }
      }
    });
  }

  private loadPlan(): void {
    this.weeklyPlanService.getActive(this.examId, this.appState.userId()).subscribe({
      next: (plan) => this.plan.set(plan)
    });
  }
}
