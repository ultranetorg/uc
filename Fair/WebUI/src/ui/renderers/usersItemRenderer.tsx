import { ReactNode } from "react"
import { TFunction } from "i18next"

import { BaseProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderActions, renderCommon, renderTitle } from "./utils"

export const getUsersItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string, name: string) => void,
    onReject: (id: string, name: string) => void,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as BaseProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderAccount(proposal.by)
      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.by.nickname),
          () => onReject(item.id, proposal.by.nickname),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          loadingItem && loadingItem.id !== item.id,
        )
    }

    return renderCommon(t, column, proposal)
  }
