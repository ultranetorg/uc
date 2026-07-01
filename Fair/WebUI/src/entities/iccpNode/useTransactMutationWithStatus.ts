import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"

import { getIccpNodeApi } from "api"
import { useAuthenticationContext } from "app"
import { useGetNexusUrl } from "entities/localFair"
import { useGetIccpNodeUrl } from "entities/nexus"
import { BaseFairOperation } from "types"
import { TransactionApe, TransactionStatus } from "types/iccpNode"

const api = getIccpNodeApi()

type TransactMutationArgs = {
  operations: BaseFairOperation[]
  userName?: string
  session?: string
}

type TransactMutationCallbacks = {
  onSuccess?: (tx: TransactionApe) => void
  onError?: (error: Error) => void
  onSettled?: () => void
}

export const useTransactMutationWithStatus = () => {
  const nexus = useGetNexusUrl()
  const node = useGetIccpNodeUrl(nexus.data)
  const { selectedUserName, users } = useAuthenticationContext()

  const [tag, setTag] = useState<string | undefined>()
  const [isPending, setIsPending] = useState(false)

  const callbacksRef = useRef<TransactMutationCallbacks | null>(null)
  const isPendingRef = useRef(false)

  const currentUser = users.find(x => x.user.name === selectedUserName)

  const setNotPending = useCallback(() => {
    isPendingRef.current = false
    setIsPending(false)
  }, [])

  const mutation = useMutation({
    mutationFn: ({ operations, userName = undefined, session = undefined }: TransactMutationArgs) =>
      api.transact(node.data!, operations, userName || selectedUserName!, currentUser?.session ?? session ?? ""),

    onSuccess: tx => {
      setTag(tx.tag)
    },

    onError: err => {
      if (isPendingRef.current) {
        setNotPending()
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
      query.state.data?.status === TransactionStatus.FailedOrNotFound ||
      (query.state.data?.status === TransactionStatus.None && query.state.data?.error !== null)
        ? false
        : 1000,
    retry: false,
    refetchOnWindowFocus: false,
  })

  useEffect(() => {
    if (!query.data) return

    if (query.data.status === TransactionStatus.Confirmed) {
      callbacksRef.current?.onSuccess?.(query.data)
      callbacksRef.current?.onSettled?.()
      setNotPending()
    } else if (query.data.status === TransactionStatus.FailedOrNotFound) {
      callbacksRef.current?.onError?.(new Error("Transaction failed or not found"))
      callbacksRef.current?.onSettled?.()
      setNotPending()
    } else if (query.data.status === TransactionStatus.None && query.data.error !== null) {
      callbacksRef.current?.onError?.(new Error(query.data.error))
      callbacksRef.current?.onSettled?.()
      setNotPending()
    }
  }, [query.data, setNotPending])

  const mutate = useCallback(
    (
      operations: BaseFairOperation | BaseFairOperation[],
      callbacks?: TransactMutationCallbacks,
      userName: string | undefined = undefined,
      session: string | undefined = undefined,
    ) => {
      isPendingRef.current = true
      setIsPending(true)
      callbacksRef.current = callbacks ?? null
      return mutation.mutateAsync({
        operations: Array.isArray(operations) ? operations : [operations],
        userName,
        session,
      })
    },
    [mutation],
  )

  return {
    mutate,
    data: query.data,
    isPending,
    isReady: !!node.data && !!selectedUserName,
    error: mutation.error || query.error,
  }
}
