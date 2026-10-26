import { moderationKeys } from "../moderationKeys"

export const referendumsKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "referendums"] as const,
}
