import { proposalsKeys } from "../proposals/proposalsKeys"

export const categoryProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "categories"] as const,

  creations: (storeId: string) => [...categoryProposalsKeys.all(storeId), "creations"] as const,
  deletions: (storeId: string) => [...categoryProposalsKeys.all(storeId), "deletions"] as const,
  movements: (storeId: string) => [...categoryProposalsKeys.all(storeId), "movements"] as const,
  typeChanges: (storeId: string) => [...categoryProposalsKeys.all(storeId), "type-changes"] as const,
  avatarChanges: (storeId: string) => [...categoryProposalsKeys.all(storeId), "avatar-changes"] as const,
}
