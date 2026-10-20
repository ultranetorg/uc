import { useMemo } from "react"

import { OperationType, Role } from "types"

import { useStorePoliciesContext } from "./StorePoliciesProvider"
import { useStoreRolesContext } from "./StoreRolesProvider"
import { useUserContext } from "./UserProvider"

type UseOperationVoterIdentityResult = {
  id: string
  role: Role
}

export const useOperationVoterIdentity = (operation?: OperationType): UseOperationVoterIdentityResult | undefined => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { policies } = useStorePoliciesContext()
  const { user } = useUserContext()

  return useMemo<UseOperationVoterIdentityResult | undefined>(() => {
    if (!operation) return undefined
    if (!user) return undefined

    const policy = policies?.find(x => x.operationClass === operation)
    if (!policy) return undefined

    if (
      isModerator &&
      (policy.approval === "all-moderators" ||
        policy.approval === "any-moderator" ||
        policy.approval === "moderators-majority")
    ) {
      return { id: user.id, role: Role.Moderator }
    }

    if (isPublisher && user.authorsIds && user.authorsIds.length > 0 && policy.approval === "publishers-majority")
      return { id: user.authorsIds[0], role: Role.Publisher }

    return undefined
  }, [isModerator, isPublisher, operation, policies, user])
}
