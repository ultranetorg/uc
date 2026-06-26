import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"
import { truncate } from "lodash"

import { categoriesKeys } from "entities"
import { useParams, useResolveSiteId } from "hooks"
import { routes } from "utils"

export const useModeratorCategoryMenuItems = (categoryId: string, categoryTitle: string) => {
  const location = useLocation()
  const { t } = useTranslation("moderatorCategoryMenu")
  const { categoryId: paramCategoryId } = useParams()
  const siteId = useResolveSiteId()

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:avatarChange"),
        to: routes.moderation.createProposal(siteId!),
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 39 })}" avatar`,
          type: "category-avatar-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:typeChange"),
        to: routes.moderation.createProposal(siteId!),
        state: {
          title: `Change category "${truncate(categoryTitle, { length: 41 })}" type`,
          type: "category-type-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!), // TODO: update all except tree
        },
      },
      {
        label: t("moderatorCategoryMenu:move"),
        to: routes.moderation.createProposal(siteId!),
        state: {
          title: `Move "${truncate(categoryTitle, { length: 48 })}" category`,
          type: "category-movement",
          categoryId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(siteId!), title: t("common:proposals") },
            { path: routes.moderation.publications(siteId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.site(siteId!) : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: routes.moderation.createProposal(siteId!),
        state: {
          title: "Create new category",
          type: "category-creation",
          categoryId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(siteId!), title: t("common:proposals") },
            { path: routes.moderation.publications(siteId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
      {
        separator: true,
      },
      {
        label: t("moderatorCategoryMenu:remove"),
        to: routes.moderation.createProposal(siteId!),
        state: {
          title: `Remove category "${truncate(categoryTitle, { length: 46 })}"`,
          type: "category-deletion",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.site(siteId!) : location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
    ],
    [t, siteId, categoryTitle, categoryId, location.pathname, paramCategoryId],
  )

  return { menuItems }
}
