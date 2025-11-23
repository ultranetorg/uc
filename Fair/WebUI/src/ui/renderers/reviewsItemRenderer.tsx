import { MouseEvent, ReactNode } from "react"
import { TFunction } from "i18next"

import { ReviewProposal } from "types"
import { ButtonOutline, ButtonPrimary, TableColumn, TableItem } from "ui/components"

import { renderAccount, renderCommon, renderPublication, renderText } from "./utils"

export const getReviewsItemRenderer =
  (t: TFunction, onApproveClick: (id: string) => void, onRejectClick: (id: string) => void) =>
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
        return (
          <div className="flex gap-5">
            <ButtonPrimary
              className="h-9 w-20 capitalize"
              label={t("common:approve")}
              onClick={(e: MouseEvent) => {
                e.stopPropagation()
                onApproveClick(proposal.id)
              }}
            />
            <ButtonOutline
              className="h-9 w-20 capitalize"
              label={t("common:reject")}
              onClick={(e: MouseEvent) => {
                e.stopPropagation()
                onRejectClick(proposal.id)
              }}
            />
          </div>
        )
    }

    return renderCommon(t, column, proposal)
  }
