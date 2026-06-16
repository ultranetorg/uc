import { TFunction } from "i18next"
import { ReactNode } from "react"
import { ModeratorProposal } from "types"

import { TableColumn, TableItem } from "ui/components"
import { renderActionShort, renderCommon } from "ui/renderers/utils"
import { renderUser, renderUsersList } from "ui/renderers2"

export const getModeratorsProposalsItemRenderer =
  (t: TFunction, votesRequired?: number) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ModeratorProposal

    switch (column.type) {
      case "account":
        return renderUser(proposal.by)
      case "accounts-list":
        return renderUsersList(proposal.moderators)
      case "action-short":
        return renderActionShort(t, proposal.operation)
    }

    return renderCommon(t, column, proposal, votesRequired)
  }
