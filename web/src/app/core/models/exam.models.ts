export interface Topic {
  id: string;
  parentTopicId: string | null;
  name: string;
  estimatedIncidence: number;
}

export interface Subject {
  id: string;
  name: string;
  weight: number;
  topics: Topic[];
}

export interface ExamDetail {
  id: string;
  name: string;
  examDate: string;
  status: string;
  subjects: Subject[];
}
