import { ReactNode } from "react"
import { TFunction } from "i18next"
import { Link } from "react-router-dom"
import { truncate } from "lodash"

import { Moderator } from "types"
import { ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { storesKeys } from "entities"
import { renderUser } from "ui/renderers2"
import { routes } from "utils"

export const moderatorsTabItemRenderer =
  (t: TFunction, siteId: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const moderator = item as unknown as Moderator

    switch (column.type) {
      case "account":
        return renderUser(moderator.user.id, moderator.user.nickname)

      case "banned":
        return moderator.bannedTill !== 0 ? moderator.bannedTill : ""

      case "actions":
        return (
          <div className="flex justify-end">
            <Link
              to={routes.governance.createReferendum(siteId)}
              state={{
                parentBreadcrumbs: [{ path: routes.moderation.moderators(siteId), title: t("title") }],
                title: `Remove moderator "${truncate(moderator.user.nickname ?? moderator.user.id, { length: 45 })}"`,
                type: "site-moderator-removal",
                moderators: [moderator.user],
                redirectAfterProposalCreation: routes.moderation.moderators(siteId, "proposals"),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.moderators(siteId),
              }}
            >
              <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
            </Link>
          </div>
        )
    }
  }
