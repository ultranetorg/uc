import { proposalsKeys } from "../proposals/proposalsKeys"

export const moderatorProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "moderators"] as const,
}
