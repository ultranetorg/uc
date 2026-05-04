import { OperationType } from "types"

export const CREATE_PROPOSAL_HIDE_TYPE_SELECTION_OPERATION_TYPES: OperationType[] = [
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
  "publication-deletion",
  "publication-publish",
  "publication-unpublish",
  "site-authors-removal",
  "site-avatar-change",
  "site-moderator-addition",
  "site-moderator-removal",
  "site-name-change",
  "site-text-change",
]

export const CREATE_PROPOSAL_DISABLE_TYPE_SELECTION_OPERATION_TYPES: OperationType[] = [
  "category-avatar-change",
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
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
  "publication-unpublish",
  "publication-updation",
  "review-status-change",
  "user-unregistration",
] as const
