import { memo, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"

import { ProposalViewContentProps } from "./types"
import { renderDescription } from "./utils"

export const DefaultContent = memo(
  ({ t, pageState, proposal, voteStatus, votedValue, onVoteClick }: ProposalViewContentProps) => {
    const { getOperationVoterId } = useModerationContext()
    const { siteId } = useParams()
    const voterId = getOperationVoterId(proposal.operation)

    const items = useMemo<OptionsCollapsesListItem[]>(
      () =>
        proposal.options.map((x, i) => ({
          title: x.title ?? t("operations:" + x.operation.$type),
          description: renderDescription(siteId!, x),
          value: i,
          votePercents:
            proposal.votesRequiredToWin > 0
              ? Math.min(100, Math.round((proposal.optionsVotesCount[i] / proposal.votesRequiredToWin) * 100))
              : 0,
          voted: i === votedValue,
          votesCount: proposal.optionsVotesCount[i],
        })),
      [proposal.options, proposal.optionsVotesCount, proposal.votesRequiredToWin, siteId, t, votedValue],
    )

    return (
      <OptionsCollapsesList
        className="max-w-187.5"
        items={items}
        showResults={pageState == "results"}
        showVoteButton={voteStatus !== "voted" && !!voterId}
        votesText={t("common:votes")}
        votedValue={votedValue}
        onVoteClick={onVoteClick}
      />
    )
  },
)
