import { ReactNode } from "react"
import { TFunction } from "i18next"

import { UserUnregistrationProposal } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderActions, renderCommon, renderUser } from "ui/renderers/utils"
import { isUserVoted } from "utils"

export const getUserRemovalsTabItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
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
        return renderUser(proposal.by.id, proposal.by.nickname)
      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.by.nickname),
          () => onReject(item.id, proposal.by.nickname),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isVoted || (loadingItem && loadingItem.id !== item.id),
        )
    }

    return renderCommon(t, column, proposal)
  }
