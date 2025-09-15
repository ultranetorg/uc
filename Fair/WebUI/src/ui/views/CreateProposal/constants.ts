import { OperationType } from "types"

export const PROPOSAL_DURATIONS: string[] = ["10", "15", "30"]

export const PROPOSAL_DURATION_DEFAULT = "15"

export const OPERATION_TYPES: OperationType[] = [
  "category-avatar-change",
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-remove-from-changed",
  "publication-updation",
  "review-status-change",
  "site-authors-change",
  "site-avatar-change",
  "site-moderators-change",
  "site-nickname-change",
  "site-policy-change",
  "site-text-change",
  "user-deletion",
]

export const SINGLE_OPTION_OPERATION_TYPES: OperationType[] = [
  "category-deletion",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-remove-from-changed",
  "review-status-change",
  "user-deletion",
]
