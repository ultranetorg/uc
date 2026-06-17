import { ReactNode } from "react"
import { TFunction } from "i18next"
import { Link } from "react-router-dom"

import { truncate } from "lodash"
import { Publisher } from "types"
import { ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"
import { sitesKeys } from "entities"

export const getPublishersTabItemRenderer =
  (t: TFunction, siteId: string, pathname: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher

    switch (column.type) {
      case "author":
        return <Link to={`/${siteId}/m/a/p/${publisher.author.id}`}> {renderAuthor(publisher.author)}</Link>

      case "banned":
        return publisher.bannedTill !== 0 ? publisher.bannedTill : ""

      case "actions":
        return (
          <div className="flex justify-end">
            <Link
              to={`/${siteId}/m/new`}
              state={{
                parentBreadcrumbs: [
                  { path: `/${siteId}/m/a`, title: t("common:proposals") },
                  { path: `/${siteId}/m/a/p/`, title: t("title") },
                ],
                previousPath: pathname,
                title: `Remove author "${truncate(publisher.author.title, { length: 48 })}"`,
                type: "site-authors-removal",
                authors: [publisher.author],
                redirectAfterProposalCreation: `/${siteId}/m/a/r/`,
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
