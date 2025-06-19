import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteDescriptionChange = {
  siteId: string
  description: string
} & BaseVotableOperation
