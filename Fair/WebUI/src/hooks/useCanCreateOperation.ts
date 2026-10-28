import { useCallback } from "react"

import { useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { isModeratorCanCreate, isPublisherCanCreate, isUserCanCreate } from "utils"
import { OperationType } from "types"

export const useCanCreateOperation = () => {
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { user } = useUserContext()

  return useCallback(
    (operation: OperationType) =>
      !!(
        (isModerator && isModeratorCanCreate(operation, policies)) ||
        (isPublisher && isPublisherCanCreate(operation, policies)) ||
        (!isModerator && !isPublisher && user && isUserCanCreate(operation, policies))
      ),
    [isModerator, isPublisher, policies, user],
  )
}
