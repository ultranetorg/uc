import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetUserStoreExists } from "entities"

import { useStoreContext } from "./StoreProvider"
import { useUserContext } from "./UserProvider"

type StoreRolesContextType = {
  isPublisher: boolean
  isModerator: boolean
  isJoined: boolean
  publisherIds?: string[]
}

const StoreRolesContext = createContext<StoreRolesContextType>({
  isPublisher: false,
  isModerator: false,
  isJoined: false,
})

export const StoreRolesProvider = ({ children }: PropsWithChildren) => {
  const { store } = useStoreContext()
  const { user } = useUserContext()

  const isPublisher = Boolean(store?.authorsIds?.some(x => user?.authorsIds?.includes(x)))
  const isModerator = Boolean(store?.moderatorsIds?.some(x => user?.id === x))

  const { data: isJoined } = useGetUserStoreExists(user?.id, store?.id)

  const publisherIds = useMemo(
    () => (store && user ? user.authorsIds.filter(x => store.authorsIds.includes(x)) : undefined),
    [store, user],
  )

  const value = useMemo<StoreRolesContextType>(
    () => ({
      isPublisher,
      isModerator,
      isJoined: isJoined ?? false,
      publisherIds,
    }),
    [isPublisher, isModerator, isJoined, publisherIds],
  )

  return <StoreRolesContext.Provider value={value}>{children}</StoreRolesContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useStoreRolesContext = () => useContext(StoreRolesContext)
