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
        return renderAccount(proposal.by)
      case "publication":
        return renderPublication(proposal.publication)
      case "text":
        return renderText(proposal.reviewText)
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
