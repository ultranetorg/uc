import { ApprovalRequirement, OperationType, Policy, Site } from "types"

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

export const calculateVotesRequiredToWinPerpetualSurvey = (sitePublishersCount: number) =>
  sitePublishersCount / 2 + (sitePublishersCount & 1)

export const calculateVotesRequiredToWinProposal = (
  operation?: OperationType,
  site?: Site,
  policies?: Policy[],
): number => {
  if (!operation || !site || !policies) return -1
  const policy = policies.find(x => x.operationClass === operation)
  if (!policy) return -1

  return calculateVotesRequiredToWinProposalByApproval(policy.approval, site)
}

const calculateVotesRequiredToWinProposalByApproval = (approval: ApprovalRequirement, site: Site): number => {
  switch (approval) {
    case "any-moderator":
      return 1

    case "moderators-majority":
      return Math.floor(site.moderatorsIds.length / 2) + (site.moderatorsIds.length & 1)

    case "all-moderators":
      return site.moderatorsIds.length

    case "publishers-majority":
      return Math.floor(site.authorsIds.length / 2) + (site.authorsIds.length & 1)

    default:
      return -1
  }
}
