import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { unpublishedPublicationsKeys } from "entities"
import { useResolveSiteId } from "hooks"
import { routes } from "utils"

export const useModeratorPublicationMenuItems = (
  publicationId: string,
  publicationTitle?: string,
  isFromContextMenu: boolean = false,
) => {
  const siteId = useResolveSiteId()
  const { t } = useTranslation("moderatorPublicationMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unpublishPublication"),
        to: routes.moderation.create(siteId!),
        state: {
          title: publicationTitle
            ? `Unpublish publication "${truncate(publicationTitle, { length: 40 })}"`
            : "Unpublish publication",
          type: "publication-unpublish",
          publicationId,
          parentBreadcrumbs: [
            { path: routes.moderation.root(siteId!), title: t("common:proposals") },
            { path: routes.moderation.publications(siteId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.publications(siteId!),
          redirectAfterProposalExecution: isFromContextMenu ? location.pathname : routes.site(siteId!),
          invalidateQueryKeys: unpublishedPublicationsKeys.all(siteId!),
        },
      },
      { separator: true },
      {
        label: t("removePublication"),
        to: routes.moderation.create(siteId!),
        state: {
          title: publicationTitle
            ? `Remove publication "${truncate(publicationTitle, { length: 43 })}"`
            : "Remove publication",
          type: "publication-deletion",
          publicationId,
          parentBreadcrumbs: [
            { path: routes.moderation.root(siteId!), title: t("common:proposals") },
            { path: routes.moderation.publications(siteId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.publications(siteId!),
          redirectAfterProposalExecution: isFromContextMenu ? location.pathname : routes.site(siteId!),
        },
      },
    ],
    [isFromContextMenu, publicationId, publicationTitle, siteId, t],
  )

  return { menuItems }
}
