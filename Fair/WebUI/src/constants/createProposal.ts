import { ExtendedOperationType, OperationType } from "types"

export const CREATE_DISCUSSION_EXTRA_OPERATION_TYPES = [
  "site-author-addition",
  "site-author-removal",
] as ExtendedOperationType[]

export const CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES: OperationType[] = [
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-unpublish",
  "publication-updation",
  "review-creation",
  "review-edit",
  "review-status-change",
  "user-registration",
  "user-unregistration",
]

export const CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES: OperationType[] = [
  "category-deletion",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "review-status-change",
  "user-unregistration",
] as const
