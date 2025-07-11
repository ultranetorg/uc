import { TFunction } from "i18next"
import { ReactNode } from "react"

import { TEST_REFERENDUM_AUTHOR } from "testConfig"
import { AuthorReferendum, BaseVotableOperation } from "types"
import { TableColumn, TableItem, TableItemRenderer } from "ui/components/Table"
import { formatProposal } from "utils"

export const getReferendumsItemRenderer =
  (t: TFunction): TableItemRenderer =>
  (item: TableItem, column: TableColumn): ReactNode => {
    if (column.accessor === "createdBy") {
      return (
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 overflow-hidden rounded-full">
            <img src={TEST_REFERENDUM_AUTHOR} className="h-full w-full object-cover" />
          </div>
          <span className="text-2sm font-medium leading-5">nickname123</span>
        </div>
      )
    }

    if (column.type === "proposal") {
      return formatProposal(item["proposal"] as BaseVotableOperation, t)
    }

    if (column.type === "voting") {
      const typed = item as AuthorReferendum
      return (
        <div className="flex items-center gap-1">
          <span className="text-light-green" title={t("common:yesVotes")}>
            {typed.yesCount}
          </span>
          <span className="text-gray-400">/</span>
          <span className="text-light-red" title={t("common:noVotes")}>
            {typed.noCount}
          </span>
          <span className="text-gray-400">/</span>
          <span className="text-light-yellow" title={t("common:absVotes")}>
            {typed.absCount}
          </span>
        </div>
      )
    }
    return undefined
  }
