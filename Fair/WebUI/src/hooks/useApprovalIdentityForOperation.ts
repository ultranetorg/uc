import { useMemo } from "react"

import { useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { OperationIdentity, OperationType, Role } from "types"
import { isProposal, isReferendum } from "utils"

export const useApprovalIdentityForOperation = (operation?: OperationType): OperationIdentity | undefined => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const policies = useStorePoliciesContext()
  const { user } = useUserContext()

  return useMemo<OperationIdentity | undefined>(() => {
    if (!operation) return undefined
    if (!user) return undefined

    const policy = policies?.find(x => x.operationClass === operation)
    if (!policy) return undefined

    if (isProposal(operation, policies) && isModerator) {
      return { id: user.id, role: Role.Moderator }
    }
    if (isReferendum(operation, policies) && isPublisher && user.authorsIds && user.authorsIds.length > 0) {
      return { id: user.authorsIds[0], role: Role.Publisher }
    }

    return undefined
  }, [isModerator, isPublisher, operation, policies, user])
}
