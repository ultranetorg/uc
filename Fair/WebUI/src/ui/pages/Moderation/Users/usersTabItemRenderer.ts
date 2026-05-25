import { ReactNode } from "react"

import { User } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderUser } from "ui/renderers/utils"

export const usersTabItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const user = item as User

  switch (column.type) {
    case "user":
      return renderUser(user)
  }

  return null
}
