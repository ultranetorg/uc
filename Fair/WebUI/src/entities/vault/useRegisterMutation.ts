import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"
import { useGetVaultUrl } from "entities/node"

const vaultApi = getVaultApi()

type RegisterMutationArgs = {
  userName: string
}

export const useRegisterMutation = () => {
  const { isLoading, data: baseUrl } = useGetVaultUrl()

  const {
    mutateAsync: mutate,
    isPending,
    error,
  } = useMutation({
    mutationFn: ({ userName }: RegisterMutationArgs) => vaultApi.register(baseUrl!, userName),
  })

  return { mutate, isFetching: isPending, isReady: !isLoading && !!baseUrl, error: error ?? undefined }
}
