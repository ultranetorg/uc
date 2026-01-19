import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"
import { useGetVaultUrl } from "entities/node"

const vaultApi = getVaultApi()

type IsAuthenticatedMutationArgs = {
  userName: string
  session: string
}

export const useIsAuthenticatedMutation = () => {
  const { isLoading, data: baseUrl } = useGetVaultUrl()

  const {
    mutateAsync: isAuthenticated,
    isPending,
    error,
  } = useMutation({
    mutationFn: ({ userName, session }: IsAuthenticatedMutationArgs) =>
      vaultApi.isAuthenticated(baseUrl!, userName, session),
  })

  return { isAuthenticated, isPending, isReady: !isLoading && !!baseUrl, error: error ?? undefined }
}
