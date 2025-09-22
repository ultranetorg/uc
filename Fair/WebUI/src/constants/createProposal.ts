import { OperationType } from "types"

export const CREATE_PROPOSAL_DURATIONS: string[] = ["10", "15", "30"]

export const CREATE_PROPOSAL_DURATION_DEFAULT = CREATE_PROPOSAL_DURATIONS[1]

export const CREATE_PROPOSAL_OPERATION_TYPES: OperationType[] = [
  "category-avatar-change",
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
  "site-authors-change",
  "site-avatar-change",
  "site-moderators-change",
  "site-nickname-change",
  "site-policy-change",
  "site-text-change",
  "user-deletion",
] as const

export const CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES: OperationType[] = [
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-remove-from-changed",
  "publication-updation",
  "review-status-change",
]

export const CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES: OperationType[] = [
  "category-deletion",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-remove-from-changed",
  "review-status-change",
  "user-deletion",
] as const
