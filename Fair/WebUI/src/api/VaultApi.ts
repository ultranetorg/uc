import { AuthenticationResult, Wallet } from "types/vault"

export type VaultApi = {
  authenticate(baseUrl: string, userName: string, address: string): Promise<AuthenticationResult | null>
  isAuthenticated(baseUrl: string, userName: string, session: string): Promise<boolean>
  register(baseUrl: string, userName: string): Promise<AuthenticationResult | null>

  getWallets(baseUrl: string): Promise<Wallet[]>
}
