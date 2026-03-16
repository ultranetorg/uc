import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Publisher } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"

export const getItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher
    console.log(publisher)

    switch (column.type) {
      case "author":
        return renderAuthor(publisher.author)
    }
  }
