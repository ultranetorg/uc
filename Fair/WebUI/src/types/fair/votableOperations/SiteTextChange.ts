import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteTextChange = {
  title: string
  slogan: string
  description: string
} & BaseVotableOperation
