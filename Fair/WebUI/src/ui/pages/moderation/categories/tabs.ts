import { OperationType } from "types"

/** Tab keys of the moderation categories page (also route segments) for the category operations proposals. */
export const CATEGORY_PROPOSALS_TAB_KEYS: Partial<Record<OperationType, string>> = {
  "category-creation": "creations",
  "category-deletion": "deletions",
  "category-movement": "movements",
  "category-type-change": "type-changes",
  "category-avatar-change": "avatar-changes",
}
