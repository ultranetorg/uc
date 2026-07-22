import { ReactNode } from "react"
import { TFunction } from "i18next"
import { Link } from "react-router-dom"

import { truncate } from "lodash"
import { Publisher } from "types"
import { ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { renderAuthor } from "ui/renderers/utils"
import { storesKeys } from "entities"
import { routes } from "utils"

export const getPublishersTabItemRenderer =
  (t: TFunction, storeId: string, pathname: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher

    switch (column.type) {
      case "author":
        return (
          <Link to={routes.moderation.publisher(storeId, publisher.author.id)}> {renderAuthor(publisher.author)}</Link>
        )

      case "banned":
        return publisher.bannedTill !== 0 ? publisher.bannedTill : ""

      case "actions":
        return (
          <div className="flex justify-end">
            <Link
              to={routes.moderation.createProposal(storeId)}
              state={{
                parentBreadcrumbs: [{ path: routes.moderation.publishers(storeId), title: t("common:proposals") }],
                previousPath: pathname,
                title: `Remove author "${truncate(publisher.author.title, { length: 48 })}"`,
                type: "site-authors-removal",
                authors: [publisher.author],
                redirectAfterProposalCreation: routes.moderation.publishers(storeId, "proposals"),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.publishers(storeId),
              }}
            >
              <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
            </Link>
          </div>
        )
    }
  }
