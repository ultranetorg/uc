import { memo, useCallback } from "react"
import { FloatingPortal } from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { useUserContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"
import { SimpleMenu } from "ui/components"

import { useModeratorCategoryMenuItems } from "./useModeratorCategoryMenuItems"

export type ModeratorCategoryMenu = {
  categoryId: string
}

export const ModeratorCategoryMenu = memo(({ categoryId }: ModeratorCategoryMenu) => {
  const { menuItems } = useModeratorCategoryMenuItems(categoryId)
  const { isModerator } = useUserContext()

  const menu = useSubmenu({ placement: "bottom-end" })
  useScrollOrResize(() => menu.setOpen(false), menu.isOpen)

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
