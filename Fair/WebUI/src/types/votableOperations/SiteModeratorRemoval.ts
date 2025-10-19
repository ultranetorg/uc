import { BaseVotableOperation } from "./BaseVotableOperation"

export type SiteModeratorRemoval = {
  moderatorId: string[]
} & BaseVotableOperation
