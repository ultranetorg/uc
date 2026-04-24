import { User } from "types/User"
import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorAddition = {
  candidates: User[]
} & BaseVotableOperation
