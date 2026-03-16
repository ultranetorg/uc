import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Link } from "react-router-dom"
import { Moderator } from "types"
import { ButtonOutline, TableColumn, TableItem } from "ui/components"
import { renderAccount } from "ui/renderers/utils"

export const getItemRenderer =
  (t: TFunction, siteId: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Moderator

    switch (column.type) {
      case "account":
        return renderAccount(publisher.user)

      case "banned":
        return publisher.bannedTill !== 0 ? publisher.bannedTill : ""

      case "actions":
        return (
          <Link to={`/${siteId}/g/new`} state={{ type: "site-moderator-removal", publisherId: publisher.user.id }}>
            <ButtonOutline className="h-9 w-20 capitalize" label={t("common:remove")} />
          </Link>
        )
    }
  }
