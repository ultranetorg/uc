import { proposalsKeys } from "../proposals/proposalsKeys"

export const storeProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "store"] as const,

  renamings: (storeId: string) => [...storeProposalsKeys.all(storeId), "renamings"] as const,
  infoUpdations: (storeId: string) => [...storeProposalsKeys.all(storeId), "info-updations"] as const,
  avatarChanges: (storeId: string) => [...storeProposalsKeys.all(storeId), "avatar-changes"] as const,
}
