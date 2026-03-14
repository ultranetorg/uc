import { BaseVotableOperation, OperationType, Proposal } from "types"

export const getFirstOperation = <T extends BaseVotableOperation>(
  proposal: Proposal,
  operationType: OperationType,
): T | undefined => {
  const operation = proposal.options[0].operation
  return proposal.operation === operationType ? (operation as T) : undefined
}
