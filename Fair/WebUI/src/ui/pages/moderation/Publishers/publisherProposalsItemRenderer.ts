import { TFunction } from "i18next"
import { ReactNode } from "react"

import { PublisherProposal } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderActionShort, renderAuthor2, renderCommon } from "ui/renderers/utils"
import { renderUser } from "ui/renderers2"

export const getPublisherProposalsItemRenderer =
  (t: TFunction, votesRequired: number) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublisherProposal
    const author = proposal.authors[0]

    switch (column.type) {
      case "account":
        return renderUser(proposal.by)

      case "accounts-list":
        return renderAuthor2(author!.title!, author!.avatarId)

      case "action-short":
        return renderActionShort(t, proposal.operation)
    }

    return renderCommon(t, column, proposal, votesRequired)
  }
