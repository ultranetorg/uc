import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo } from "react"
import { useLocalStorage } from "usehooks-ts"

import { LOCAL_STORAGE_KEYS } from "constants/"
import { useAuthenticateMutation, useIsAuthenticatedMutation } from "entities/vault"
import { AuthenticationResult } from "types/vault"

type Callbacks = {
  onSuccess?: (data: AuthenticationResult | null) => void
  onError?: (error: Error) => void
  onSettled?: (data?: AuthenticationResult | null) => void
}

type StoredUser = {
  name: string
  owner: string
}

type StoredUserSession = {
  user: StoredUser
  session: string
}

type UsersStorageState = {
  users: StoredUserSession[]
  selectedUserName?: string
}

type ManageUsersContextType = {
  users: StoredUserSession[]
  selectedUserName?: string
  isPending: boolean
  selectUser(userName: string): void
  authenticate(userName: string, owner: string, callbacks?: Callbacks): void
  logout(userName: string): void
  register(userName: string, callbacks?: Callbacks): void
}

const ManageUsersContext = createContext<ManageUsersContextType>({
  isPending: false,
  users: [],
  selectUser: () => {},
  authenticate: () => {},
  logout: () => {},
  register: () => {},
})

export const ManageUsersProvider = ({ children }: PropsWithChildren) => {
  const [storage, setStorage, removeStorage] = useLocalStorage<UsersStorageState>(LOCAL_STORAGE_KEYS.STORED_USERS, {
    users: [],
  })

  const { mutate: authenticateMutation, isFetching: isAuthenticatePending } = useAuthenticateMutation()
  const {
    isAuthenticated: isAuthenticatedMutation,
    isPending: isAuthenticatedPending,
    isReady: isAuthenticatedReady,
  } = useIsAuthenticatedMutation()

  useEffect(() => {
    if (!isAuthenticatedReady) return
    if (storage.selectedUserName === undefined) return

    const user = storage.users.find(x => x.user.name === storage.selectedUserName)
    if (!user) return

    isAuthenticatedMutation(
      { userName: user.user.name, session: user.session },
      {
        onSettled: valid => {
          if (!valid) removeUser(storage.selectedUserName!)
        },
      },
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticatedMutation, isAuthenticatedReady, removeStorage, setStorage])

  const authenticate = useCallback(
    (userName: string, address: string, callbacks?: Callbacks) =>
      authenticateMutation(
        { userName, address },
        {
          onSuccess: data => {
            callbacks?.onSuccess?.(data)

            if (data === null) return

            setStorage(prev => {
              const existingIndex = prev.users.findIndex(x => x.user.name === userName)
              if (existingIndex !== -1) {
                return {
                  ...prev,
                  users: prev.users.map((acc, i) => (i === existingIndex ? { ...acc, session: data.session } : acc)),
                }
              }

              const newUsers = [...prev.users, { session: data.session, user: { owner: data.account, name: userName } }]
              return {
                ...prev,
                users: newUsers,
                selectedUserName: userName,
              }
            })
          },
          onError: error => callbacks?.onError?.(error),
          onSettled: data => callbacks?.onSettled?.(data),
        },
      ),
    [authenticateMutation, setStorage],
  )

  const removeUser = useCallback(
    (userName: string) => {
      if (storage.users.length > 1) {
        setStorage(p => {
          const users = p.users.filter(x => x.user.name !== userName)
          const selectedUserName = p.selectedUserName !== userName ? p.selectedUserName : undefined
          return { ...p, users, selectedUserName }
        })
      } else {
        removeStorage()
      }
    },
    [removeStorage, setStorage, storage.users.length],
  )

  const selectUser = useCallback(
    (userName: string) => {
      if (storage.selectedUserName === userName) return

      const user = storage.users.find(x => x.user.name === userName)
      if (!user) return

      isAuthenticatedMutation(
        { userName: user.user.name, session: user.session },
        {
          onSuccess: valid => {
            if (valid) {
              setStorage(p => ({ ...p, selectedUserName: userName }))
            }
          },
          onSettled: valid => {
            if (!valid) removeUser(userName)
          },
        },
      )
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [storage.selectedUserName, storage.users.length, isAuthenticatedMutation, setStorage, removeUser],
  )

  const logout = useCallback((userName: string) => removeUser(userName), [removeUser])

  const value = useMemo(
    () => ({
      users: storage.users,
      selectedUserName: storage.selectedUserName,
      isPending: isAuthenticatePending || isAuthenticatedPending,
      authenticate,
      logout,
      selectUser,
    }),
    [
      storage.users,
      storage.selectedUserName,
      isAuthenticatePending,
      isAuthenticatedPending,
      authenticate,
      logout,
      selectUser,
    ],
  )

  return <ManageUsersContext.Provider value={value}>{children}</ManageUsersContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useManageUsersContext = () => useContext(ManageUsersContext)
