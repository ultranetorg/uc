import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useLocalStorage } from "usehooks-ts"

import { LOCAL_STORAGE_KEYS } from "constants/"
import { useGetAccountByAddress } from "entities"
import { useAuthenticateMutation, useIsAuthenticatedMutation } from "entities/vault"
import { Account, AccountBase } from "types"
import { MakeOptional, showToast } from "utils"

// TODO: showToast related code with showToast function should be moved from here.

type StoredAccount = MakeOptional<AccountBase, "id">

type StoredAccountSession = {
  account: StoredAccount
  session: string
}

type AccountsStorageState = {
  accounts: StoredAccountSession[]
  selectedIndex?: number
}

type AccountsContextType = {
  accounts: StoredAccountSession[]
  currentAccount?: Account
  isPending: boolean
  refetchAccount(): void
  selectAccount(index: number): void
  authenticate(): void
  logout(index: number): void
}

const AccountsContext = createContext<AccountsContextType>({
  isPending: false,
  accounts: [],
  refetchAccount: () => {},
  selectAccount: () => {},
  authenticate: () => {},
  logout: () => {},
})

export const AccountsProvider = ({ children }: PropsWithChildren) => {
  const [session, setSession, removeSession] = useLocalStorage<AccountsStorageState>(
    LOCAL_STORAGE_KEYS.STORED_ACCOUNTS,
    {
      accounts: [],
    },
  )

  const currentAccountAddress =
    session.selectedIndex !== undefined ? session.accounts[session.selectedIndex]?.account?.address : undefined
  const [currentAccount, setCurrentAccount] = useState<Account | undefined>()

  const { authenticate: authenticateMutation, isFetching: isAuthenticatePending } = useAuthenticateMutation()
  const {
    isAuthenticated: isAuthenticatedMutation,
    isPending: isAuthenticatedPending,
    isReady: isAuthenticatedReady,
  } = useIsAuthenticatedMutation()
  const { data: account, refetch: refetchAccount } = useGetAccountByAddress(currentAccountAddress)

  useEffect(() => {
    setCurrentAccount(account)

    if (account) {
      setSession(p => ({
        ...p,
        accounts: p.accounts.map(a =>
          a.account.address !== account.address
            ? a
            : {
                ...a,
                ...{ account: { address: account.address, id: account.id, nickname: account.nickname } },
              },
        ),
      }))
    }
  }, [account, session.accounts.length, setSession])

  useEffect(() => {
    if (!isAuthenticatedReady) return
    if (session.selectedIndex === undefined) return

    if (session.accounts.length <= session.selectedIndex) {
      removeSession()
      return
    }

    const target = session.accounts[session.selectedIndex]
    isAuthenticatedMutation(
      { accountAddress: target.account.address, session: target.session },
      {
        onSuccess: valid => {
          if (!valid) {
            if (session.accounts.length > 1) {
              setSession(p => {
                const accounts = p.accounts.filter((_, i) => i !== p.selectedIndex)
                return { ...p, accounts, selectedIndex: undefined }
              })
            } else {
              removeSession()
            }

            setCurrentAccount(undefined)
          }
        },
      },
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticatedMutation, isAuthenticatedReady, removeSession, setSession])

  const authenticate = useCallback(
    () =>
      authenticateMutation(
        {},
        {
          onSuccess: data => {
            if (data.account === undefined) {
              showToast("Authentication cancelled", "warning")
              return
            }

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

              const newAccounts = [{ session: data.session, account: { address: data.account } }, ...prev.accounts]

              return {
                ...prev,
                accounts: newAccounts,
                selectedIndex: 0,
              }
            })
          },
          onError: error => showToast(error.message, "error"),
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
        { accountAddress: target.account.address, session: target.session },
        {
          onSuccess: valid => {
            if (valid) {
              setSession(p => ({ ...p, selectedIndex: index }))
            } else {
              // Remove invalid account.
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
      currentAccount,
      isPending: isAuthenticatePending || isAuthenticatedPending,
      refetchAccount,
      selectAccount,
      authenticate,
      logout,
    }),
    [
      session.accounts,
      currentAccount,
      isAuthenticatePending,
      isAuthenticatedPending,
      refetchAccount,
      selectAccount,
      authenticate,
      logout,
    ],
  )

  return <AccountsContext.Provider value={value}>{children}</AccountsContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useAccountsContext = () => useContext(AccountsContext)
