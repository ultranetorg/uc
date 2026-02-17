import { OperationType } from "types"

export const CREATE_DISCUSSION_EXTRA_OPERATION_TYPES = ["site-author-addition", "site-author-removal"] as const

export const CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES: OperationType[] = [
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-unpublish",
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
