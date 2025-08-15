import { ReactNode } from "react"
import { TFunction } from "i18next"

import { PublicationProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderActionShort, renderCommon, renderPublication, renderTitle } from "./utils"

export const getPublicationsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublicationProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "publication":
        return renderPublication(proposal.publication)
      case "account":
        return renderAccount(proposal.author)
      case "action-short":
        return renderActionShort(t, proposal.options[0].operation.$type)
    }

    return renderCommon(t, column, proposal)
  }
