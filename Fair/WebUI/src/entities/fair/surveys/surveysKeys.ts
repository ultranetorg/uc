export const surveysKeys = {
  all: (storeId: string) => ["stores", storeId, "surveys"] as const,

  policy: (storeId: string) => [...surveysKeys.all(storeId), "policies"] as const,
  policyDetails: (storeId: string, surveyId: string) => [...surveysKeys.all(storeId), surveyId] as const,

  moderators: (storeId: string) => [...surveysKeys.all(storeId), "moderators"] as const,
}
