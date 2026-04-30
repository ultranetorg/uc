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

type GroupedOperations = {
  items: OperationType[]
}

export const groupOperations = (operations: OperationType[]): GroupedOperations[] => {
  const map = new Map<string, OperationType[]>()

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
