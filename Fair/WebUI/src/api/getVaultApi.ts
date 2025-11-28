import { Wallet, WalletAccount } from "types/vault"

import { VaultApi } from "./VaultApi"

const { VITE_APP_VAULT_API_BASE_URL: BASE_URL } = import.meta.env

const getWallets = (): Promise<Wallet> => fetch(`${BASE_URL}/Wallets`).then(res => res.json())

const getWalletAccounts = (name: string): Promise<WalletAccount> =>
  fetch(`${BASE_URL}/WalletAccounts?name=${name}`).then(res => res.json())

const vaultApi: VaultApi = {
  getWallets,
  getWalletAccounts,
}

export const getVaultApi = () => vaultApi
