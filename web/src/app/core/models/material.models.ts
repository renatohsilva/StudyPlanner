export type MaterialStatusValue = 'Uploaded' | 'Processing' | 'Processed' | 'Failed';
export type MaterialSourceType = 'File' | 'YouTube' | 'Link';

export interface MaterialStatus {
  id: string;
  examId: string;
  name: string;
  status: MaterialStatusValue;
  sourceType: MaterialSourceType;
  sourceUrl: string | null;
  errorMessage: string | null;
}

export interface MaterialTopicLink {
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  relevanceScore: number;
}
