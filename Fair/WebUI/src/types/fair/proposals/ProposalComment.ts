import { BaseComment } from "types"

export type ProposalComment = BaseComment & {
  proposalId: string
}
