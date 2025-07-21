import { TFunction } from "i18next"
import { ReactNode } from "react"
import { Link } from "react-router-dom"

import { TEST_REFERENDUM_AUTHOR } from "testConfig"
import { AuthorReferendum, BaseVotableOperation } from "types"
import { TableColumn, TableItem, TableItemRenderer, TableRowRenderer } from "ui/components/Table"
import { buildSrc, formatOption } from "utils"

const renderVoting = (t: TFunction, voting: AuthorReferendum) => (
  <div className="flex items-center gap-1">
    <span className="text-light-green" title={t("common:yesVotes")}>
      {voting.yesCount}
    </span>
    <span className="text-gray-400">/</span>
    <span className="text-light-red" title={t("common:noVotes")}>
      {voting.noCount}
    </span>
    <span className="text-gray-400">/</span>
    <span className="text-light-yellow" title={t("common:absVotes")}>
      {voting.absCount}
    </span>
  </div>
)

const renderAccountBy = (referendum: AuthorReferendum) => (
  <div className="flex items-center gap-2">
    <div className="h-8 w-8 flex-shrink-0 overflow-hidden rounded-full">
      <img src={buildSrc(referendum.byAvatar, TEST_REFERENDUM_AUTHOR)} className="h-full w-full object-cover" />
    </div>
    <span
      title={referendum.byNickname ?? referendum.byAddress}
      className="flex-1 overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5"
    >
      {referendum.byNickname ?? referendum.byAddress}
    </span>
  </div>
)

export const getReferendumsRowRenderer =
  (siteId: string): TableRowRenderer =>
  (children: JSX.Element, item: TableItem): JSX.Element => (
    <Link to={`/${siteId}/a-r/${item.id}`} key={item.id}>
      {children}
    </Link>
  )

export const getReferendumsItemRenderer =
  (t: TFunction): TableItemRenderer =>
  (item: TableItem, column: TableColumn): ReactNode => {
    if (column.type === "account-by") {
      const referendum = item as AuthorReferendum
      return renderAccountBy(referendum)
    }

    if (column.type === "option") {
      return formatOption(item["option"] as BaseVotableOperation, t)
    }

    if (column.type === "voting") {
      const referendum = item as AuthorReferendum
      return renderVoting(t, referendum)
    }

    return undefined
  }
