import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Link } from "react-router-dom"
import { Moderator } from "types"
import { ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { renderAccount } from "ui/renderers/utils"

export const moderatorsTabItemRenderer =
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
          <Link
            to={`/${siteId}/g/new`}
            state={{
              parentBreadcrumbs: [
                { path: `/${siteId}/m`, title: t("common:proposals") },
                { path: `/${siteId}/m/m/`, title: t("title") },
              ],
              previousPath: `/${siteId}/m/m/`,
              title: t("removeModerator"),
              type: "site-moderator-removal",
              moderators: [publisher.user],
            }}
          >
            <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
          </Link>
        )
    }
  }
