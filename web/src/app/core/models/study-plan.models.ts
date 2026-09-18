export interface TodayPlanItem {
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  priorityScore: number;
  mastery: number;
  attemptsCount: number;
  suggestedType: 'NewContent' | 'Review' | 'Practice';
}

export interface TodayPlan {
  examId: string;
  today: string;
  items: TodayPlanItem[];
}
