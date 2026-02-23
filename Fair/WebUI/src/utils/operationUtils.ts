import { CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES } from "constants/"
import { OperationType, ProposalType } from "types"

type GroupedOperations = {
  items: OperationType[]
}

export const getVisibleOperations = (
  proposalType: ProposalType,
  discussions?: OperationType[],
  referendums?: OperationType[],
): OperationType[] => {
  const operations = proposalType === "discussion" ? discussions : referendums
  return operations !== undefined ? operations.filter(x => !CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES.includes(x)) : []
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
  "site-authors-change": "SiteAuthorsChange",
  "site-avatar-change": "SiteAvatarChange",
  "site-moderator-addition": "SiteModeratorAddition",
  "site-moderator-removal": "SiteModeratorRemoval",
  "site-nickname-change": "SiteNicknameChange",
  "site-text-change": "SiteTextChange",
  "user-deletion": "UserDeletion",
  "user-registration": "UserRegistration",
}

export const getFairOperationType = (type: OperationType): string | undefined =>
  operationTypeToFairOperationTypeMap[type]
