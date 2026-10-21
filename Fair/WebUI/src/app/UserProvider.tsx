import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetUserDetails } from "entities"
import { UserDetails } from "types"

import { useAuthenticationContext } from "./AuthenticationProvider"

type UserContextType = {
  isFetching: boolean
  user?: UserDetails
  refetch: () => void
}

const UserContext = createContext<UserContextType>({
  isFetching: false,
  refetch: () => {},
})

export const UserProvider = ({ children }: PropsWithChildren) => {
  const { selectedUserName } = useAuthenticationContext()

  const { isFetching, data: user, refetch } = useGetUserDetails(selectedUserName)

  const value = useMemo(() => ({ isFetching, user, refetch }), [user, isFetching, refetch])

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserContext = () => useContext(UserContext)
