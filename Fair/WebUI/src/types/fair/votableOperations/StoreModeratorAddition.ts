import { User } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreModeratorAddition = {
  candidates: User[]
} & BaseVotableOperation
