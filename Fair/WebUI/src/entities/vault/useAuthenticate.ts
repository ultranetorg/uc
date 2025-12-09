import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"
import { useGetVaultUrl } from "entities/node"

const vaultApi = getVaultApi()

type AuthenticateMutationArgs = {
  accountAddress?: string
}

export const useAuthenticate = () => {
  const { isLoading: isUrlLoading, data: baseUrl } = useGetVaultUrl()

  const {
    mutateAsync: authenticate,
    isPending,
    error,
  } = useMutation({
    mutationFn: async ({ accountAddress }: AuthenticateMutationArgs) => {
      const res = await vaultApi.authenticate(baseUrl!, accountAddress)
      if (res === null) throw new Error("Authentication failed")
      return res
    },
  })

  return { authenticate, isFetching: isPending, isUrlLoading, error: error ?? undefined }
}
