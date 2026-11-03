import { useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreRolesContext } from "app"
import { useCanSeeOperation, useStoreChangeMenuItems } from "hooks"
import { OperationType } from "types"
import { SimpleMenuItem } from "ui/components"
import { routes } from "utils"

const storeOperations: OperationType[] = ["store-renaming", "store-info-updation", "store-avatar-change"]

const userOperations: OperationType[] = ["user-registration", "user-unregistration"]

const categoriesOperations: OperationType[] = [
  "category-creation",
  "category-deletion",
  "category-movement",
  "category-type-change",
  "category-avatar-change",
]

const publicationsOperations: OperationType[] = [
  "publication-creation",
  "publication-deletion",
  "publication-publish",
  "publication-unpublish",
  "publication-updation",
]

const reviewsOperations: OperationType[] = ["review-creation", "review-edit", "review-deletion"]

export const useStoreDropdownButtonMenuItems = (storeId: string): SimpleMenuItem[] => {
  const { t } = useTranslation("storeDropdownMenu")
  const { isPublisher, isModerator } = useStoreRolesContext()

  const storeItems = useStoreChangeMenuItems(storeId, { isPublisher, isModerator })
  const canSeeOperation = useCanSeeOperation()

  const isModeratorsVisible = canSeeOperation("store-moderator-addition")
  const isPublishersVisible = canSeeOperation("store-authors-removal")
  const isUsersVisible = userOperations.some(canSeeOperation)

  const isStoreOperationsVisible = storeOperations.some(canSeeOperation)
  const isCategoriesVisible = categoriesOperations.some(canSeeOperation)
  const isPublicationsVisible = publicationsOperations.some(canSeeOperation)
  const isReviewsVisible = reviewsOperations.some(canSeeOperation)

  const menuItems = useMemo(
    () => [
      ...(isPublisher || isModerator
        ? [{ label: t("common:policySurveys"), to: routes.moderation.policySurveys(storeId) }]
        : []),
      ...(isModeratorsVisible || isPublishersVisible || isUsersVisible
        ? [
            { separator: true },
            ...(isModeratorsVisible
              ? [{ label: t("common:moderators"), to: routes.moderation.moderators(storeId) }]
              : []),
            ...(isPublishersVisible
              ? [{ label: t("common:publishers"), to: routes.moderation.publishers(storeId) }]
              : []),
            ...(isUsersVisible ? [{ label: t("common:users"), to: routes.moderation.users(storeId) }] : []),
          ]
        : []),
      ...(isStoreOperationsVisible || isCategoriesVisible || isPublicationsVisible || isReviewsVisible
        ? [
            { separator: true },
            ...(isStoreOperationsVisible
              ? [{ label: t("common:settings"), to: routes.moderation.store(storeId) }]
              : []),
            ...(isCategoriesVisible
              ? [{ label: t("common:categories"), to: routes.moderation.categories(storeId) }]
              : []),
            ...(isPublicationsVisible
              ? [{ label: t("common:publications"), to: routes.moderation.publications(storeId) }]
              : []),
            ...(isReviewsVisible ? [{ label: t("common:reviews"), to: routes.moderation.reviews(storeId) }] : []),
          ]
        : []),
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [
      isCategoriesVisible,
      isModerator,
      isModeratorsVisible,
      isPublicationsVisible,
      isPublisher,
      isPublishersVisible,
      isReviewsVisible,
      isStoreOperationsVisible,
      isUsersVisible,
      storeId,
      storeItems,
      t,
    ],
  )

  return menuItems
}
