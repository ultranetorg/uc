import { useMutation } from "@tanstack/react-query"

import { getVaultApi } from "api"

const vaultApi = getVaultApi()

type IsAuthenticatedMutationArgs = {
  accountAddress: string
  session: string
}

export const useIsAuthenticated = () => {
  const { mutateAsync: isAuthenticated, isPending } = useMutation({
    mutationFn: ({ accountAddress, session }: IsAuthenticatedMutationArgs) =>
      vaultApi.isAuthenticated(accountAddress, session),
  })

  return { isAuthenticated, isPending }
}
