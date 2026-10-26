import { useMemo } from "react"

import { OperationType, Role } from "types"
import { isProposal, isReferendum } from "utils"

import { useStorePoliciesContext } from "./StorePoliciesProvider"
import { useStoreRolesContext } from "./StoreRolesProvider"
import { useUserContext } from "./UserProvider"

interface ApprovalIdentity {
  id: string
  role: Role
}

export const useApprovalIdentityForOperation = (operation?: OperationType): ApprovalIdentity | undefined => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const policies = useStorePoliciesContext()
  const { user } = useUserContext()

  return useMemo<ApprovalIdentity | undefined>(() => {
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
