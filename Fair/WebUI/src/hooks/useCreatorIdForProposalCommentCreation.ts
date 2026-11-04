import { useMemo } from "react"

import { useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { OperationType } from "types"
import { isReferendum } from "utils"

export const useCreatorIdForProposalCommentCreation = (operation?: OperationType): string | undefined => {
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { user } = useUserContext()

  return useMemo<string | undefined>(() => {
    if (!policies || !user) return undefined

    if (isModerator) {
      return user.id
    }

    if (isReferendum(operation, policies) && isPublisher && user.authorsIds && user.authorsIds.length > 0) {
      return user.authorsIds[0]
    }

    return undefined
  }, [isModerator, isPublisher, operation, policies, user])
}
