import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useCanCreateOperation } from "hooks"
import { routes } from "utils"

export const useStoreThreeDotsMenu = (storeId: string) => {
  const location = useLocation()
  const { t } = useTranslation()

  const canCreateOperation = useCanCreateOperation()

  const isCategoryCreationVisible = canCreateOperation("category-creation")

  return useMemo(
    () =>
      isCategoryCreationVisible
        ? [
            {
              label: t("moderatorCategoryMenu:categoryCreate"),
              to: routes.moderation.createProposal(storeId),
              state: {
                title: "Create category",
                type: "category-creation",
                parentBreadcrumbs: [{ path: routes.moderation.categories(storeId), title: t("common:categories") }],
                categoryId: null,
                redirectAfterProposalCreation: routes.moderation.categories(storeId, "creations"),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        : [],
    [isCategoryCreationVisible, location.pathname, storeId, t],
  )
}
