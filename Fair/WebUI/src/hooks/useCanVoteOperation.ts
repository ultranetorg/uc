import { useCallback } from "react"

import { useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { OperationType } from "types"
import { isProposal, isReferendum } from "utils"

/**
 * Moderator votes for proposals, publisher (author) votes for referendums.
 * @returns Function that tells whether the current user can vote for the operation proposals.
 */
export const useCanVoteOperation = () => {
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { user } = useUserContext()

  return useCallback(
    (operation: OperationType) => {
      if (!user || !policies?.some(x => x.operationClass === operation)) return false

      return (
        (isProposal(operation, policies) && isModerator) ||
        (isReferendum(operation, policies) && isPublisher && !!user.authorsIds && user.authorsIds.length > 0)
      )
    },
    [isModerator, isPublisher, policies, user],
  )
}
