import { ReactNode } from "react"
import { TFunction } from "i18next"

import { ReviewProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderActions, renderCommon, renderPublication, renderText } from "./utils"

export const getReviewsItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string) => void,
    onReject: (id: string) => void,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ReviewProposal

    switch (column.type) {
      case "account":
        return renderAccount(proposal.reviewer)
      case "publication":
        return renderPublication(proposal.publication)
      case "text":
        // @ts-expect-error text
        return renderText(proposal.options[0].operation?.text)
      case "review-action":
        return renderActions(
          t,
          () => onApprove(proposal.id),
          () => onReject(proposal.id),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          loadingItem && loadingItem.id !== item.id,
        )
    }

    return renderCommon(t, column, proposal)
  }
