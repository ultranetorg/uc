import { proposalsKeys } from "./proposalsKeys"

export const proposalCommentsKeys = {
  all: (storeId: string, proposalId: string) => [...proposalsKeys.detail(storeId, proposalId), "comments"] as const,
}
