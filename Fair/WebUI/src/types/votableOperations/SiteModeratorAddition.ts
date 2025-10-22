import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorAddition = {
  candidatesIds: string[]
} & BaseVotableOperation
