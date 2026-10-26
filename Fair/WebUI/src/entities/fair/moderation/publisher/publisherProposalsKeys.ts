import { proposalsKeys } from "../proposals/proposalsKeys"

export const publisherProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "publishers"] as const,
}
