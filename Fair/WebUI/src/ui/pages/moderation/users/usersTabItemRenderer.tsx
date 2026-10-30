import { ReactNode } from "react"
import { TFunction } from "i18next"

import { User } from "types"
import { TableColumn, TableItem } from "ui/components"
import { renderWithYouBadge } from "ui/renderers/utils"
import { renderUser } from "ui/renderers2"

export const getUsersTabItemRenderer =
  (t: TFunction, currentUserId?: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const user = item as User

    switch (column.type) {
      case "user":
        return renderWithYouBadge(t, renderUser(user), user.id === currentUserId)
    }

    return null
  }
