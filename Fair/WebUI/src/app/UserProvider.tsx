import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"

import { useGetUserDetails } from "entities"
import { Account } from "types"

import { useManageUsersContext } from "./ManageUsersProvider"
import { useSiteContext } from "./SiteProvider"

type UserContextType = {
  isPublisher?: boolean
  isModerator?: boolean
  publishersIds?: string[]
  isFetching: boolean
  user?: Account
  refetch: () => void
}

const UserContext = createContext<UserContextType>({
  isFetching: false,
  refetch: () => {},
})

export const UserProvider = ({ children }: PropsWithChildren) => {
  const [currentUser, setCurrentUser] = useState<Account | undefined>()

  const { site } = useSiteContext()
  const { selectedUserName } = useManageUsersContext()

  const { isFetching, data: user, refetch } = useGetUserDetails(selectedUserName)

  useEffect(() => setCurrentUser(user), [user])

  const value = useMemo(
    () => ({
      isPublisher: Boolean(site?.authorsIds?.some(x => user?.authorsIds?.includes(x))),
      isModerator: Boolean(site?.moderatorsIds?.some(x => user?.id === x)),
      publishersIds: site && user ? user.authorsIds.filter(x => site.authorsIds.includes(x)) : undefined,
      isFetching,
      user: currentUser,
      refetch,
    }),
    [currentUser, isFetching, refetch, site, user],
  )

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserContext = () => useContext(UserContext)
