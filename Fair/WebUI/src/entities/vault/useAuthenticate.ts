import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"

const vaultApi = getVaultApi()

type AuthenticateMutationArgs = {
  accountAddress?: string
}

export const useAuthenticate = () => {
  const { mutateAsync: authenticate, isPending } = useMutation({
    mutationFn: ({ accountAddress }: AuthenticateMutationArgs) => vaultApi.authenticate(accountAddress),
  })

  return { authenticate, isPending }
}
