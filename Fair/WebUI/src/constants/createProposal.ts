import { OperationType } from "types"

export const CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES: OperationType[] = [
  "category-deletion",
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-unpublish",
  "publication-updation",
  "review-status-change",
  "user-unregistration",
] as const
