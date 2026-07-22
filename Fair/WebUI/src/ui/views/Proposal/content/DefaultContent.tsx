import { memo, useMemo } from "react"

import { useOperationPolicy } from "app"
import { useResolveStoreId } from "hooks"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"

import { ProposalViewContentProps } from "./types"
import { renderDescription } from "./utils"

export const DefaultContent = memo(
  ({ t, pageState, proposal, isReferendum, voteStatus, votedValue, onVoteClick }: ProposalViewContentProps) => {
    const { voterId } = useOperationPolicy(proposal.operation)
    const storeId = useResolveStoreId()

    const items = useMemo<OptionsCollapsesListItem[]>(
      () =>
        proposal.options.map((x, i) => ({
          title: x.title ?? t("operations:" + x.operation.$type),
          description: renderDescription(storeId!, x),
          value: i,
          votePercents:
            proposal.votesRequiredToWin > 0
              ? Math.min(100, Math.round((proposal.yes[i].length / proposal.votesRequiredToWin) * 100))
              : 0,
          voted: i === votedValue,
          votesCount: proposal.yes[i].length,
        })),
      [proposal.options, proposal.votesRequiredToWin, proposal.yes, storeId, t, votedValue],
    )

    return (
      <OptionsCollapsesList
        className="max-w-187.5"
        items={items}
        showResults={pageState == "results"}
        showVoteButton={isReferendum || (voteStatus !== "voted" && !!voterId)}
        votesText={t("common:votes")}
        votedValue={votedValue}
        onVoteClick={onVoteClick}
      />
    )
  },
)
