import { TFunction } from "i18next"
import { ReactNode } from "react"

import { PublisherProposal } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderAccount, renderActionShort, renderCommon } from "ui/renderers/utils"
import { renderAccountsList } from "ui/renderers2"

export const getPublisherProposalsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as PublisherProposal

    switch (column.type) {
      case "account":
        return renderAccount(proposal.by)
      case "accounts-list":
        return proposal.authors ? renderAccountsList(proposal.authors) : t("multipleOptions")
      case "action-short":
        return renderActionShort(t, proposal.operation)
    }

    return renderCommon(t, column, proposal)
  }
