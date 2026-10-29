import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Proposal } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderCreator } from "ui/renderers2"
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
        return renderCreator(proposal.creator)

      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.creator.title),
          () => onReject(item.id, proposal.creator.title),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isVoted || (loadingItem && loadingItem.id !== item.id),
        )
    }

    return renderCommon(t, column, proposal, votesRequired)
  }
