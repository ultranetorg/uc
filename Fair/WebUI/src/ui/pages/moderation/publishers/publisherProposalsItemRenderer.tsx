import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Policy, PublisherProposal, Store } from "types"
import { Badge, TableColumn, TableItem } from "ui/components"
import { renderAuthor2, renderCommon, renderProposalProgress, renderYourVote } from "ui/renderers/utils"
import { renderCreator } from "ui/renderers2"
import { calculateVotesRequiredToWinProposal, getVoterIds } from "utils"

export type PublisherProposalsVoting = {
  store?: Store
  policies?: Policy[]
  moderatorId?: string
  publisherIds?: string[]
}

const renderPublishers = (t: TFunction, proposal: PublisherProposal, publisherIds?: string[]) => {
  const [first, ...others] = proposal.authors
  if (!first) return "-"

  const isYou = proposal.authors.some(x => publisherIds?.includes(x.id))
  const names = proposal.authors.map(x => x.title ?? x.name ?? x.id).join(", ")

  return (
    <div className="flex items-center gap-2" title={names}>
      {renderAuthor2(first.title ?? first.name ?? first.id, first.avatarId)}
      {others.length > 0 && <Badge>+{others.length}</Badge>}
      {isYou && <Badge>{t("common:you")}</Badge>}
    </div>
  )
}

export const getPublisherProposalsItemRenderer =
  (t: TFunction, { store, policies, moderatorId, publisherIds }: PublisherProposalsVoting = {}) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublisherProposal

    switch (column.type) {
      case "account":
        return renderCreator(proposal.creator)

      case "accounts-list":
        return renderPublishers(t, proposal, publisherIds)

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
