import { AuthenticationResult, Wallet } from "types/vault"

export type VaultApi = {
  authenticate(baseUrl: string, user: string, account: string): Promise<AuthenticationResult | null>
  isAuthenticated(baseUrl: string, user: string, session: string): Promise<boolean>
  register(baseUrl: string, user: string): Promise<AuthenticationResult | null>

  getWallets(baseUrl: string): Promise<Wallet[]>
}
