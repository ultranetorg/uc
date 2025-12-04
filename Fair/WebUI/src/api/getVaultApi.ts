import { VAULT } from "constants/"
import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

import { CryptographyType } from "types/vault/CryptographyType"
import { VaultApi } from "./VaultApi"
import { toLowerCamel } from "./utils"

const { VITE_APP_VAULT_API_BASE_URL: BASE_URL } = import.meta.env

const authenticate = async (accountAddress?: string): Promise<AuthenticationResult> => {
  const response = await fetch(`${BASE_URL}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      Logo: VAULT.LOGO,
      ...(accountAddress ? { Account: accountAddress } : {}),
    }),
  })
  const data = await response.json()
  const r = toLowerCamel(data) as AuthenticationResult
  console.log(r)
  return r
}

const isAuthenticated = (accountAddress: string, session: string): Promise<boolean> =>
  fetch(`${BASE_URL}/IsAuthenticated`, {
    method: "POST",
    body: JSON.stringify({
      Account: accountAddress,
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      Session: session,
    }),
  }).then(res => res.json())

const authorize = (operation: string, accountAddress: string, session: string, hash: string): Promise<void> =>
  fetch(`${BASE_URL}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Cryptography: CryptographyType.Mcv,
      Net: VAULT.NETWORK,
      Application: VAULT.APPLICATION,
      Operation: operation,
      Account: accountAddress,
      Session: session,
      Hash: hash,
    }),
  }).then(res => res.json())

const getWallets = (): Promise<Wallet[]> => fetch(`${BASE_URL}/Wallets`).then(res => res.json())

const getWalletAccounts = (name?: string): Promise<WalletAccount[]> =>
  fetch(`${BASE_URL}/WalletAccounts`, { body: JSON.stringify({ Name: name }) }).then(res => res.json())

const vaultApi: VaultApi = {
  authenticate,
  isAuthenticated,
  authorize,
  getWallets,
  getWalletAccounts,
}

export const getVaultApi = () => vaultApi
