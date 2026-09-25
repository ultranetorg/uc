import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { SimpleMenuItem } from "ui/components"
import { routes } from "utils"

export const useCategoryCreationMenuItem = (storeId: string): SimpleMenuItem => {
  const location = useLocation()
  const { t } = useTranslation()

  return useMemo(
    () => ({
      label: t("moderatorCategoryMenu:categoryCreate"),
      to: routes.moderation.createProposal(storeId),
      state: {
        title: "Create category",
        type: "category-creation",
        parentBreadcrumbs: [
          { path: routes.moderation.proposals(storeId), title: t("common:proposals") },
          { path: routes.moderation.publications(storeId), title: t("common:publications") },
        ],
        categoryId: null,
        redirectAfterProposalCreation: routes.moderation.proposals(storeId),
        redirectAfterProposalExecution: location.pathname,
      },
    }),
    [location.pathname, storeId, t],
  )
}
