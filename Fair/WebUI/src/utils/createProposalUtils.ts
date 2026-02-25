import { CREATE_DISCUSSION_EXTRA_OPERATION_TYPES, CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES } from "constants/"
import { ExtendedOperationType, OperationType, ProposalType } from "types"

export const getProposalOperations = (
  proposalType: ProposalType,
  discussions?: OperationType[],
  referendums?: OperationType[],
): ExtendedOperationType[] => {
  if (discussions === undefined || referendums === undefined) return []

  const operations = proposalType === "discussion" ? discussions : referendums
  const index = operations.indexOf("site-authors-change")
  if (index !== -1) {
    return [
      ...operations.slice(0, index),
      ...CREATE_DISCUSSION_EXTRA_OPERATION_TYPES,
      ...operations.slice(index + 1),
    ] as ExtendedOperationType[]
  }

  return operations as ExtendedOperationType[]
}

export const getVisibleProposalOperations = (
  proposalType: ProposalType,
  discussions?: OperationType[],
  referendums?: OperationType[],
): ExtendedOperationType[] => {
  const allOperations = getProposalOperations(proposalType, discussions, referendums)
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
