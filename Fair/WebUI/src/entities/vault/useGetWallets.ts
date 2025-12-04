import { useQuery } from "@tanstack/react-query"

import { getVaultApi } from "api"

const vaultApi = getVaultApi()

export const useGetWallets = () => {
  const queryFn = () => vaultApi.getWallets()

  const { isPending, isError, data } = useQuery({
    queryKey: ["wallets"],
    queryFn: queryFn,
  })

  return { isPending, isError, data }
}
