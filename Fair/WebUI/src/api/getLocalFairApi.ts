import { LocalFairApi } from "./LocalFairApi"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getNexusUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/nexus`).then(res => res.json())

const getVaultUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/vault`).then(res => res.json())

const api: LocalFairApi = {
  getNexusUrl,
  getVaultUrl,
}

export const getLocalFairApi = () => api
