import { VAULT } from "constants/"
import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

import { VaultApi } from "./VaultApi"
import { keysToCamelCase } from "./utils"

const authenticate = async (baseUrl: string, accountAddress?: string): Promise<AuthenticationResult | null> => {
  const response = await fetch(`${baseUrl}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      Logo: VAULT.LOGO,
      ...(accountAddress ? { Account: accountAddress } : {}),
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as AuthenticationResult
}

const isAuthenticated = (baseUrl: string, accountAddress: string, session: string): Promise<boolean> =>
  fetch(`${baseUrl}/IsAuthenticated`, {
    method: "POST",
    body: JSON.stringify({
      Account: accountAddress,
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
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
