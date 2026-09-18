export type NoticeStatusValue = 'Uploaded' | 'Processing' | 'ExtractionReady' | 'Failed' | 'Confirmed';

export interface NoticeStatusDto {
  id: string;
  examId: string;
  status: NoticeStatusValue;
  extractedStructureJson: string | null;
  errorMessage: string | null;
}

export interface ExtractedTopic {
  name: string;
  estimatedIncidence: number;
}

export interface ExtractedSubject {
  name: string;
  weight: number;
  topics: ExtractedTopic[];
}

export interface ExtractedExamStructure {
  subjects: ExtractedSubject[];
}
