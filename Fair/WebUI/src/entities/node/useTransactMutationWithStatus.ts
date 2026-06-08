import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"

import { getNodeApi } from "api"
import { useAuthenticationContext } from "app"
import { useGetNodeUrl } from "entities/nexus"
import { useGetNexusUrl } from "entities/node"
import { BaseFairOperation, TransactionStatus } from "types"
import { TransactionApe } from "types/node"

const api = getNodeApi()

type TransactMutationArgs = {
  operations: BaseFairOperation[]
  userName?: string
  session?: string
  signer?: string
}

type TransactMutationCallbacks = {
  onSuccess?: (tx: TransactionApe) => void
  onError?: (error: Error) => void
  onSettled?: () => void
}

export const useTransactMutationWithStatus = () => {
  const nexus = useGetNexusUrl()
  const node = useGetNodeUrl(nexus.data)
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
    mutationFn: ({ operations, userName = undefined, session = undefined, signer = undefined }: TransactMutationArgs) =>
      api.transact(
        node.data!,
        operations,
        userName || selectedUserName!,
        currentUser?.session ?? session ?? "",
        currentUser?.user.owner ?? signer ?? "",
      ),

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
      signer: string | undefined = undefined,
    ) => {
      isPendingRef.current = true
      setIsPending(true)
      callbacksRef.current = callbacks ?? null
      return mutation.mutateAsync({
        operations: Array.isArray(operations) ? operations : [operations],
        userName,
        session,
        signer,
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
