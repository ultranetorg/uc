import { proposalsKeys } from "../proposals/proposalsKeys"

export const publicationProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "publications"] as const,
  creations: (storeId: string) => [...publicationProposalsKeys.all(storeId), "creations"] as const,
  deletions: (storeId: string) => [...publicationProposalsKeys.all(storeId), "deletions"] as const,
  publishes: (storeId: string) => [...publicationProposalsKeys.all(storeId), "publishes"] as const,
  unpublishes: (storeId: string) => [...publicationProposalsKeys.all(storeId), "unpublishes"] as const,
  updations: (storeId: string) => [...publicationProposalsKeys.all(storeId), "updations"] as const,
}
