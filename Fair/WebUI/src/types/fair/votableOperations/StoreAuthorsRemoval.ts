import { AuthorBaseAvatar } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreAuthorsRemoval = {
  removalsIds: string[]
  removals: AuthorBaseAvatar[]
} & BaseVotableOperation
