import { ReactNode } from "react"
import { TFunction } from "i18next"

import { UserUnregistrationProposal } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderActions, renderCommon } from "ui/renderers/utils"
import { renderUser } from "ui/renderers2"
import { isUserVoted } from "utils"

export const getRemoveUsersTabItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
    votesRequired: number,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as UserUnregistrationProposal
    const isVoted = isUserVoted(currentUserId, proposal)

    switch (column.type) {
      case "user":
        return renderUser(proposal.userId, proposal.userName)
      case "created-by":
        return renderUser(proposal.by)
      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.by.name),
          () => onReject(item.id, proposal.by.name),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isVoted || (loadingItem && loadingItem.id !== item.id),
        )
    }

    return renderCommon(t, column, proposal, votesRequired)
  }
