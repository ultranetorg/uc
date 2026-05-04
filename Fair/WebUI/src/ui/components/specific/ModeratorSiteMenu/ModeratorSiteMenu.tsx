import { memo, useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { FloatingPortal } from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { useModerationContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"
import { PropsWithClassName } from "types"
import { SimpleMenu } from "ui/components"

export const ModeratorSiteMenu = memo(({ className }: PropsWithClassName) => {
  const { isModerator } = useModerationContext()
  const { siteId } = useParams()
  const { t } = useTranslation()

  const menu = useSubmenu({ placement: "bottom-end" })
  useScrollOrResize(() => menu.setOpen(false), menu.isOpen)

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: `/${siteId}/m/new`,
        state: {
          title: "Create new category",
          type: "category-creation",
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/`,
        },
      },
    ],
    [siteId, t],
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
