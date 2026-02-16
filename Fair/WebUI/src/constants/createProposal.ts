import { OperationType } from "types"

export const CREATE_REFERENDUM_OPERATION_TYPES: OperationType[] = [
  "site-avatar-change",
  "site-moderator-addition",
  "site-moderator-removal",
  "site-nickname-change",
  "site-text-change",
]

export const CREATE_DISCUSSION_EXTRA_OPERATION_TYPES = ["site-author-addition", "site-author-removal"] as const

export const CREATE_DISCUSSION_OPERATION_TYPES: (
  | Exclude<OperationType, "site-authors-change">
  | "site-author-addition"
  | "site-author-removal"
)[] = [
  "category-avatar-change",
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
  ...CREATE_DISCUSSION_EXTRA_OPERATION_TYPES,
] as const

export const CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES: OperationType[] = [
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-updation",
  "review-status-change",
  "user-deletion",
]

export const CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES: OperationType[] = [
  "category-deletion",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "review-status-change",
  "user-deletion",
] as const
