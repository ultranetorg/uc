import { BaseVotableOperation, OperationType, Proposal, ProposalDetails, SpecialChoice } from "types"

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

export const isVoted = (voterId?: string, proposal?: ProposalDetails): boolean | undefined =>
  voterId && proposal
    ? proposal.options.some(x => x.yes.includes(voterId)) ||
      proposal.neither.includes(voterId) ||
      proposal.any.includes(voterId) ||
      proposal.ban.includes(voterId) ||
      proposal.banish.includes(voterId)
    : undefined
