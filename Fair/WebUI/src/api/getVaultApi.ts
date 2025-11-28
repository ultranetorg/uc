import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

import { VaultApi } from "./VaultApi"

const { VITE_APP_VAULT_API_BASE_URL: BASE_URL } = import.meta.env

const authenticate = (application: string, network: string, accountAddress: string): Promise<AuthenticationResult> =>
  fetch(`${BASE_URL}/Authenticate?Application=${application}&Net=${network}&Account=${accountAddress}`).then(res =>
    res.json(),
  )

const isAuthenticated = (network: string, accountAddress: string, session: string): Promise<boolean> =>
  fetch(`${BASE_URL}/IsAuthenticated?Account=${accountAddress}&Net=${network}&Session=${session}`).then(res =>
    res.json(),
  )

const getWallets = (): Promise<Wallet[]> => fetch(`${BASE_URL}/Wallets`).then(res => res.json())

const getWalletAccounts = (name?: string): Promise<WalletAccount[]> =>
  fetch(`${BASE_URL}/WalletAccounts` + (name ? `?Name=${name}` : "")).then(res => res.json())

const vaultApi: VaultApi = {
  authenticate,
  isAuthenticated,

  getWallets,
  getWalletAccounts,
}

export const getVaultApi = () => vaultApi
