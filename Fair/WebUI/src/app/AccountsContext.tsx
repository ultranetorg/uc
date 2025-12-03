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
  logout(): void
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
    session.selectedIndex !== undefined && session.accounts.length > session.selectedIndex
      ? session.accounts[session.selectedIndex].account
      : undefined

  const { authenticate: authenticateMutation, isPending: isAuthenticatePending } = useAuthenticate()
  const { isAuthenticated, isPending: isAuthenticatedPending } = useIsAuthenticated()
  const { data: account } = useGetAccountByAddress(currentAccount?.address)

  useEffect(() => {
    if (account === undefined) return

    const index = session.accounts.find(x => x.account.address === account?.address)
    if (index?.account.id) {
      return
    }

    setSession(prev => {
      const accounts = prev.accounts.map(item =>
        item.account.address === account!.address
          ? {
              ...item,
              account: {
                ...item.account,
                id: account!.id,
                nickname: account!.nickname,
              },
            }
          : item,
      )

      return {
        ...prev,
        accounts,
      }
    })
  }, [account, session.accounts, setSession])

  const selectAccount = useCallback(
    (index: number) => {
      if (session.accounts.length <= index || session.selectedIndex === index) return

      const sessionAccount = session.accounts[index]
      isAuthenticated(
        { accountAddress: sessionAccount.account.address, session: sessionAccount.session },
        {
          onSuccess: response => {
            if (response) {
              setSession(p => ({ ...p, selectedIndex: index }))
            } else {
              setSession(p => ({
                accounts: p.accounts.filter((_, i) => i !== index),
                selectedIndex: p.selectedIndex && p.selectedIndex > index ? p.selectedIndex - 1 : p.selectedIndex,
              }))
            }
          },
        },
      )
    },
    [isAuthenticated, session.accounts, session.selectedIndex, setSession],
  )

  const authenticate = useCallback(
    () =>
      authenticateMutation(
        {},
        {
          onSuccess: data =>
            setSession(p => {
              // RU: Проверяем, выбран ли уже аккаунт, который находится в списке accounts. В этом случае делаем его выбранным (currentAccountIndex) и обновляем session.
              // EN: Checking if an account that is in the accounts list has already been selected. In this case, we set it as the selected one (currentAccountIndex) and update the session.
              const index = p.accounts.findIndex(x => x.account.address === data.account)
              if (index !== -1) {
                const accounts = p.accounts.map((x, i) => (i !== index ? x : { ...x, session: data.session }))
                return { selectedIndex: index, accounts }
              }

              const accounts = [{ session: data.session, account: { address: data.account } }, ...p.accounts]
              return {
                selectedIndex: 0,
                accounts,
              }
            }),
        },
      ),
    [authenticateMutation, setSession],
  )

  const logout = useCallback(() => removeSession(), [removeSession])

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
