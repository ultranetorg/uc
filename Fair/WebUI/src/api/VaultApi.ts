import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

export type VaultApi = {
  authenticate(baseUrl: string, accountAddress?: string, logo?: string): Promise<AuthenticationResult | null>
  isAuthenticated(baseUrl: string, accountAddress: string, session: string): Promise<boolean>

  getWallets(baseUrl: string): Promise<Wallet[]>
  getWalletAccounts(baseUrl: string, name?: string): Promise<WalletAccount[]>
}
