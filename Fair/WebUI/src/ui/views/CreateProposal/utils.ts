import { CreateProposalData, OperationType } from "types"
import { getFairOperationType } from "utils"

export const prepareRequest = (data: CreateProposalData) => {
  return {
    ...data,
    options: data.options.map(option => {
      const { title, ...rest } = option
      const $type = getFairOperationType(data.type as OperationType)
      return { title, operation: { $type, ...rest } }
    }),
  }
}
