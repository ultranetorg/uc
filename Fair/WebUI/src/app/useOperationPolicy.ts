import { useMemo } from "react"

import { OperationCreator, OperationType, OperationVoter, Policy, Role } from "types"

import { useStorePoliciesContext } from "./StorePoliciesProvider"
import { useStoreRolesContext } from "./StoreRolesProvider"
import { useUserContext } from "./UserProvider"

type OperationPolicyResult = {
  policy?: Policy
  voterId?: string
  creator?: OperationCreator
  voter?: OperationVoter
}

export const useOperationPolicy = (operation?: OperationType): OperationPolicyResult => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { policies } = useStorePoliciesContext()
  const { user } = useUserContext()

  return useMemo<OperationPolicyResult>(() => {
    if (!operation) return {}

    const policy = policies?.find(x => x.operationClass === operation)
    if (!policy) return {}

    if (!user) return { policy }

    const voterId =
      policy.approval === "publishers-majority" && isModerator
        ? policy.approval === "publishers-majority" && isPublisher && user.authorsIds && user.authorsIds.length > 0
          ? user.authorsIds[0]
          : undefined
        : user.id

    let creator: OperationCreator | undefined
    if (policy.creators.includes("user")) creator = { id: user.id, role: Role.User }
    else if (isModerator && policy.creators.includes("moderator")) creator = { id: user.id, role: Role.Moderator }
    else if (isPublisher && policy.creators.includes("publisher") && user.authorsIds && user.authorsIds.length > 0)
      creator = { id: user.authorsIds[0], role: Role.Publisher }

    let voter: OperationVoter | undefined
    if (
      (isModerator && policy.approval === "any-moderator") ||
      policy.approval === "moderators-majority" ||
      policy.approval === "all-moderators"
    )
      voter = { id: user.id, role: Role.Moderator }
    else if (isPublisher && policy.approval === "publishers-majority" && user.authorsIds && user.authorsIds.length > 0)
      voter = { id: user.authorsIds[0], role: Role.Publisher }

    return { policy, voterId, creator, voter }
  }, [operation, policies, isModerator, isPublisher, user])
}
