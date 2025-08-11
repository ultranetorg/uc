import { ReactNode } from "react"

import { ReviewProposal } from "types"
import { TableColumn, TableItem } from "ui/components/Table"

import {
  renderAccount,
  renderApproveRejectAction,
  renderDate,
  renderPublication,
  renderRating,
  renderText,
} from "./common"

export const itemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const proposal = item as ReviewProposal

  switch (column.type) {
    case "account":
      return renderAccount(proposal.reviewer)

    case "approve-reject-action":
      return renderApproveRejectAction()

    case "date":
      return renderDate(proposal.creationTime)

    case "publication":
      return renderPublication(proposal.publication)

    case "rating":
      // @ts-expect-error rating
      return renderRating(proposal.options[0].operation?.rating)

    case "text":
      // @ts-expect-error text
      return renderText(proposal.options[0].operation?.text)
  }

  return undefined
}
