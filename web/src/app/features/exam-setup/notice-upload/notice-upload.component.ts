import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NoticeService } from '../../../core/services/notice.service';
import { ExtractedExamStructure, NoticeStatusDto } from '../../../core/models/notice.models';

@Component({
  selector: 'app-notice-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notice-upload.component.html',
  styleUrl: './notice-upload.component.scss'
})
export class NoticeUploadComponent {
  private readonly noticeService = inject(NoticeService);

  @Input({ required: true }) examId!: string;
  @Output() confirmed = new EventEmitter<void>();

  readonly selectedFile = signal<File | null>(null);
  readonly uploading = signal(false);
  readonly confirming = signal(false);
  readonly notice = signal<NoticeStatusDto | null>(null);
  readonly structure = signal<ExtractedExamStructure | null>(null);
  readonly errorMessage = signal<string | null>(null);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.uploading.set(true);
    this.errorMessage.set(null);
    this.notice.set(null);
    this.structure.set(null);

    this.noticeService.upload(this.examId, file).subscribe({
      next: (notice) => {
        this.notice.set(notice);
        this.uploading.set(false);

        if (notice.status === 'ExtractionReady' && notice.extractedStructureJson) {
          this.structure.set(JSON.parse(notice.extractedStructureJson));
        } else if (notice.status === 'Failed') {
          this.errorMessage.set(notice.errorMessage ?? 'Falha ao processar o edital.');
        }
      },
      error: () => {
        this.uploading.set(false);
        this.errorMessage.set('Não foi possível enviar o edital.');
      }
    });
  }

  addSubject(): void {
    this.structure.update((s) => ({
      subjects: [...(s?.subjects ?? []), { name: '', weight: 0.1, topics: [] }]
    }));
  }

  removeSubject(index: number): void {
    this.structure.update((s) => ({
      subjects: (s?.subjects ?? []).filter((_, i) => i !== index)
    }));
  }

  addTopic(subjectIndex: number): void {
    this.structure.update((s) => {
      if (!s) return s;
      const subjects = s.subjects.map((subject, i) =>
        i === subjectIndex
          ? { ...subject, topics: [...subject.topics, { name: '', estimatedIncidence: 0.1 }] }
          : subject
      );
      return { subjects };
    });
  }

  removeTopic(subjectIndex: number, topicIndex: number): void {
    this.structure.update((s) => {
      if (!s) return s;
      const subjects = s.subjects.map((subject, i) =>
        i === subjectIndex ? { ...subject, topics: subject.topics.filter((_, j) => j !== topicIndex) } : subject
      );
      return { subjects };
    });
  }

  confirmStructure(): void {
    const notice = this.notice();
    const structure = this.structure();
    if (!notice || !structure) return;

    this.confirming.set(true);
    this.errorMessage.set(null);

    this.noticeService.confirm(notice.id, structure).subscribe({
      next: () => {
        this.confirming.set(false);
        this.confirmed.emit();
      },
      error: () => {
        this.confirming.set(false);
        this.errorMessage.set('Não foi possível confirmar a estrutura.');
      }
    });
  }
}
