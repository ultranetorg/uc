import { TFunction } from "i18next"
import { ReactNode } from "react"
import { Link } from "react-router-dom"

import { AccountBase, Proposal } from "types"
import { AccountInfo } from "ui/components"
import { TableColumn, TableItem, TableItemRenderer, TableRowRenderer } from "ui/components/Table"

const renderAccount = (account: AccountBase) => (
  <AccountInfo title={account.nickname ?? account.address} avatar={account.avatar} />
)

const renderTitle = (proposal: Proposal) => (
  <div className="flex flex-col gap-1" title={proposal.title}>
    <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-4 text-gray-500">
      {proposal.title}
    </span>
    <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2sm leading-5">{proposal.text}</span>
  </div>
)

const renderOptions = (proposal: Proposal) => {
  const votes = proposal.optionsVotesCount.join(" / ")
  return (
    <div className="overflow-hidden text-ellipsis whitespace-nowrap" title={votes}>
      {votes}
    </div>
  )
}

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
    const proposal = item as Proposal

    switch (column.type) {
      case "title": {
        return renderTitle(proposal)
      }

      case "account": {
        return renderAccount(proposal.byAccount)
      }

      case "options":
        return renderOptions(proposal)
    }

    return undefined
  }
