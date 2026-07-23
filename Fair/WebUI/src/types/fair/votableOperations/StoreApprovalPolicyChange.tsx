import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreApprovalPolicyChange = {
  operation: string
  approval: string
} & BaseVotableOperation
