import { ReactNode } from "react"

import { ChangedPublication } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAuthor2, renderCategory, renderPublication2 } from "./utils"

export const changedPublicationItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as ChangedPublication

  switch (column.type) {
    case "account":
      return renderAuthor2(publication.authorTitle, publication.authorLogoId)
    case "category":
      return renderCategory(publication.categoryTitle)
    case "publication":
      return renderPublication2(publication.title, publication.logoId)
    case "version":
      return publication[column.accessor as "currentVersion" | "latestVersion"]
  }

  return undefined
}
