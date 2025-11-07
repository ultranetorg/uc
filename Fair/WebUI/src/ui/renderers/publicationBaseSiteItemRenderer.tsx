import { ReactNode } from "react"

import { PublicationBaseSite } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderPublication } from "./utils"

export const publicationBaseSiteItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as PublicationBaseSite

  switch (column.type) {
    case "publication":
      return renderPublication(publication.publication)
    case "account":
      return renderAccount(publication.author)
  }

  return undefined
}
