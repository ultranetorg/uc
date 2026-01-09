import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo } from "react"
import { useLocalStorage } from "usehooks-ts"

import { LOCAL_STORAGE_KEYS } from "constants/"
import { useAuthenticateMutation, useIsAuthenticatedMutation } from "entities/vault"
import { AuthenticationResult } from "types/vault"

type AuthenticateMutationCallbacks = {
  onSuccess?: (data: AuthenticationResult | null) => void
  onError?: (error: Error) => void
  onSettled?: (data?: AuthenticationResult | null) => void
}

type StoredAccount = {
  userName: string
  address: string
}

type StoredAccountSession = {
  account: StoredAccount
  session: string
}

type AccountsStorageState = {
  accounts: StoredAccountSession[]
  selectedIndex?: number
}

type ManageUsersContextType = {
  accounts: StoredAccountSession[]
  currentUserName?: string
  isPending: boolean
  selectAccount(index: number): void
  authenticateMutation(userName: string, address: string, callbacks?: AuthenticateMutationCallbacks): void
  logout(index: number): void
}

const ManageUsersContext = createContext<ManageUsersContextType>({
  isPending: false,
  accounts: [],
  selectAccount: () => {},
  authenticateMutation: () => {},
  logout: () => {},
})

export const ManageUsersProvider = ({ children }: PropsWithChildren) => {
  const [session, setSession, removeSession] = useLocalStorage<AccountsStorageState>(
    LOCAL_STORAGE_KEYS.STORED_ACCOUNTS,
    {
      accounts: [],
    },
  )

  const currentUserName =
    session.selectedIndex !== undefined ? session.accounts[session.selectedIndex]?.account?.userName : undefined

  const { authenticate: authenticateMutation, isFetching: isAuthenticatePending } = useAuthenticateMutation()
  const {
    isAuthenticated: isAuthenticatedMutation,
    isPending: isAuthenticatedPending,
    isReady: isAuthenticatedReady,
  } = useIsAuthenticatedMutation()

  useEffect(() => {
    if (!isAuthenticatedReady) return
    if (session.selectedIndex === undefined) return

    if (session.accounts.length <= session.selectedIndex) {
      removeSession()
      return
    }

    const target = session.accounts[session.selectedIndex]
    isAuthenticatedMutation(
      { userName: target.account.userName, session: target.session },
      {
        onSettled: valid => {
          if (!valid) {
            if (session.accounts.length > 1) {
              setSession(p => {
                const accounts = p.accounts.filter((_, i) => i !== p.selectedIndex)
                return { ...p, accounts, selectedIndex: undefined }
              })
            } else {
              removeSession()
            }
          }
        },
      },
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticatedMutation, isAuthenticatedReady, removeSession, setSession])

  const authenticate = useCallback(
    (userName: string, address: string, callbacks?: AuthenticateMutationCallbacks) =>
      authenticateMutation(
        { userName, address },
        {
          onSuccess: data => {
            callbacks?.onSuccess?.(data)

            if (data === null) return

            setSession(prev => {
              const existingIndex = prev.accounts.findIndex(x => x.account.address === data.account)

              if (existingIndex !== -1) {
                return {
                  ...prev,
                  accounts: prev.accounts.map((acc, i) =>
                    i === existingIndex ? { ...acc, session: data.session } : acc,
                  ),
                  selectedIndex: existingIndex,
                }
              }

              const newAccounts = [
                ...prev.accounts,
                { session: data.session, account: { address: data.account, userName } },
              ]

              return {
                ...prev,
                accounts: newAccounts,
                selectedIndex: 0,
              }
            })
          },
          onError: error => callbacks?.onError?.(error),
          onSettled: data => callbacks?.onSettled?.(data),
        },
      ),
    [authenticateMutation, setSession],
  )

  const selectAccount = useCallback(
    (index: number) => {
      if (index < 0 || index >= session.accounts.length) return
      if (session.selectedIndex === index) return

      const target = session.accounts[index]

      isAuthenticatedMutation(
        { userName: target.account.userName, session: target.session },
        {
          onSuccess: valid => {
            if (valid) {
              setSession(p => ({ ...p, selectedIndex: index }))
            }
          },
          onSettled: valid => {
            if (!valid) {
              if (session.accounts.length > 1) {
                setSession(p => {
                  const accounts = p.accounts.filter((_, i) => i !== p.selectedIndex)
                  return { ...p, accounts, selectedIndex: undefined }
                })
              } else {
                removeSession()
              }
            }
          },
        },
      )
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [isAuthenticatedMutation, session.accounts.length, session.selectedIndex, setSession],
  )

  const logout = useCallback(
    (index: number) => {
      if (index < 0 || index >= session.accounts.length) return
      if (session.accounts.length === 1) removeSession()

      setSession(p => {
        const accounts = p.accounts.filter((_, i) => i !== index)
        const newSelected =
          p.selectedIndex !== undefined
            ? p.selectedIndex > index
              ? p.selectedIndex - 1
              : p.selectedIndex === index
                ? undefined
                : p.selectedIndex
            : undefined

        return { ...p, accounts, selectedIndex: newSelected }
      })
    },
    [removeSession, session.accounts.length, setSession],
  )

  const value = useMemo(
    () => ({
      accounts: session.accounts,
      currentUserName,
      isPending: isAuthenticatePending || isAuthenticatedPending,
      authenticateMutation: authenticate,
      logout,
      selectAccount,
    }),
    [
      session.accounts,
      currentUserName,
      isAuthenticatePending,
      isAuthenticatedPending,
      authenticate,
      logout,
      selectAccount,
    ],
  )

  return <ManageUsersContext.Provider value={value}>{children}</ManageUsersContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useManageUsersContext = () => useContext(ManageUsersContext)
