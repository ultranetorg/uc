import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useCanCreateOperation, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const usePublicationCardContextThreeDotsMenu = (
  publicationId: string,
  publicationTitle?: string,
  isFromContextMenu: boolean = false,
) => {
  const { t } = useTranslation("moderatorPublicationMenu")

  const canCreateOperation = useCanCreateOperation()
  const storeId = useResolveStoreId()

  const isUnpublishVisible = canCreateOperation("publication-unpublish")
  const isDeletionVisible = canCreateOperation("publication-deletion")

  return useMemo(
    () => [
      ...(isUnpublishVisible
        ? [
            {
              label: t("unpublishPublication"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: publicationTitle
                  ? truncate(`Unpublish publication "${publicationTitle}"`, { length: 64 })
                  : "Unpublish publication",
                type: "publication-unpublish",
                publicationId,
                parentBreadcrumbs: [
                  { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
                  { path: routes.moderation.publications(storeId!), title: t("common:publications") },
                ],
                redirectAfterProposalCreation: routes.moderation.publications(storeId!),
                redirectAfterProposalExecution: isFromContextMenu ? location.pathname : routes.store(storeId!),
              },
            },
          ]
        : []),
      ...(isUnpublishVisible && isDeletionVisible ? [{ separator: true }] : []),
      ...(isDeletionVisible
        ? [
            {
              label: t("removePublication"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: publicationTitle
                  ? truncate(`Remove publication "${publicationTitle}"`, { length: 64 })
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
          ]
        : []),
    ],
    [isDeletionVisible, isFromContextMenu, isUnpublishVisible, publicationId, publicationTitle, storeId, t],
  )
}
