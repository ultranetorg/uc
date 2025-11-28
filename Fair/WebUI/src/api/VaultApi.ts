import { AuthenticationResult, Wallet, WalletAccount } from "types/vault"

export type VaultApi = {
  authenticate(application: string, network: string, accountAddress: string): Promise<AuthenticationResult>
  isAuthenticated(network: string, accountAddress: string, session: string): Promise<boolean>

  getWallets(): Promise<Wallet[]>
  getWalletAccounts(name?: string): Promise<WalletAccount[]>
}
