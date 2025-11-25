import { ReactNode } from "react"

import { UnpublishedProduct } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderPublication } from "./utils"

export const unpublishedProductItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as UnpublishedProduct

  switch (column.type) {
    case "publication":
      return renderPublication(publication.publication)
    case "account":
      return renderAccount(publication.author)
  }

  return undefined
}
