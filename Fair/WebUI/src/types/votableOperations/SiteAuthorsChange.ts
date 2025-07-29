import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteAuthorsChange = {
  additionsIds: string[]
  removalsIds: string[]
} & BaseVotableOperation
