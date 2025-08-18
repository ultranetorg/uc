import { ReactNode } from "react"
import { TFunction } from "i18next"

import { ReviewProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderActionShort, renderCommon, renderPublication, renderText } from "./utils"

export const getReviewsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ReviewProposal

    switch (column.type) {
      case "account":
        return renderAccount(proposal.reviewer)
      case "action-short":
        return renderActionShort(t, proposal.options[0].operation.$type)
      case "publication":
        return renderPublication(proposal.publication)
      case "text":
        // @ts-expect-error text
        return renderText(proposal.options[0].operation?.text)
    }

    return renderCommon(t, column, proposal)
  }
