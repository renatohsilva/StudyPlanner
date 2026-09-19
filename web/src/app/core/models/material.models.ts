export type MaterialStatusValue = 'Uploaded' | 'Processing' | 'Processed' | 'Failed';

export interface MaterialStatus {
  id: string;
  examId: string;
  name: string;
  status: MaterialStatusValue;
  errorMessage: string | null;
}

export interface MaterialTopicLink {
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  relevanceScore: number;
}
