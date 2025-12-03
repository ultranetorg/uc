import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

export type VaultApi = {
  authenticate(accountAddress?: string, logo?: string): Promise<AuthenticationResult>
  isAuthenticated(accountAddress: string, session: string): Promise<boolean>
  authorize(operation: string, accountAddress: string, session: string, hash: string): Promise<void>

  getWallets(): Promise<Wallet[]>
  getWalletAccounts(name?: string): Promise<WalletAccount[]>
}
