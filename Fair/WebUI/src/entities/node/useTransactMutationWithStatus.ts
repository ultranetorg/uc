import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"

import { getNodeApi } from "api"
import { useManageUsersContext } from "app"
import { useGetNodeUrl } from "entities/nexus"
import { useGetNexusUrl } from "entities/node"
import { BaseFairOperation, TransactionStatus } from "types"
import { TransactionApe } from "types/node"

const api = getNodeApi()

type TransactMutationArgs = {
  operations: BaseFairOperation[]
  userName?: string
}

type TransactMutationCallbacks = {
  onSuccess?: (tx: TransactionApe) => void
  onError?: (error: Error) => void
  onSettled?: () => void
}

export const useTransactMutationWithStatus = () => {
  const nexus = useGetNexusUrl()
  const node = useGetNodeUrl(nexus.data)
  const { selectedUserName } = useManageUsersContext()

  const [tag, setTag] = useState<string | undefined>()
  const callbacksRef = useRef<TransactMutationCallbacks | null>(null)
  const isPendingRef = useRef(false)

  const mutation = useMutation({
    mutationFn: ({ operations, userName = undefined }: TransactMutationArgs) =>
      api.transact(node.data!, operations, userName || selectedUserName!),

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
    enabled: !!node.data && !!tag,
    queryFn: () => api.outgoingTransaction(node.data!, tag!),
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
      isPendingRef.current = false
    } else if (query.data.status === TransactionStatus.FailedOrNotFound) {
      callbacksRef.current?.onError?.(new Error("Transaction failed or not found"))
      callbacksRef.current?.onSettled?.()
      isPendingRef.current = false
    }
  }, [query.data])

  const mutate = useCallback(
    (
      operations: BaseFairOperation | BaseFairOperation[],
      callbacks?: TransactMutationCallbacks,
      userName: string | undefined = undefined,
    ) => {
      isPendingRef.current = true
      callbacksRef.current = callbacks ?? null
      return mutation.mutateAsync({ operations: Array.isArray(operations) ? operations : [operations], userName })
    },
    [mutation],
  )

  return {
    mutate,
    data: query.data,
    isPending: isPendingRef.current,
    isReady: !!node.data && !!selectedUserName,
    error: mutation.error || query.error,
  }
}
