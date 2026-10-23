import { moderationKeys } from "../moderationKeys"

export const proposalsKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "proposals"] as const,
  detail: (storeId: string, proposalId: string) => [...proposalsKeys.all(storeId), proposalId] as const,
}
