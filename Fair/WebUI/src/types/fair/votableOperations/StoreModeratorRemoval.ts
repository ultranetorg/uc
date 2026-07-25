import { User } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreModeratorRemoval = {
  moderator: User
} & BaseVotableOperation
