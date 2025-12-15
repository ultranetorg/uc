import { useQuery } from "@tanstack/react-query"

import { getVaultApi } from "api"

const vaultApi = getVaultApi()

export const useGetWallets = (baseUrl?: string) => {
  const queryFn = () => vaultApi.getWallets(baseUrl!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["wallets"],
    queryFn: queryFn,
    enabled: !!baseUrl,
  })

  return { isPending, isError, data }
}
