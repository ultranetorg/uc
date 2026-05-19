import { useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { FloatingPortal } from "@floating-ui/react"

import { useCloseOnScrollOrResize, useSubmenu } from "hooks"
import { PropsWithClassName } from "types"
import { ChevronDownButton, SimpleMenu, SimpleMenuItem } from "ui/components"

import { MENU_ITEM_STYLE } from "./styles"

type HeaderDropdownButtonBaseProps = {
  label: string
  menuItems: SimpleMenuItem[]
}

export type HeaderDropdownButtonProps = PropsWithClassName & HeaderDropdownButtonBaseProps

export const HeaderDropdownButton = ({ className, label, menuItems }: HeaderDropdownButtonProps) => {
  const menu = useSubmenu({
    placement: "bottom-end",
  })

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  useCloseOnScrollOrResize(() => menu.setOpen(false))

  return (
    <>
      <div
        ref={menu.refs.setReference}
        className={twMerge(MENU_ITEM_STYLE, "flex items-center", className, menu.isOpen && "font-semibold")}
        {...menu.getReferenceProps()}
      >
        {label} <ChevronDownButton />
      </div>
      {menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            items={menuItems}
            multiColumnMenu={false}
            style={menu.floatingStyles}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
}
