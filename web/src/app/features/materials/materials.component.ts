import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppStateService } from '../../core/services/app-state.service';
import { MaterialService } from '../../core/services/material.service';
import { MaterialStatus, MaterialTopicLink } from '../../core/models/material.models';

@Component({
  selector: 'app-materials',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
  readonly importingYouTube = signal(false);
  readonly importingLink = signal(false);
  readonly errorMessage = signal<string | null>(null);

  youTubeUrl = '';
  linkUrl = '';

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

  importYouTube(): void {
    if (!this.youTubeUrl) return;

    this.importingYouTube.set(true);
    this.errorMessage.set(null);

    this.materialService.importYouTube(this.examId, this.youTubeUrl).subscribe({
      next: () => {
        this.importingYouTube.set(false);
        this.youTubeUrl = '';
        this.loadMaterials();
      },
      error: (err) => {
        this.importingYouTube.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Não foi possível importar o vídeo.');
      }
    });
  }

  importLink(): void {
    if (!this.linkUrl) return;

    this.importingLink.set(true);
    this.errorMessage.set(null);

    this.materialService.importLink(this.examId, this.linkUrl).subscribe({
      next: () => {
        this.importingLink.set(false);
        this.linkUrl = '';
        this.loadMaterials();
      },
      error: (err) => {
        this.importingLink.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Não foi possível importar o link.');
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
