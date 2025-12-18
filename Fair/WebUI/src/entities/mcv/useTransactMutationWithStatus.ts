import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"

import { getMcvApi } from "api"
import { useAccountsContext } from "app"
import { useGetFairUrl } from "entities/nexus"
import { useGetNexusUrl } from "entities/node"
import { BaseFairOperation, TransactionStatus } from "types"
import { TransactionApe } from "types/mcv"

const api = getMcvApi()

type TransactMutationCallbacks = {
  onSuccess?: (tx: TransactionApe) => void
  onError?: (error: Error) => void
  onSettled?: () => void
}

export const useTransactMutationWithStatus = () => {
  const nexus = useGetNexusUrl()
  const fair = useGetFairUrl(nexus.data)
  const { currentAccount } = useAccountsContext()

  const [tag, setTag] = useState<string | undefined>()
  const callbacksRef = useRef<TransactMutationCallbacks | null>(null)
  const hasFinishedRef = useRef(false)

  const mutation = useMutation({
    mutationFn: ({ operation }: { operation: BaseFairOperation }) =>
      api.transact(fair.data!, operation, currentAccount!.address),

    onSuccess: tx => {
      setTag(tx.tag)
    },

    onError: err => {
      if (!hasFinishedRef.current) {
        hasFinishedRef.current = true
        callbacksRef.current?.onError?.(err)
        callbacksRef.current?.onSettled?.()
      }
    },
  })

  const query = useQuery<TransactionApe, Error>({
    queryKey: ["operations", "tag", tag],
    enabled: !!fair.data && !!tag,
    queryFn: () => api.outgoingTransaction(fair.data!, tag!),
    refetchInterval: query =>
      query.state.data?.status === TransactionStatus.Confirmed ||
      query.state.data?.status === TransactionStatus.FailedOrNotFound
        ? false
        : 1000,
    retry: false,
  })

  useEffect(() => {
    if (!query.data || hasFinishedRef.current) return

    if (query.data.status === TransactionStatus.Confirmed) {
      hasFinishedRef.current = true
      callbacksRef.current?.onSuccess?.(query.data)
      callbacksRef.current?.onSettled?.()
    }

    if (query.data.status === TransactionStatus.FailedOrNotFound) {
      hasFinishedRef.current = true
      callbacksRef.current?.onError?.(new Error("Transaction failed or not found"))
      callbacksRef.current?.onSettled?.()
    }
  }, [query.data])

  const mutate = useCallback(
    (operation: BaseFairOperation, callbacks?: TransactMutationCallbacks) => {
      hasFinishedRef.current = false
      callbacksRef.current = callbacks ?? null
      return mutation.mutateAsync({ operation })
    },
    [mutation],
  )

  // @ts-expect-error fix
  const isPending = mutation.isPending || (query.isPending && query.data?.status !== TransactionStatus.Confirmed)

  return {
    mutate,
    data: query.data,
    isPending,
    isReady: !!fair.data && !!currentAccount?.address,
    error: mutation.error || query.error,
  }
}
