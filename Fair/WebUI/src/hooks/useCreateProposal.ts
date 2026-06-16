import { useCallback } from "react"
import { useParams } from "react-router-dom"

import { useTransactMutationWithStatus } from "entities/iccpNode"
import { BaseVotableOperation, OperationCreator, ProposalCreation, ProposalOption } from "types"

export const useCreateProposal = (
  operation: BaseVotableOperation,
  creator: OperationCreator | undefined,
  onSuccess: () => void,
  onError: (err: Error) => void,
  onSettled?: () => void,
) => {
  const { siteId } = useParams()
  const { mutate, isPending } = useTransactMutationWithStatus()

  const execute = useCallback(() => {
    if (!creator) return undefined

    const options = [
      {
        title: "",
        operation,
      },
    ] as ProposalOption[]
    const proposal = new ProposalCreation(siteId!, creator.id, creator.role, "", options, "")

    mutate(proposal, {
      onSuccess,
      onError,
      onSettled,
    })
  }, [creator, mutate, onError, onSettled, onSuccess, operation, siteId])

  return { execute, isExecuting: isPending }
}
