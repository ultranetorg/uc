import { useCallback } from "react"
import { useMutation } from "@tanstack/react-query"

import { getMcvApi } from "api"
import { useAccountsContext } from "app"
import { useGetFairUrl } from "entities/nexus"
import { useGetNexusUrl } from "entities/node"
import { BaseFairOperation } from "types"

const api = getMcvApi()

type TransactMutationArgs = {
  operation: BaseFairOperation
}

export const useTransactMutation = () => {
  const nexus = useGetNexusUrl()
  const fair = useGetFairUrl(nexus.data)
  const { currentAccount } = useAccountsContext()

  const mutationFn = useCallback(
    async ({ operation }: TransactMutationArgs) => {
      return api.transact(fair.data!, operation, currentAccount!.address)
    },
    [currentAccount, fair.data],
  )

  const {
    mutateAsync: mutate,
    isPending,
    error,
  } = useMutation({
    mutationFn,
  })

  return {
    mutate,
    isPending,
    isReady: !!fair?.data && !!currentAccount?.address,
    error: error ?? undefined,
  }
}
