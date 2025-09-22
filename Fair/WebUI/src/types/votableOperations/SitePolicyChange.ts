import { ApprovalPolicy } from "types/ApprovalPolicy"
import { BaseVotableOperation } from "./BaseVotableOperation"

export type SitePolicyChange = {
  change: string
  creators: string[]
  approval: ApprovalPolicy
} & BaseVotableOperation
