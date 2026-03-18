import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteNameChange = {
  siteName: string
  name: string
} & BaseVotableOperation
