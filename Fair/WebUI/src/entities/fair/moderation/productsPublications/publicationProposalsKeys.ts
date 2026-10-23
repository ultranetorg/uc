import { proposalsKeys } from "../proposals/proposalsKeys"

export const publicationProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "publications"] as const,
}
