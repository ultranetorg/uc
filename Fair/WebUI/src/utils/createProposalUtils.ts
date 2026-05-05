import { CREATE_PROPOSAL_DISABLE_TYPE_SELECTION_OPERATION_TYPES } from "constants/"
import { OperationType, Policy, ProposalType } from "types"

const getProposalOperations = (proposalType: ProposalType, policies: Policy[]): OperationType[] => {
  return proposalType === "discussion"
    ? policies.filter(x => x.approval !== "publishers-majority").map(x => x.operationClass)
    : policies.filter(x => x.approval === "publishers-majority").map(x => x.operationClass)
}

export const getVisibleProposalOperations = (
  proposalType: ProposalType,
  policies: Policy[] | undefined = [],
): OperationType[] => {
  const allOperations = getProposalOperations(proposalType, policies)
  return allOperations.filter(
    x => !(CREATE_PROPOSAL_DISABLE_TYPE_SELECTION_OPERATION_TYPES as OperationType[]).includes(x),
  )
}

const operationTypeToFairOperationTypeMap: Record<OperationType, string> = {
  "category-avatar-change": "CategoryAvatarChange",
  "category-creation": "CategoryCreation",
  "category-deletion": "CategoryDeletion",
  "category-movement": "CategoryMovement",
  "category-type-change": "CategoryTypeChange",
  "publication-creation": "PublicationCreation",
  "publication-deletion": "PublicationDeletion",
  "publication-publish": "PublicationPublish",
  "publication-updation": "PublicationUpdation",
  "publication-unpublish": "PublicationUnpublish",
  "review-creation": "ReviewCreation",
  "review-edit": "ReviewEdit",
  "review-status-change": "ReviewStatusChange",
  "site-authors-removal": "SiteAuthorsRemoval",
  "site-avatar-change": "SiteAvatarChange",
  "site-moderator-addition": "SiteModeratorAddition",
  "site-moderator-removal": "SiteModeratorRemoval",
  "site-name-change": "SiteNameChange",
  "site-text-change": "SiteTextChange",
  "user-registration": "UserRegistration",
  "user-unregistration": "UserUnregistration",
}

export const getFairOperationType = (type: OperationType): string | undefined =>
  operationTypeToFairOperationTypeMap[type]
