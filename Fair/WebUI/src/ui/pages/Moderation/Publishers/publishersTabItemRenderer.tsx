import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Link } from "react-router-dom"
import { Publisher } from "types"
import { ButtonPrimary, LinkFullscreen, TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"

export const getPublishersTabItemRenderer =
  (t: TFunction, siteId: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher

    switch (column.type) {
      case "author":
        return (
          <LinkFullscreen to={`/${siteId}/a/${publisher.author.id}`}> {renderAuthor(publisher.author)}</LinkFullscreen>
        )
      case "banned":
        return publisher.bannedTill !== 0 ? publisher.bannedTill : ""

      case "actions":
        return (
          <Link
            to={`/${siteId}/m/new`}
            state={{
              parentBreadcrumbs: [
                { path: `/${siteId}/m/a`, title: t("common:proposals") },
                { path: `/${siteId}/m/a/p/`, title: t("title") },
              ],
              previousPath: `/${siteId}/m/a/`,
              title: t("removeAuthor"),
              type: "site-authors-removal",
              authors: [publisher.author],
            }}
          >
            <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
          </Link>
        )
    }
  }
