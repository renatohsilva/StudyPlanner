export interface SubjectMetrics {
  subjectId: string;
  subjectName: string;
  weight: number;
  averageMastery: number;
  topicsWithAttempts: number;
  totalTopics: number;
  questionsAnswered: number;
  accuracyOverall: number;
}

export interface ExamMetrics {
  coverage: number;
  totalTopics: number;
  topicsStudied: number;
  averageMastery: number;
  topicsWithAttempts: number;
  studyHoursTotal: number;
  studyHoursLast7Days: number;
  questionsAnswered: number;
  accuracyOverall: number;
  revisionRate: number;
  subjects: SubjectMetrics[];
}
