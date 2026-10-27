import { useCallback } from "react"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { OperationType } from "types"
import {
  isModeratorCanCreate,
  isModeratorCanVote,
  isPublisherCanCreate,
  isPublisherCanVote,
  isVotingRequired,
} from "utils"

export const useIsOperationVisibleForStaff = () => {
  const policies = useStorePoliciesContext()
  const { store } = useStoreContext()
  const { isPublisher, isModerator } = useStoreRolesContext()

  return useCallback(
    (operation: OperationType) => {
      const isModeratorShouldVote =
        isModeratorCanVote(operation, policies) && isVotingRequired(operation, store, policies)
      const isPublisherShouldVote =
        isPublisherCanVote(operation, policies) && isVotingRequired(operation, store, policies)

      return (
        (isModerator &&
          (isModeratorShouldVote ||
            isPublisherCanCreate(operation, policies) ||
            isModeratorCanCreate(operation, policies))) ||
        (isPublisher && (isPublisherShouldVote || isPublisherCanCreate(operation, policies)))
      )
    },
    [isModerator, isPublisher, policies, store],
  )
}
