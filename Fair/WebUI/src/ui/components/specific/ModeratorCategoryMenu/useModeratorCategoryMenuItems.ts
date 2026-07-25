import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"
import { truncate } from "lodash"

import { categoriesKeys } from "entities"
import { useParams, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useModeratorCategoryMenuItems = (categoryId: string, categoryTitle: string) => {
  const location = useLocation()
  const { t } = useTranslation("moderatorCategoryMenu")
  const { categoryId: paramCategoryId } = useParams()
  const storeId = useResolveStoreId()

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:avatarChange"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 39 })}" avatar`,
          type: "category-avatar-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(storeId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:typeChange"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 41 })}" type`,
          type: "category-type-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(storeId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:move"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: `Move "${truncate(categoryTitle, { length: 48 })}" category`,
          type: "category-movement",
          categoryId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.publications(storeId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(storeId!),
        },
      },
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
          invalidateQueryKeys: categoriesKeys.all(storeId!),
        },
      },
      {
        separator: true,
      },
      {
        label: t("moderatorCategoryMenu:remove"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: `Remove category "${truncate(categoryTitle, { length: 46 })}"`,
          type: "category-deletion",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(storeId!),
        },
      },
    ],
    [t, storeId, categoryTitle, categoryId, location.pathname, paramCategoryId],
  )

  return { menuItems }
}
