import { memo, useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"
import { FloatingPortal } from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { useSiteRolesContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { categoriesKeys } from "entities"
import { useResolveSiteId, useScrollOrResize, useSubmenu } from "hooks"
import { PropsWithClassName } from "types"
import { SimpleMenu } from "ui/components"
import { routes } from "utils"

export const ModeratorSiteMenu = memo(({ className }: PropsWithClassName) => {
  const location = useLocation()
  const { isModerator } = useSiteRolesContext()
  const siteId = useResolveSiteId()
  const { t } = useTranslation()

  const menu = useSubmenu({ placement: "bottom-end" })
  useScrollOrResize(() => menu.setOpen(false), menu.isOpen)

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: routes.moderation.create(siteId!),
        state: {
          title: "Create category",
          type: "category-creation",
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(siteId!), title: t("common:proposals") },
            { path: routes.moderation.publications(siteId!), title: t("common:publications") },
          ],
          categoryId: null,
          redirectAfterProposalCreation: routes.moderation.proposals(siteId!),
          redirectAfterProposalExecution: location.pathname,
          invalidateQueryKeys: categoriesKeys.all(siteId!),
        },
      },
    ],
    [location.pathname, siteId, t],
  )

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  if (!isModerator) {
    return null
  }

  return (
    <>
      <div
        className={twMerge(
          "group box-border flex size-10 cursor-pointer items-center justify-center rounded border border-gray-300 bg-gray-100 hover:border-gray-400",
          menu.isOpen && "border-gray-400",
          className,
        )}
        ref={menu.refs.setReference}
        {...menu.getReferenceProps()}
      >
        <SvgThreeDotsSm
          className={twMerge("size-6 fill-gray-500 group-hover:fill-gray-800", menu.isOpen && "fill-gray-800")}
        />
      </div>
      {menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            items={menuItems}
            style={menu.floatingStyles}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
