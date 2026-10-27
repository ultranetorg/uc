import { useCallback } from "react"

import { useStorePoliciesContext, useStoreRolesContext } from "app"
import { isModeratorCanCreate, isPublisherCanCreate } from "utils"
import { OperationType } from "types"

export const useIsOperationVisibleForCreator = () => {
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()

  return useCallback(
    (operation: OperationType) =>
      !!(
        (isModerator && isModeratorCanCreate(operation, policies)) ||
        (isPublisher && isPublisherCanCreate(operation, policies))
      ),
    [isModerator, isPublisher, policies],
  )
}
