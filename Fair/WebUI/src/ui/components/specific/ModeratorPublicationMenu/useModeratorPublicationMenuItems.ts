import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { unpublishedPublicationsKeys } from "entities"
import { useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useModeratorPublicationMenuItems = (
  publicationId: string,
  publicationTitle?: string,
  isFromContextMenu: boolean = false,
) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("moderatorPublicationMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unpublishPublication"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: publicationTitle
            ? `Unpublish publication "${truncate(publicationTitle, { length: 40 })}"`
            : "Unpublish publication",
          type: "publication-unpublish",
          publicationId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.publications(storeId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.publications(storeId!),
          redirectAfterProposalExecution: isFromContextMenu ? location.pathname : routes.store(storeId!),
          invalidateQueryKeys: unpublishedPublicationsKeys.all(storeId!),
        },
      },
      { separator: true },
      {
        label: t("removePublication"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: publicationTitle
            ? `Remove publication "${truncate(publicationTitle, { length: 43 })}"`
            : "Remove publication",
          type: "publication-deletion",
          publicationId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.publications(storeId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.publications(storeId!),
          redirectAfterProposalExecution: isFromContextMenu ? location.pathname : routes.store(storeId!),
        },
      },
    ],
    [isFromContextMenu, publicationId, publicationTitle, storeId, t],
  )

  return { menuItems }
}
