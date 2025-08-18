import { ReactNode } from "react"
import { TFunction } from "i18next"

import { UserProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderCommon, renderTitle } from "./utils"

export const getUsersItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as UserProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderAccount(proposal.signer)
    }

    return renderCommon(t, column, proposal)
  }
