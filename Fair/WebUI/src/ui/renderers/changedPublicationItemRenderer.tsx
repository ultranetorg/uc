import { ReactNode } from "react"

import { PublicationChanged } from "types"
import { TableColumn, TableItem } from "ui/components"
import { publicationBaseSiteItemRenderer } from "./publicationBaseSiteItemRenderer"
import { renderCategory } from "./utils"

export const changedPublicationItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as PublicationChanged

  switch (column.type) {
    case "category":
      return renderCategory(publication.categoryTitle)
    case "version":
      return publication[column.accessor as "currentVersion" | "latestVersion"]
  }

  return publicationBaseSiteItemRenderer(publication, column)
}
