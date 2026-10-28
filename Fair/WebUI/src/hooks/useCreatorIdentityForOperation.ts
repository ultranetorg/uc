import { useMemo } from "react"

import { useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { OperationIdentity, OperationType, Role } from "types"
import { isModeratorCanCreate, isPublisherCanCreate, isUserCanCreate } from "utils"

export const useCreatorIdentityForOperation = (operation?: OperationType): OperationIdentity | undefined => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const policies = useStorePoliciesContext()
  const { user } = useUserContext()

  return useMemo<OperationIdentity | undefined>(() => {
    if (!operation) return undefined
    if (!user) return undefined

    const policy = policies?.find(x => x.operationClass === operation)
    if (!policy) return undefined

    if (isModerator && isModeratorCanCreate(operation, policies)) return { id: user.id, role: Role.Moderator }
    if (isPublisher && isPublisherCanCreate(operation, policies) && user.authorsIds && user.authorsIds.length > 0)
      return { id: user.authorsIds[0], role: Role.Publisher }
    if (!isModerator && !isPublisher && user && isUserCanCreate(operation, policies))
      return { id: user.id, role: Role.User }

    return undefined
  }, [operation, policies, isModerator, isPublisher, user])
}
