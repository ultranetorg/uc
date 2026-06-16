import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation, useParams } from "react-router-dom"
import { truncate } from "lodash"

import { categoriesKeys } from "entities"

export const useModeratorCategoryMenuItems = (categoryId: string, categoryTitle: string) => {
  const location = useLocation()
  const { t } = useTranslation("moderatorCategoryMenu")
  const { siteId, categoryId: paramCategoryId } = useParams()

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:avatarChange"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 39 })}" avatar`,
          type: "category-avatar-change",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          redirectAfterProposalCreation: `/${siteId}/m/`,
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:typeChange"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 41 })}" type`,
          type: "category-type-change",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          redirectAfterProposalCreation: `/${siteId}/m/`,
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:move"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Move "${truncate(categoryTitle, { length: 48 })}" category`,
          type: "category-movement",
          categoryId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          redirectAfterProposalCreation: `/${siteId}/m/`,
          redirectAfterProposalExecution: paramCategoryId === categoryId ? `/${siteId}` : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: `/${siteId}/m/new`,
        state: {
          title: "Create new category",
          type: "category-creation",
          categoryId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          redirectAfterProposalCreation: `/${siteId}/m/`,
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
      {
        separator: true,
      },
      {
        label: t("moderatorCategoryMenu:remove"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Remove category "${truncate(categoryTitle, { length: 46 })}"`,
          type: "category-deletion",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          redirectAfterProposalCreation: `/${siteId}/m/`,
          redirectAfterProposalExecution: paramCategoryId === categoryId ? `/${siteId}` : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
    ],
    [t, siteId, categoryTitle, categoryId, location.pathname, paramCategoryId],
  )

  return { menuItems }
}
