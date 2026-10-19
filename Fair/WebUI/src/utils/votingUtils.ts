import { ApprovalRequirement, OperationType, Policy, RoleName, Store } from "types"

const findPolicy = (operation?: OperationType, policies?: Policy[]) =>
  operation && policies ? policies.find(x => x.operationClass === operation) : undefined

const canCreate = (creator: RoleName, operation?: OperationType, policies?: Policy[]) =>
  findPolicy(operation, policies)?.creators.includes(creator)

export const isPublisherCanCreate = (operation?: OperationType, policies?: Policy[]) =>
  canCreate("publisher", operation, policies)

export const isModeratorCanCreate = (operation?: OperationType, policies?: Policy[]) =>
  canCreate("moderator", operation, policies)

export const isUserCanCreate = (operation?: OperationType, policies?: Policy[]) =>
  canCreate("user", operation, policies)

export const isCandidateCanCreate = (operation?: OperationType, policies?: Policy[]) =>
  canCreate("candidate", operation, policies)

export const isPublisherCanVote = (operation?: OperationType, policies?: Policy[]) => {
  const policy = findPolicy(operation, policies)
  return policy && policy.approval === "publishers-majority"
}

export const isModeratorCanVote = (operation?: OperationType, policies?: Policy[]) => {
  const policy = findPolicy(operation, policies)
  return policy && policy.approval !== "publishers-majority"
}

// Moderators-majority approval is a referendum, any other approval is a discussion.
export const isReferendum = (operation?: OperationType, policies?: Policy[]) =>
  findPolicy(operation, policies)?.approval === "moderators-majority"

export const isVotingRequired = (operation?: OperationType, store?: Store, policies?: Policy[]): boolean => {
  const policy = findPolicy(operation, policies)
  if (!store || !policy) return true

  switch (policy.approval) {
    case "any-moderator":
      return false

    case "moderators-majority":
      return store.moderatorsIds.length > 1

    case "publishers-majority":
      return store.authorsIds.length > 2

    case "all-moderators":
      return store.moderatorsIds.length > 1

    default:
      return true
  }
}

export const calculateVotesRequiredToWinPerpetualSurvey = (storePublishersCount: number) =>
  storePublishersCount / 2 + (storePublishersCount & 1)

export const calculateVotesRequiredToWinProposal = (
  operation?: OperationType,
  store?: Store,
  policies?: Policy[],
): number => {
  const policy = findPolicy(operation, policies)
  if (!store || !policy) return -1

  return calculateVotesRequiredToWinProposalByApproval(policy.approval, store)
}

const calculateVotesRequiredToWinProposalByApproval = (approval: ApprovalRequirement, store: Store): number => {
  switch (approval) {
    case "any-moderator":
      return 1

    case "moderators-majority":
      return Math.floor(store.moderatorsIds.length / 2) + (store.moderatorsIds.length & 1)

    case "all-moderators":
      return store.moderatorsIds.length

    case "publishers-majority":
      return Math.floor(store.authorsIds.length / 2) + (store.authorsIds.length & 1)

    default:
      return -1
  }
}
