import { BaseVotableOperation } from "./BaseVotableOperation"

export type SitePolicyChange = {
  siteId: string
  change: string
  policy: string
} & BaseVotableOperation
