import { memo, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"

import { ProposalTypeViewProps } from "./types"
import { renderDescription } from "./utils"

export const ProposalDefaultView = memo(
  ({ t, pageState, proposal, votedValue, onVoteClick }: ProposalTypeViewProps) => {
    const { getOperationVoterId } = useModerationContext()
    const { siteId } = useParams()
    const voterId = getOperationVoterId(proposal.operation)

    const items = useMemo<OptionsCollapsesListItem[]>(
      () =>
        proposal.options.map((x, i) => ({
          title: x.title ?? t("operations:" + x.operation.$type),
          description: renderDescription(siteId!, x),
          value: i,
          votePercents: 2,
          voted: false,
          votesCount: 4,
        })),
      [proposal.options, siteId, t],
    )

    return (
      <OptionsCollapsesList
        className="max-w-187.5"
        items={items}
        showResults={pageState == "results"}
        showVoteButton={!!voterId}
        votesText={t("common:votes")}
        votedValue={votedValue}
        onVoteClick={onVoteClick}
      />
    )
  },
)
