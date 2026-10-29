import { BaseVotableOperation, OperationType, Policy, Proposal, ProposalDetails, SpecialChoice } from "types"

import { isModeratorCanVote, isPublisherCanVote } from "./votingUtils"

export const getFirstOperation = <T extends BaseVotableOperation>(
  proposal: Proposal,
  operationType: OperationType,
): T | undefined => {
  const operation = proposal.options[0].operation
  return proposal.operation === operationType ? (operation as T) : undefined
}

export const getVotedIndex = (voterId?: string, proposal?: ProposalDetails): number | undefined => {
  if (voterId && proposal) {
    const choice = proposal.options.findIndex(x => x.yes.includes(voterId))
    if (choice !== -1) return choice
    if (proposal.neither.includes(voterId)) return SpecialChoice.Neither
    if (proposal.any.includes(voterId)) return SpecialChoice.Any
    if (proposal.ban.includes(voterId)) return SpecialChoice.Ban
    if (proposal.banish.includes(voterId)) return SpecialChoice.Banish
  }

  return undefined
}

export const getLeadingVotes = (proposal: Proposal): number => Math.max(0, ...proposal.yes.map(x => x.length))

export const getLeadingOptionIndexes = (proposal: Proposal): number[] => {
  const leadingVotes = getLeadingVotes(proposal)
  if (leadingVotes === 0) return []

  return proposal.yes.flatMap((x, i) => (x.length === leadingVotes ? [i] : []))
}

export const getVotedChoice = (voterIds: string[], proposal: Proposal): number | undefined => {
  const hasVoted = (ids: string[]) => ids.some(id => voterIds.includes(id))

  const choice = proposal.yes.findIndex(hasVoted)
  if (choice !== -1) return choice
  if (hasVoted(proposal.neither)) return SpecialChoice.Neither
  if (hasVoted(proposal.any)) return SpecialChoice.Any
  if (hasVoted(proposal.ban)) return SpecialChoice.Ban
  if (hasVoted(proposal.banish)) return SpecialChoice.Banish

  return undefined
}

export const getVoterIds = (
  operation: OperationType,
  policies?: Policy[],
  moderatorId?: string,
  publisherIds?: string[],
): string[] => {
  if (isModeratorCanVote(operation, policies)) return moderatorId ? [moderatorId] : []
  if (isPublisherCanVote(operation, policies)) return publisherIds ?? []

  return []
}

export const isVoted = (voterId?: string, proposal?: ProposalDetails): boolean | undefined =>
  voterId && proposal
    ? proposal.options.some(x => x.yes.includes(voterId)) ||
      proposal.neither.includes(voterId) ||
      proposal.any.includes(voterId) ||
      proposal.ban.includes(voterId) ||
      proposal.banish.includes(voterId)
    : undefined

export const isUserVoted = (userId?: string, proposal?: Proposal): boolean =>
  userId !== undefined &&
  proposal !== undefined &&
  (proposal.neither.includes(userId) ||
    proposal.any.includes(userId) ||
    proposal.ban.includes(userId) ||
    proposal.banish.includes(userId) ||
    proposal.yes.flat().includes(userId))
