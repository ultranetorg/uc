import { ReactNode } from "react"
import { TFunction } from "i18next"

import { PublicationProposal } from "types"
import { TableColumn, TableItem } from "ui/components"

import {
  renderAccount,
  renderActionShort,
  renderAr,
  renderAuthor2,
  renderCommon,
  renderPublication,
  renderTitle,
} from "./utils"

export const getPublicationsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublicationProposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "publication":
        return renderPublication(proposal.publication)
      case "author":
        return renderAuthor2(proposal.authorTitle, proposal.authorLogoId)
      case "action-short":
        return renderActionShort(t, proposal.operation)
      case "ar":
        return renderAr(t, proposal)
      case "account":
        return renderAccount(proposal.by)
    }

    return renderCommon(t, column, proposal)
  }
