import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"
import { useGetVaultUrl } from "entities/node"

const vaultApi = getVaultApi()

type AuthenticateMutationArgs = {
  userName: string
  address: string
}

export const useAuthenticateMutation = () => {
  const { isLoading, data: baseUrl } = useGetVaultUrl()

  const {
    mutateAsync: authenticate,
    isPending,
    error,
  } = useMutation({
    mutationFn: ({ userName, address }: AuthenticateMutationArgs) => vaultApi.authenticate(baseUrl!, userName, address),
  })

  return { authenticate, isFetching: isPending, isReady: !isLoading && !!baseUrl, error: error ?? undefined }
}
