import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorsChange = {
  additionsIds: string[]
  removalsIds: string[]
} & BaseVotableOperation
