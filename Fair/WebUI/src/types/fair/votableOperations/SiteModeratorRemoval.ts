import { User } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorRemoval = {
  moderator: User
} & BaseVotableOperation
