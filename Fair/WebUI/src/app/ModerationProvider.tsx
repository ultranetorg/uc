import { createContext, PropsWithChildren, useCallback, useContext, useMemo } from "react"

import { useGetSitePolicies } from "entities"
import { ExtendedOperationType, Policy } from "types"
import { toOperationType } from "utils"

import { useSiteContext } from "./SiteProvider"
import { useUserContext } from "./UserProvider"

type ModerationContextType = {
  isPublisher: boolean
  isModerator: boolean
  policies?: Policy[]
  publishersIds?: string[]
  isOperationAllowed(operation: ExtendedOperationType): boolean
  getOperationVoterId(operation: ExtendedOperationType): string | undefined
}

const ModerationContext = createContext<ModerationContextType>({
  isPublisher: false,
  isModerator: false,
  isOperationAllowed: () => false,
  getOperationVoterId: () => undefined,
})

export const ModerationProvider = ({ children }: PropsWithChildren) => {
  const { site } = useSiteContext()
  const { user } = useUserContext()

  const isPublisher = Boolean(site?.authorsIds?.some(x => user?.authorsIds?.includes(x)))
  const isModerator = Boolean(site?.moderatorsIds?.some(x => user?.id === x))

  const { data: policies } = useGetSitePolicies(isPublisher || isModerator, site?.id)

  const isOperationAllowed = useCallback(
    (operation: ExtendedOperationType) => {
      const operationClass = toOperationType(operation)
      return (
        !!policies &&
        ((isModerator &&
          policies.some(x => x.operationClass === operationClass && x.approval !== "publishers-majority")) ||
          (isPublisher &&
            policies.some(x => x.operationClass == operationClass && x.approval === "publishers-majority")))
      )
    },
    [isModerator, isPublisher, policies],
  )

  const getOperationVoterId = useCallback(
    (operation?: ExtendedOperationType) => {
      if (!operation) return undefined
      const operationClass = toOperationType(operation)
      const policy = policies?.find(x => x.operationClass === operationClass)
      if (!policy || !user) return undefined

      return policy.approval !== "publishers-majority" && isModerator
        ? user.id
        : policy.approval === "publishers-majority" && isPublisher && user.authorsIds && user.authorsIds.length > 0
          ? user.authorsIds[0]
          : undefined
    },
    [isModerator, isPublisher, policies, user],
  )

  const value = useMemo(
    () => ({
      isPublisher,
      isModerator,
      policies,
      publishersIds: site && user ? user.authorsIds.filter(x => site.authorsIds.includes(x)) : undefined,
      isOperationAllowed,
      getOperationVoterId,
    }),
    [getOperationVoterId, isModerator, isOperationAllowed, isPublisher, policies, site, user],
  )

  return <ModerationContext.Provider value={value}>{children}</ModerationContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useModerationContext = () => useContext(ModerationContext)
