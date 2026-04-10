import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"
import { useGetVaultUrl } from "entities/node"

const vaultApi = getVaultApi()

type AuthenticateMutationArgs = {
  user: string
  account: string
}

export const useAuthenticateMutation = () => {
  const { isLoading, data: baseUrl } = useGetVaultUrl()

  const {
    mutateAsync: authenticate,
    isPending,
    error,
  } = useMutation({
    mutationFn: ({ user, account }: AuthenticateMutationArgs) => vaultApi.authenticate(baseUrl!, user, account),
  })

  return { mutate: authenticate, isFetching: isPending, isReady: !isLoading && !!baseUrl, error: error ?? undefined }
}
