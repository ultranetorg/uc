import { User } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorAddition = {
  candidates: User[]
} & BaseVotableOperation
