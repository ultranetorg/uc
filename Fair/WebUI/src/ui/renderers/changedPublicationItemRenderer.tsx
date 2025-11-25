import { ReactNode } from "react"

import { ChangedPublication } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAccount, renderCategory, renderPublication } from "./utils"

export const changedPublicationItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as ChangedPublication

  switch (column.type) {
    case "account":
      return renderAccount(publication.author)
    case "category":
      return renderCategory(publication.categoryTitle)
    case "publication":
      return renderPublication(publication.publication)
    case "version":
      return publication[column.accessor as "currentVersion" | "latestVersion"]
  }

  return undefined
}
