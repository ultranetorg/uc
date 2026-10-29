import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Policy, Proposal, Store } from "types"
import { TableColumn, TableItem } from "ui/components/Table"
import {
  renderAction,
  renderCommon,
  renderLeadingOption,
  renderProposalProgress,
  renderTitle,
  renderYourVote,
} from "ui/renderers/utils"
import { renderCreator } from "ui/renderers2"
import { calculateVotesRequiredToWinProposal, getVoterIds } from "utils"

export type ProposalsVoting = {
  store?: Store
  policies?: Policy[]
  moderatorId?: string
  publisherIds?: string[]
}

export const getProposalsItemRenderer =
  (t: TFunction, { store, policies, moderatorId, publisherIds }: ProposalsVoting = {}) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderCreator(proposal.creator)
      case "action":
        return renderAction(t, proposal.operation)
      case "leading-option":
        return renderLeadingOption(t, proposal)
      case "progress":
        return renderProposalProgress(
          t,
          proposal,
          calculateVotesRequiredToWinProposal(proposal.operation, store, policies),
        )
      case "your-vote":
        return renderYourVote(t, proposal, getVoterIds(proposal.operation, policies, moderatorId, publisherIds))
    }

    return renderCommon(t, column, proposal)
  }
