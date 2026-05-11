import { OperationType, Policy, Site } from "types"

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

export const isVotingRequired = (operation?: OperationType, site?: Site, policies?: Policy[]): boolean => {
  if (!operation || !site || !policies) return true

  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return true

  switch (policy.approval) {
    case "any-moderator":
      return false

    case "moderators-majority":
      return site.authorsIds.length > 2

    case "publishers-majority":
      return site.authorsIds.length > 2

    case "all-moderators":
      return site.moderatorsIds.length > 1

    default:
      return true
  }
}
