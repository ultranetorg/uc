import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo } from "react"
import { useLocalStorage } from "usehooks-ts"

import { LOCAL_STORAGE_KEYS } from "constants/"
import { useAuthenticate, useIsAuthenticated } from "entities/vault"
import { useGetAccountByAddress } from "entities"
import { AccountBase } from "types"
import { MakeOptional } from "utils"

type Account = MakeOptional<AccountBase, "id">

interface SessionAccount {
  account: Account
  session: string
}

interface StoredAccounts {
  accounts: SessionAccount[]
  selectedIndex?: number
}

interface AccountsContextType {
  accounts: SessionAccount[]
  currentAccount?: Account
  isPending: boolean
  selectAccount(index: number): void
  authenticate(): void
  logout(index: number): void
}

const AccountsContext = createContext<AccountsContextType>({
  isPending: false,
  accounts: [],
  selectAccount: () => {},
  authenticate: () => {},
  logout: () => {},
})

export const AccountsProvider = ({ children }: PropsWithChildren) => {
  const [session, setSession, removeSession] = useLocalStorage<StoredAccounts>(LOCAL_STORAGE_KEYS.STORED_ACCOUNTS, {
    accounts: [],
  })

  const currentAccount =
    session.selectedIndex !== undefined && session.accounts[session.selectedIndex]
      ? session.accounts[session.selectedIndex].account
      : undefined

  const { authenticate: authenticateMutation, isPending: isAuthenticatePending } = useAuthenticate()
  const { isAuthenticated, isPending: isAuthenticatedPending } = useIsAuthenticated()
  const { data: account } = useGetAccountByAddress(currentAccount?.address)

  useEffect(() => {
    if (!account) return

    const found = session.accounts.find(a => a.account.address === account.address)
    if (!found || found.account.id) return

    setSession(p => ({
      ...p,
      accounts: p.accounts.map(a =>
        a.account.address === account.address
          ? {
              ...a,
              account: {
                ...a.account,
                id: account.id,
                nickname: account.nickname,
              },
            }
          : a,
      ),
    }))
  }, [account, session.accounts.length, setSession])

  const selectAccount = useCallback(
    (index: number) => {
      if (index < 0 || index >= session.accounts.length) return
      if (session.selectedIndex === index) return

      const target = session.accounts[index]

      isAuthenticated(
        { accountAddress: target.account.address, session: target.session },
        {
          onSuccess: valid => {
            if (valid) {
              setSession(p => ({ ...p, selectedIndex: index }))
            } else {
              // remove invalid account
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
    [isAuthenticated, session.accounts.length, session.selectedIndex, setSession],
  )

  const authenticate = useCallback(
    () =>
      authenticateMutation(
        {},
        {
          onSuccess: data =>
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
            }),
        },
      ),
    [authenticateMutation, setSession],
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

  useEffect(() => {
    if (session.selectedIndex === undefined) return
    if (session.accounts.length <= session.selectedIndex) {
      removeSession()
      return
    }

    const target = session.accounts[session.selectedIndex]
    isAuthenticated(
      { accountAddress: target.account.address, session: target.session },
      {
        onSuccess: valid => {
          if (!valid) {
            setSession(p => {
              const accounts = p.accounts.filter((_, i) => i !== p.selectedIndex)
              return { ...p, accounts, selectedIndex: undefined }
            })
          }
        },
      },
    )
  }, [])

  const value = useMemo(
    () => ({
      accounts: session.accounts,
      currentAccount,
      isPending: isAuthenticatePending || isAuthenticatedPending,
      selectAccount,
      authenticate,
      logout,
    }),
    [
      session.accounts,
      currentAccount,
      isAuthenticatePending,
      isAuthenticatedPending,
      selectAccount,
      authenticate,
      logout,
    ],
  )

  return <AccountsContext.Provider value={value}>{children}</AccountsContext.Provider>
}

export const useAccountsContext = () => useContext(AccountsContext)
