import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Proposal } from "types"
import { TableColumn, TableItem } from "ui/components/Table"

import { renderAccount, renderAction, renderCommon, renderTitle } from "./utils"

export const getProposalsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderAccount(proposal.byAccount)
      case "action":
        return renderAction(t, proposal.options[0].operation?.$type)
    }

    return renderCommon(t, column, proposal)
  }
