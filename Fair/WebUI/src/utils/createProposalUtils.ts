import { CREATE_DISCUSSION_EXTRA_OPERATION_TYPES, CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES } from "constants/"
import { ExtendedOperationType, OperationType, Policy, ProposalType } from "types"

const getProposalOperations = (proposalType: ProposalType, policies: Policy[]): ExtendedOperationType[] => {
  const operations =
    proposalType === "discussion"
      ? policies.filter(x => x.approval !== "publishers-majority").map(x => x.operationClass)
      : policies.filter(x => x.approval === "publishers-majority").map(x => x.operationClass)
  return toExtendedOperationTypes(operations)
}

export const toOperationTypes = (operations: ExtendedOperationType[]): OperationType[] => {
  const hasExtra = operations.some(x => CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(x))
  const filtered = operations.filter(x => !CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(x))
  return (hasExtra ? [...filtered, "site-authors-change"] : filtered) as OperationType[]
}

export const toExtendedOperationTypes = (operations: OperationType[]): ExtendedOperationType[] =>
  operations.flatMap(x => (x === "site-authors-change" ? CREATE_DISCUSSION_EXTRA_OPERATION_TYPES : x))

export const toOperationType = (operation: ExtendedOperationType): OperationType =>
  CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(operation) ? "site-authors-change" : (operation as OperationType)

export const getVisibleProposalOperations = (
  proposalType: ProposalType,
  policies: Policy[] | undefined = [],
): ExtendedOperationType[] => {
  const allOperations = getProposalOperations(proposalType, policies)
  return allOperations.filter(x => !(CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES as ExtendedOperationType[]).includes(x))
}

type GroupedOperations = {
  items: ExtendedOperationType[]
}

export const groupOperations = (operations: ExtendedOperationType[]): GroupedOperations[] => {
  const map = new Map<string, ExtendedOperationType[]>()

  for (const op of operations) {
    const parts = op.split("-")

    const groupKey = parts[0]

    if (!map.has(groupKey)) {
      map.set(groupKey, [])
    }

    map.get(groupKey)!.push(op)
  }

  return Array.from(map.entries())
    .sort(([a], [b]) => {
      const aFirst = a.split("-")[0]
      const bFirst = b.split("-")[0]
      return aFirst.localeCompare(bFirst)
    })
    .map(([, items]) => ({
      items: items.sort(),
    }))
}

const operationTypeToFairOperationTypeMap: Record<ExtendedOperationType, string> = {
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
  "site-author-addition": "SiteAuthorsChange",
  "site-author-removal": "SiteAuthorsChange",
  "site-avatar-change": "SiteAvatarChange",
  "site-moderator-addition": "SiteModeratorAddition",
  "site-moderator-removal": "SiteModeratorRemoval",
  "site-nickname-change": "SiteNicknameChange",
  "site-text-change": "SiteTextChange",
  "user-registration": "UserRegistration",
  "user-unregistration": "UserUnregistration",
}

export const getFairOperationType = (type: ExtendedOperationType): string | undefined =>
  operationTypeToFairOperationTypeMap[type]
