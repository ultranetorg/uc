import { proposalsKeys } from "../proposals/proposalsKeys"

export const userProposalsKeys = {
  all: (storeId: string) => [...proposalsKeys.all(storeId), "user"] as const,

  registrations: (storeId: string) => [...userProposalsKeys.all(storeId), "registrations"] as const,
  unregistrations: (storeId: string) => [...userProposalsKeys.all(storeId), "unregistrations"] as const,
}
