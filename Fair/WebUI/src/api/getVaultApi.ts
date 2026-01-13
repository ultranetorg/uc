import { VAULT } from "constants/"
import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

import { VaultApi } from "./VaultApi"
import { keysToCamelCase } from "./utils"

const authenticate = async (
  baseUrl: string,
  userName: string,
  address: string,
): Promise<AuthenticationResult | null> => {
  const response = await fetch(`${baseUrl}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      User: userName,
      Account: address,
    }),
  })
  const data = await response.json()
  if (data === null) return null

  const res = keysToCamelCase(data) as AuthenticationResult
  if (res.account !== address) throw new Error("You don't have permission to selected account")
  return res
}

const isAuthenticated = (baseUrl: string, userName: string, session: string): Promise<boolean> =>
  fetch(`${baseUrl}/IsAuthenticated`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      User: userName,
      Session: session,
    }),
  }).then(res => res.json())

const getWallets = (baseUrl: string): Promise<Wallet[]> => fetch(`${baseUrl}/Wallets`).then(res => res.json())

const getWalletAccounts = (baseUrl: string, name?: string): Promise<WalletAccount[]> =>
  fetch(`${baseUrl}/WalletAccounts`, { body: JSON.stringify({ Name: name }) }).then(res => res.json())

const api: VaultApi = {
  authenticate,
  isAuthenticated,
  getWallets,
  getWalletAccounts,
}

export const getVaultApi = () => api
