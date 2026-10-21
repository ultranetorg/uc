import { useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { useStoreChangeMenuItems } from "hooks"
import { OperationType } from "types"
import { SimpleMenuItem } from "ui/components"
import {
  isModeratorCanCreate,
  isModeratorCanVote,
  isPublisherCanCreate,
  isPublisherCanVote,
  isVotingRequired,
  routes,
} from "utils"

const operationTypesWithoutSection: OperationType[] = [
  "category-creation",
  "category-deletion",
  "category-type-change",
  "category-movement",
  "category-avatar-change",
  "store-renaming",
  "store-info-updation",
  "store-avatar-change",
]

const userOperations: OperationType[] = ["user-registration", "user-unregistration"]

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
  const { policies } = useStorePoliciesContext()
  const { store } = useStoreContext()
  const { isPublisher, isModerator } = useStoreRolesContext()

  const storeItems = useStoreChangeMenuItems(storeId, { isPublisher, isModerator })

  const isModeratorShouldVote = (operation: OperationType) =>
    isModeratorCanVote(operation, policies) && isVotingRequired(operation, store, policies)

  const isPublisherShouldVote = (operation: OperationType) =>
    isPublisherCanVote(operation, policies) && isVotingRequired(operation, store, policies)

  const isOperationVisibleForStaff = (operation: OperationType) =>
    (isModerator &&
      (isModeratorShouldVote(operation) ||
        isPublisherCanCreate(operation, policies) ||
        isModeratorCanCreate(operation, policies))) ||
    (isPublisher && (isPublisherShouldVote(operation) || isPublisherCanCreate(operation, policies)))

  const isReferendumsVisible = operationTypesWithoutSection.some(isOperationVisibleForStaff)
  const isProposalsVisible = operationTypesWithoutSection.some(isOperationVisibleForStaff)

  const isModeratorsVisible = isOperationVisibleForStaff("store-moderator-addition")
  const isPublishersVisible = isOperationVisibleForStaff("store-authors-removal")
  const isUsersVisible = userOperations.some(isOperationVisibleForStaff)

  const isPublicationsVisible = publicationsOperations.some(isOperationVisibleForStaff)
  const isReviewsVisible = reviewsOperations.some(isOperationVisibleForStaff)

  const menuItems = useMemo(
    () => [
      { label: t("common:policySurveys"), to: routes.moderation.policySurveys(storeId) },
      ...(isReferendumsVisible || isProposalsVisible
        ? [
            { separator: true },
            ...(isReferendumsVisible
              ? [{ label: t("common:referendums"), to: routes.moderation.referendums(storeId) }]
              : []),
            ...(isProposalsVisible ? [{ label: t("common:proposals"), to: routes.moderation.proposals(storeId) }] : []),
          ]
        : []),
      ...(isModeratorsVisible || isPublishersVisible || isUsersVisible
        ? [
            { separator: true },
            ...(isPublishersVisible
              ? [{ label: t("common:publishers"), to: routes.moderation.publishers(storeId) }]
              : []),
            ...(isModeratorsVisible
              ? [{ label: t("common:moderators"), to: routes.moderation.moderators(storeId) }]
              : []),
            ...(isUsersVisible ? [{ label: t("common:users"), to: routes.moderation.users(storeId) }] : []),
          ]
        : []),
      ...(isPublicationsVisible || isReviewsVisible
        ? [
            { separator: true },
            ...(isPublicationsVisible
              ? [{ label: t("common:publications"), to: routes.moderation.publications(storeId) }]
              : []),
            ...(isReviewsVisible ? [{ label: t("common:reviews"), to: routes.moderation.reviews(storeId) }] : []),
          ]
        : []),
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [
      isModeratorsVisible,
      isProposalsVisible,
      isPublicationsVisible,
      isPublishersVisible,
      isReferendumsVisible,
      isReviewsVisible,
      isUsersVisible,
      storeId,
      storeItems,
      t,
    ],
  )

  return menuItems
}
