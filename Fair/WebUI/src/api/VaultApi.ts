import { AccountBase } from "types"

export type VaultApi = {
  getAccounts(): Promise<AccountBase>
}
