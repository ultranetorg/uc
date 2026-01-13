import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"

import { useGetUserDetails } from "entities"
import { Account } from "types"

import { useManageUsersContext } from "./ManageUsersProvider"

type UserContextType = {
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

  const { selectedUserName } = useManageUsersContext()

  const { isFetching, data: user, refetch } = useGetUserDetails(selectedUserName)

  useEffect(() => setCurrentUser(user), [user])

  const value = useMemo(() => ({ isFetching, user: currentUser, refetch }), [currentUser, isFetching, refetch])

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserContext = () => useContext(UserContext)
