import { memo, useMemo } from "react"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"

import { ProposalTypeViewProps } from "./types"
import { getOptionDescription, getOptionTitle } from "./utils"

export const ProposalDefaultView = memo(({ t, pageState, proposal }: ProposalTypeViewProps) => {
  const items = useMemo<OptionsCollapsesListItem[]>(
    () =>
      proposal.options.map((x, i) => ({
        title: x.title ?? getOptionTitle(t, x),
        description: getOptionDescription(t, x),
        value: i,
        votePercents: 2,
        voted: false,
        votesCount: 4,
      })),
    [proposal.options, t],
  )

  return (
    <OptionsCollapsesList
      className="max-w-187.5"
      items={items}
      showResults={pageState == "results"}
      votesText={t("common:votes")}
    />
  )
})
