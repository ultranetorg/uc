import { createContext, PropsWithChildren, useCallback, useContext, useMemo } from "react"

import { useGetSitePolicies } from "entities"
import { OperationType, Policy, Role } from "types"

import { useSiteContext } from "./SiteProvider"
import { useUserContext } from "./UserProvider"

type ModerationContextType = {
  isPublisher: boolean
  isModerator: boolean
  policies?: Policy[]
  publishersIds?: string[]
  isOperationAllowed(operation: OperationType): boolean
  getOperationVoterId(operation?: OperationType): string | undefined
  getOperationCreatorId(operation?: OperationType): { id: string; role: Role } | undefined
  getOperationApprovalId(operation?: OperationType): { id: string; role: Role } | undefined
}

const ModerationContext = createContext<ModerationContextType>({
  isPublisher: false,
  isModerator: false,
  isOperationAllowed: () => false,
  getOperationVoterId: () => undefined,
  getOperationCreatorId: () => undefined,
  getOperationApprovalId: () => undefined,
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
      if (!policy || !user) return undefined

      return policy.approval === "publishers-majority" && isModerator
        ? policy.approval === "publishers-majority" && isPublisher && user.authorsIds && user.authorsIds.length > 0
          ? user.authorsIds[0]
          : undefined
        : user.id
    },
    [isModerator, isPublisher, policies, user],
  )

  const getOperationCreatorId = useCallback(
    (operation?: OperationType): { role: Role; id: string } | undefined => {
      if (!user) return undefined
      if (!operation) return undefined

      const policy = policies?.find(x => x.operationClass === operation)
      if (!policy) return undefined

      if (policy.creators.includes("user")) return { id: user.id, role: Role.User }
      if (isModerator && policy.creators.includes("moderator")) return { id: user.id, role: Role.Moderator }
      if (isPublisher && policy.creators.includes("publisher") && user.authorsIds && user.authorsIds.length > 0)
        return { id: user.authorsIds[0], role: Role.Publisher }

      return undefined
    },
    [isModerator, isPublisher, policies, user],
  )

  const getOperationApprovalId = useCallback(
    (operation?: OperationType): { role: Role; id: string } | undefined => {
      if (!user) return undefined
      if (!operation) return undefined

      const policy = policies?.find(x => x.operationClass === operation)
      if (!policy) return undefined

      if (
        (isModerator && policy.approval === "any-moderator") ||
        policy.approval === "moderators-majority" ||
        policy.approval === "all-moderators"
      )
        return { id: user.id, role: Role.Moderator }

      if (isPublisher && policy.approval === "publishers-majority" && user.authorsIds && user.authorsIds.length > 0)
        return { id: user.authorsIds[0], role: Role.Publisher }

      return undefined
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
      getOperationCreatorId,
      getOperationApprovalId,
    }),
    [
      getOperationApprovalId,
      getOperationCreatorId,
      getOperationVoterId,
      isModerator,
      isOperationAllowed,
      isPublisher,
      policies,
      site,
      user,
    ],
  )

  return <ModerationContext.Provider value={value}>{children}</ModerationContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useModerationContext = () => useContext(ModerationContext)
