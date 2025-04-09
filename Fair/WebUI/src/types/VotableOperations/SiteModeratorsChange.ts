import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorsChange = {
  siteId: string
  additionsIds: string[]
  removalsIds: string[]
} & BaseVotableOperation
