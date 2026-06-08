import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Proposal } from "types"
import { renderUser } from "ui/renderers2"
import { TableColumn, TableItem } from "ui/components"
import { isUserVoted } from "utils"

import { renderActions, renderCommon, renderTitle } from "./utils"

export const getUsersItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
    votesRequired?: number,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal
    const isVoted = isUserVoted(currentUserId, proposal)

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)

      case "account":
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
