import { ReactNode } from "react"
import { TFunction } from "i18next"

import { UserProposal } from "types"
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
    const proposal = item as UserProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderAccount(proposal.signer)
      case "actions":
        return renderActions(
          t,
          () => onApprove(item.id, proposal.signer.nickname),
          () => onReject(item.id, proposal.signer.nickname),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          loadingItem && loadingItem.id !== item.id,
        )
    }

    return renderCommon(t, column, proposal)
  }
