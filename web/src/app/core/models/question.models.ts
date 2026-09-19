export interface QuestionAlternative {
  key: string;
  text: string;
}

export interface QuestionSummary {
  id: string;
  statement: string;
  alternativesJson: string | null;
  source: 'Official' | 'AiGenerated' | 'UserCreated';
  attemptsCount: number;
}

export interface AttemptResult {
  attemptId: string;
  isCorrect: boolean;
  correctAnswer: string;
}
