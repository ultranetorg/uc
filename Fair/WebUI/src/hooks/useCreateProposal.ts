import { useCallback } from "react"

import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId } from "hooks"
import { BaseVotableOperation, OperationCreator, ProposalCreation, ProposalOption } from "types"

export const useCreateProposal = (
  operation: BaseVotableOperation,
  creator: OperationCreator | undefined,
  onSuccess: () => void,
  onError: (err: Error) => void,
  onSettled?: () => void,
) => {
  const storeId = useResolveStoreId()
  const { mutate, isPending } = useTransactMutationWithStatus()

  const execute = useCallback(() => {
    if (!creator) return undefined

    const options = [
      {
        title: "",
        operation,
      },
    ] as ProposalOption[]
    const proposal = new ProposalCreation(storeId!, creator.id, creator.role, "", options, "")

    mutate(proposal, {
      onSuccess,
      onError,
      onSettled,
    })
  }, [creator, mutate, onError, onSettled, onSuccess, operation, storeId])

  return { execute, isExecuting: isPending }
}
