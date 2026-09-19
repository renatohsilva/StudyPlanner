export interface WeeklyPlanItem {
  id: string;
  topicId: string;
  topicName: string;
  subjectId: string;
  subjectName: string;
  type: 'NewContent' | 'Review' | 'Practice';
  scheduledDate: string;
  durationMinutes: number;
  priorityScore: number;
}

export interface WeeklyPlan {
  planId: string | null;
  version: number;
  items: WeeklyPlanItem[];
}
