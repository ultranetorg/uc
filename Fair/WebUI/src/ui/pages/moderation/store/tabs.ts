import { OperationType } from "types"

/** Tab keys of the moderation store page (also route segments) for the store operations proposals. */
export const STORE_PROPOSALS_TAB_KEYS: Partial<Record<OperationType, string>> = {
  "store-renaming": "renamings",
  "store-info-updation": "info-updations",
  "store-avatar-change": "avatar-changes",
}
