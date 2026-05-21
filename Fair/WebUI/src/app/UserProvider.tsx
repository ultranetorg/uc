import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"
import { useParams } from "react-router-dom"

import { useGetUserDetails, useGetUserSiteExists } from "entities"
import { UserDetails } from "types"

import { useAuthenticationContext } from "./AuthenticationProvider"

type UserContextType = {
  isFetching: boolean
  isJoined: boolean
  user?: UserDetails
  refetch: () => void
}

const UserContext = createContext<UserContextType>({
  isFetching: false,
  isJoined: false,
  refetch: () => {},
})

export const UserProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const [currentUser, setCurrentUser] = useState<UserDetails | undefined>()

  const { selectedUserName } = useAuthenticationContext()

  const { isFetching, data: user, refetch } = useGetUserDetails(selectedUserName)
  const { data: isJoined } = useGetUserSiteExists(currentUser?.id, siteId)

  useEffect(() => setCurrentUser(user), [user])

  const value = useMemo(
    () => ({
      isFetching,
      user: currentUser,
      isJoined: isJoined ?? false,
      refetch,
    }),
    [currentUser, isFetching, isJoined, refetch],
  )

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserContext = () => useContext(UserContext)
