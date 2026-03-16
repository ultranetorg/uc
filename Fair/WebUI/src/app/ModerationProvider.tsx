import { createContext, PropsWithChildren, useCallback, useContext, useMemo } from "react"

import { useGetSitePolicies } from "entities"
import { OperationType, Policy } from "types"

import { useSiteContext } from "./SiteProvider"
import { useUserContext } from "./UserProvider"

type ModerationContextType = {
  isPublisher: boolean
  isModerator: boolean
  policies?: Policy[]
  publishersIds?: string[]
  isOperationAllowed(operation: OperationType): boolean
  getOperationVoterId(operation?: OperationType): string | undefined
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
    (operation: OperationType) => {
      return (
        !!policies &&
        ((isModerator && policies.some(x => x.operationClass === operation && x.approval !== "publishers-majority")) ||
          (isPublisher && policies.some(x => x.operationClass == operation && x.approval === "publishers-majority")))
      )
    },
    [isModerator, isPublisher, policies],
  )

  const getOperationVoterId = useCallback(
    (operation?: OperationType) => {
      if (!operation) return undefined
      const policy = policies?.find(x => x.operationClass === operation)
      console.log(policies, policy, operation)
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
