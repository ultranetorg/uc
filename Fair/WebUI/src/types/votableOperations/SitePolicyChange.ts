import { BaseVotableOperation } from "./BaseVotableOperation"

export type SitePolicyChange = {
  change: string
  policy: string
} & BaseVotableOperation
