export const perpetualSurveysKeys = {
  all: (storeId: string) => ["stores", storeId, "perpetual-surveys"] as const,

  details: (storeId: string, perpetualSurveyId: string) =>
    [...perpetualSurveysKeys.all(storeId), perpetualSurveyId] as const,
}
