import { ReactNode } from "react"

import { PublicationUnpublished } from "types"
import { TableColumn, TableItem } from "ui/components"

import { renderAuthor2, renderPublication2 } from "./utils"

export const unpublishedPublicationsItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const publication = item as PublicationUnpublished

  switch (column.type) {
    case "publication":
      return renderPublication2(publication.title, publication.logoId)
    case "author":
      return renderAuthor2(publication.authorTitle, publication.authorLogoId)
  }

  return undefined
}
