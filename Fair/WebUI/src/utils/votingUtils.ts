import { ApprovalRequirement, OperationType, Policy, Store } from "types"

export const isPublisherVoting = (operation?: OperationType, policies?: Policy[]) => {
  if (!operation || !policies) return undefined

  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return undefined

  return policy.approval === "publishers-majority"
}

export const isModeratorVoting = (operation?: OperationType, policies?: Policy[]) => {
  if (!operation || !policies) return undefined

  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return undefined

  return policy.approval !== "publishers-majority"
}

export const isVotingRequired = (operation?: OperationType, store?: Store, policies?: Policy[]): boolean => {
  if (!operation || !store || !policies) return true

  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return true

  switch (policy.approval) {
    case "any-moderator":
      return false

    case "moderators-majority":
      return store.authorsIds.length > 2

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
  if (!operation || !store || !policies) return -1
  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return -1

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
