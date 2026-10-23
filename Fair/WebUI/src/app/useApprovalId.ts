import { useMemo } from "react"

import { useStoreRolesContext } from "./StoreRolesProvider"
import { useUserContext } from "./UserProvider"

export const useApprovalId = (): string | undefined => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { user } = useUserContext()

  return useMemo<string | undefined>(() => {
    if (!user) return undefined

    if (isModerator) return user?.id
    if (isPublisher && user.authorsIds && user.authorsIds.length > 0) return user.authorsIds[0]

    return undefined
  }, [isModerator, isPublisher, user])
}
