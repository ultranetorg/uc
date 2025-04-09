import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteAuthorsChange = {
  siteId: string
  additionsIds: string[]
  removalsIds: string[]
} & BaseVotableOperation
