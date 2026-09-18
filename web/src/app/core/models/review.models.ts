export interface PendingReview {
  reviewId: string;
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  scheduledDate: string;
  daysOverdue: number;
  step: number;
}

export interface WeakPoint {
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  mastery: number;
  attemptsCount: number;
  weakPointScore: number;
}
