import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Proposal } from "types"
import { TableColumn } from "ui/components"

import { renderLastsFor } from "./renderLastsFor"
import { renderNabb } from "./renderNabb"
import { renderVotes } from "./renderVotes"

export const renderCommon = (
  t: TFunction,
  column: TableColumn,
  proposal: Proposal,
  votesRequired?: number,
): ReactNode => {
  switch (column.type) {
    case "nabb":
      return renderNabb(t, proposal)
    case "lasts-for":
      return renderLastsFor(t, proposal.creationTime)
    case "votes":
      return renderVotes(
        proposal.yes.map(x => x.length),
        votesRequired,
      )
  }

  return undefined
}
