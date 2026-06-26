import { ReactNode } from "react"
import { TFunction } from "i18next"
import { Link } from "react-router-dom"

import { truncate } from "lodash"
import { Publisher } from "types"
import { ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"
import { sitesKeys } from "entities"
import { routes } from "utils"

export const getPublishersTabItemRenderer =
  (t: TFunction, siteId: string, pathname: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher

    switch (column.type) {
      case "author":
        return (
          <Link to={routes.moderation.publisher(siteId, publisher.author.id)}> {renderAuthor(publisher.author)}</Link>
        )

      case "banned":
        return publisher.bannedTill !== 0 ? publisher.bannedTill : ""

      case "actions":
        return (
          <div className="flex justify-end">
            <Link
              to={routes.moderation.create(siteId)}
              state={{
                parentBreadcrumbs: [
                  { path: routes.moderation.publishers(siteId), title: t("common:proposals") },
                  { path: routes.moderation.publishers(siteId, "publishers"), title: t("title") },
                ],
                previousPath: pathname,
                title: `Remove author "${truncate(publisher.author.title, { length: 48 })}"`,
                type: "site-authors-removal",
                authors: [publisher.author],
                redirectAfterProposalCreation: routes.moderation.publishers(siteId, "proposals"),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: sitesKeys.publishers(siteId),
              }}
            >
              <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
            </Link>
          </div>
        )
    }
  }
