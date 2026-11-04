import { OperationType } from "types"

/**
 * Moves the tabs of the operations the user cannot vote for to the end and marks them to be aligned to the right.
 * The rest of the tabs stay in place, the relative order is kept.
 */
export const sortTabsByVoting = <T extends { operation?: OperationType }>(
  items: T[],
  canVote: (operation: OperationType) => boolean,
): (T & { alignRight?: boolean })[] => {
  const isDisabled = (item: T) => !!item.operation && !canVote(item.operation)

  return [...items.filter(x => !isDisabled(x)), ...items.filter(isDisabled).map(x => ({ ...x, alignRight: true }))]
}
