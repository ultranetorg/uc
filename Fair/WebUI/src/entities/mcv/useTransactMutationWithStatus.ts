import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"

import { getMcvApi } from "api"
import { useManageUsersContext } from "app"
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
  const { selectedUserName } = useManageUsersContext()

  const [tag, setTag] = useState<string | undefined>()
  const callbacksRef = useRef<TransactMutationCallbacks | null>(null)
  const isPendingRef = useRef(false)

  const mutation = useMutation({
    mutationFn: ({ operations }: { operations: BaseFairOperation[] }) =>
      api.transact(fair.data!, operations, selectedUserName!),

    onSuccess: tx => {
      setTag(tx.tag)
    },

    onError: err => {
      if (isPendingRef.current) {
        isPendingRef.current = false
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
    if (!query.data) return

    if (query.data.status === TransactionStatus.Confirmed) {
      callbacksRef.current?.onSuccess?.(query.data)
      callbacksRef.current?.onSettled?.()
    } else if (query.data.status === TransactionStatus.FailedOrNotFound) {
      callbacksRef.current?.onError?.(new Error("Transaction failed or not found"))
      callbacksRef.current?.onSettled?.()
    }

    isPendingRef.current = false
  }, [query.data])

  const mutate = useCallback(
    (operations: BaseFairOperation | BaseFairOperation[], callbacks?: TransactMutationCallbacks) => {
      isPendingRef.current = true
      callbacksRef.current = callbacks ?? null
      return mutation.mutateAsync({ operations: Array.isArray(operations) ? operations : [operations] })
    },
    [mutation],
  )

  return {
    mutate,
    data: query.data,
    isPending: isPendingRef.current,
    isReady: !!fair.data && !!selectedUserName,
    error: mutation.error || query.error,
  }
}
