import { TFunction } from "i18next"

import { ProposalDetails } from "types"

import { PageState } from "../types"

export type VoteStatus = "idle" | "voting" | "voted"

export type VoteAction = "approve" | "reject"

export type ProposalViewContentProps = {
  t: TFunction
  pageState?: PageState
  proposal: ProposalDetails
  voteStatus?: VoteStatus
  votedValue?: number
  onVoteClick?: (value: number) => void
}
