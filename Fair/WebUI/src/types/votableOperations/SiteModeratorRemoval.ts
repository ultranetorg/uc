import { User } from "types/User"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorRemoval = {
  moderator: User
} & BaseVotableOperation
