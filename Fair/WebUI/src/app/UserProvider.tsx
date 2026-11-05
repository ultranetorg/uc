import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetUserDetails } from "entities"
import { UserDetails } from "types"

import { useAuthenticationContext } from "./AuthenticationProvider"

type UserContextType = {
  isLoading: boolean
  user?: UserDetails
  refetch: () => void
}

const UserContext = createContext<UserContextType>({
  isLoading: false,
  refetch: () => {},
})

export const UserProvider = ({ children }: PropsWithChildren) => {
  const { selectedUserName } = useAuthenticationContext()

  const { isLoading, data: user, refetch } = useGetUserDetails(selectedUserName)

  const value = useMemo(() => ({ isLoading, user, refetch }), [user, isLoading, refetch])

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserContext = () => useContext(UserContext)
