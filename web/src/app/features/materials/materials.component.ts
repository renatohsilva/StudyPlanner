import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { MaterialService } from '../../core/services/material.service';
import { MaterialStatus, MaterialTopicLink } from '../../core/models/material.models';

@Component({
  selector: 'app-materials',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './materials.component.html',
  styleUrl: './materials.component.scss'
})
export class MaterialsComponent implements OnInit {
  private readonly appState = inject(AppStateService);
  private readonly materialService = inject(MaterialService);
  private readonly router = inject(Router);

  private examId!: string;

  readonly materials = signal<MaterialStatus[]>([]);
  readonly topicsByMaterial = signal<Record<string, MaterialTopicLink[] | undefined>>({});
  readonly selectedFile = signal<File | null>(null);
  readonly uploading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const examId = this.appState.currentExamId();
    if (!examId) {
      this.router.navigate(['/exam-setup']);
      return;
    }
    this.examId = examId;
    this.loadMaterials();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.uploading.set(true);
    this.errorMessage.set(null);

    this.materialService.upload(this.examId, this.appState.userId(), file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.selectedFile.set(null);
        this.loadMaterials();
      },
      error: () => {
        this.uploading.set(false);
        this.errorMessage.set('Não foi possível enviar o material.');
      }
    });
  }

  relevancePercent(score: number): number {
    return Math.round(score * 100);
  }

  private loadMaterials(): void {
    this.materialService.getByExam(this.examId).subscribe({
      next: (materials) => {
        this.materials.set(materials);
        materials
          .filter((m) => m.status === 'Processed')
          .forEach((m) => this.loadTopics(m.id));
      }
    });
  }

  private loadTopics(materialId: string): void {
    this.materialService.getTopics(materialId).subscribe({
      next: (topics) => {
        this.topicsByMaterial.update((map) => ({ ...map, [materialId]: topics }));
      }
    });
  }
}
