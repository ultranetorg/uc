export const surveysKeys = {
  all: (storeId: string) => ["stores", storeId, "perpetual-surveys"] as const,

  details: (storeId: string, surveyId: string) => [...surveysKeys.all(storeId), surveyId] as const,
}
