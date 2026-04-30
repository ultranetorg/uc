import { AuthorBase } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteAuthorsRemoval = {
  removalsIds: string[]
  removals: AuthorBase[]
} & BaseVotableOperation
