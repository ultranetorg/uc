import { VAULT } from "constants/"
import { AuthenticationResult, Wallet } from "types/vault"

import { VaultApi } from "./VaultApi"
import { keysToCamelCase } from "./utils"

const authenticate = async (baseUrl: string, user: string, account: string): Promise<AuthenticationResult | null> => {
  const response = await fetch(`${baseUrl}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      User: user,
      Account: account,
    }),
  })
  const data = await response.json()
  if (data === null) return null

  const res = keysToCamelCase(data) as AuthenticationResult

  // const signer = geSigner(user, account)
  // console.log("signer", signer, "res.signer", res.signer)
  if (res.signer !== account) throw new Error("You don't have permission to selected account")

  return res
}

const isAuthenticated = (baseUrl: string, user: string, session: string): Promise<boolean> =>
  fetch(`${baseUrl}/IsAuthenticated`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      User: user,
      Session: session,
    }),
  }).then(res => res.json())

const register = async (baseUrl: string, user: string): Promise<AuthenticationResult | null> => {
  const response = await fetch(`${baseUrl}/Authenticate`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Net: VAULT.NETWORK,
      User: user,
    }),
  })
  const data = await response.json()
  if (data === null) return null

  return keysToCamelCase(data) as AuthenticationResult
}

const getWallets = (baseUrl: string): Promise<Wallet[]> => fetch(`${baseUrl}/Wallets`).then(res => res.json())

const api: VaultApi = {
  authenticate,
  isAuthenticated,
  register,
  getWallets,
}

export const getVaultApi = () => api
