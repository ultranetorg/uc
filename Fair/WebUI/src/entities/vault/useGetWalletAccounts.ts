import { useQuery } from "@tanstack/react-query"

import { getVaultApi } from "api"

const vaultApi = getVaultApi()

export const useGetWalletAccounts = (name?: string) => {
  const queryFn = () => vaultApi.getWalletAccounts(name ?? "default")

  const { isPending, isError, data } = useQuery({
    queryKey: ["wallets", name ?? "default", "accounts"],
    queryFn: queryFn,
  })

  return { isPending, isError, data }
}
