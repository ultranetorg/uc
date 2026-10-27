import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"
import { truncate } from "lodash"

import { useIsOperationVisibleForCreator, useParams, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useCategoryContextThreeDotsMenu = (categoryId: string, categoryTitle: string) => {
  const location = useLocation()
  const { t } = useTranslation("moderatorCategoryMenu")
  const { categoryId: paramCategoryId } = useParams()
  const storeId = useResolveStoreId()
  const isOperationVisibleForCreator = useIsOperationVisibleForCreator()

  const isAvatarChangeVisible = isOperationVisibleForCreator("category-avatar-change")
  const isTypeChangeVisible = isOperationVisibleForCreator("category-type-change")
  const isMovementVisible = isOperationVisibleForCreator("category-movement")
  const isCreationVisible = isOperationVisibleForCreator("category-creation")
  const isDeletionVisible = isOperationVisibleForCreator("category-deletion")

  return useMemo(() => {
    const changeItems = [
      ...(isAvatarChangeVisible
        ? [
            {
              label: t("moderatorCategoryMenu:avatarChange"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: truncate(`Change category "${categoryTitle}" avatar`, { length: 64 }),
                type: "category-avatar-change",
                categoryId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        : []),
      ...(isTypeChangeVisible
        ? [
            {
              label: t("moderatorCategoryMenu:typeChange"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: truncate(`Change category "${categoryTitle}" type`, { length: 64 }),
                type: "category-type-change",
                categoryId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        : []),
      ...(isMovementVisible
        ? [
            {
              label: t("moderatorCategoryMenu:move"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: truncate(`Move "${categoryTitle}" category`, { length: 64 }),
                type: "category-movement",
                categoryId,
                parentBreadcrumbs: [
                  { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
                  { path: routes.moderation.publications(storeId!), title: t("common:publications") },
                ],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
                redirectAfterProposalExecution:
                  paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
              },
            },
          ]
        : []),
      ...(isCreationVisible
        ? [
            {
              label: t("moderatorCategoryMenu:categoryCreate"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: "Create new category",
                type: "category-creation",
                categoryId,
                parentBreadcrumbs: [
                  { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
                  { path: routes.moderation.publications(storeId!), title: t("common:publications") },
                ],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        : []),
    ]

    const deletionItems = isDeletionVisible
      ? [
          {
            label: t("moderatorCategoryMenu:remove"),
            to: routes.moderation.createProposal(storeId!),
            state: {
              title: truncate(`Remove category "${categoryTitle}"`, { length: 64 }),
              type: "category-deletion",
              categoryId,
              parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
              redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
              redirectAfterProposalExecution:
                paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
            },
          },
        ]
      : []

    return [
      ...changeItems,
      ...(changeItems.length > 0 && deletionItems.length > 0 ? [{ separator: true }] : []),
      ...deletionItems,
    ]
  }, [
    categoryId,
    categoryTitle,
    isAvatarChangeVisible,
    isCreationVisible,
    isDeletionVisible,
    isMovementVisible,
    isTypeChangeVisible,
    location.pathname,
    paramCategoryId,
    storeId,
    t,
  ])
}
