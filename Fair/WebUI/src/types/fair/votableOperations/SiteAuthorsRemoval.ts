import { AuthorBaseAvatar } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteAuthorsRemoval = {
  removalsIds: string[]
  removals: AuthorBaseAvatar[]
} & BaseVotableOperation
