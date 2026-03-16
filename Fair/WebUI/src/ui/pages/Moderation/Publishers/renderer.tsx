import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Link } from "react-router-dom"
import { Publisher } from "types"
import { ButtonOutline, LinkFullscreen, TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"

export const getItemRenderer =
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
          <Link to={`/${siteId}/g/new?type=site-author-removal&publisherId=${publisher.author.id}`}>
            <ButtonOutline className="h-9 w-20 capitalize" label={t("common:remove")} />
          </Link>
        )
    }
  }
