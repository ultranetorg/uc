import { OperationType } from "types"

/**
 * Moves the tabs of the operations the user can vote for to the beginning, the relative order is kept.
 */
export const sortTabsByVoting = <T extends { operation?: OperationType }>(
  items: T[],
  canVote: (operation: OperationType) => boolean,
): T[] => {
  const isVoting = (item: T) => !!item.operation && canVote(item.operation)

  return [...items.filter(isVoting), ...items.filter(x => !isVoting(x))]
}
