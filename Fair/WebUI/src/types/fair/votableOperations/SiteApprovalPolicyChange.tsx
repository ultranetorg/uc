import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteApprovalPolicyChange = {
  operation: string
  approval: string
} & BaseVotableOperation
