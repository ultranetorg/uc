import { TFunction } from "i18next"

import { Proposal } from "types"
import { VotesProgress } from "ui/components"
import { getLeadingVotes } from "utils"

export const renderProposalProgress = (t: TFunction, proposal: Proposal, votesRequired: number) => {
  const leadingVotes = getLeadingVotes(proposal)
  const votes = leadingVotes + proposal.any.length

  return votesRequired > 0 ? (
    <VotesProgress
      votes={votes}
      votesRequired={votesRequired}
      title={`${leadingVotes} + ${t("common:any")}: ${proposal.any.length}`}
    />
  ) : (
    <span>{votes}</span>
  )
}
