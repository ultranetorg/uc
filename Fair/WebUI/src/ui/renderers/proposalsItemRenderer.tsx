import { ReactNode } from "react"
import { Link } from "react-router-dom"

import { Proposal } from "types"
import { TableColumn, TableItem, TableRowRenderer } from "ui/components/Table"

import { renderAccount, renderAction, renderCommon, renderTitle } from "./utils"
import { TFunction } from "i18next"

export const getReferendumsRowRenderer =
  (siteId: string): TableRowRenderer =>
  (children: JSX.Element, item: TableItem): JSX.Element => (
    <Link to={`/${siteId}/g/${item.id}`} key={item.id}>
      {children}
    </Link>
  )

export const getDiscussionsRowRowRenderer =
  (siteId: string): TableRowRenderer =>
  (children: JSX.Element, item: TableItem): JSX.Element => (
    <Link to={`/${siteId}/m-d/${item.id}`} key={item.id}>
      {children}
    </Link>
  )

export const getProposalsItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as Proposal

    switch (column.type) {
      case "title":
        return renderTitle(proposal.title, proposal.text)
      case "account":
        return renderAccount(proposal.byAccount)
      case "action":
        return renderAction(t, proposal.options[0].operation?.$type)
    }

    return renderCommon(t, column, proposal)
  }
