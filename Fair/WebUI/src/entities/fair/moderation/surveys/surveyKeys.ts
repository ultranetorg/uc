import { moderationKeys } from "../moderationKeys"

export const surveyKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "surveys"] as const,

  policies: (storeId: string) => [...surveyKeys.all(storeId), "policies"] as const,
  policyDetails: (storeId: string, surveyId: string) => [...surveyKeys.policies(storeId), surveyId] as const,

  moderators: (storeId: string) => [...surveyKeys.all(storeId), "moderators"] as const,
}
