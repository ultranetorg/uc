import { memo, useCallback } from "react"
import { FloatingPortal } from "@floating-ui/react"
import { useSubmenu } from "hooks"

import { SvgChevronDown2Sm } from "assets"

import { SimpleMenu, SimpleMenuItem } from "./SimpleMenu"

export type DropdownButtonProps = {
  items: SimpleMenuItem[]
  label: string
  multiColumnMenu?: boolean
  menuClassName?: string
}

export const DropdownButton = memo(({ label, items, multiColumnMenu = true, menuClassName }: DropdownButtonProps) => {
  const menu = useSubmenu({
    placement: "bottom-end",
    offset: 4,
    setFloatSizeAsReference: !menuClassName && items && items.length < 8,
  })

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  return (
    <>
      <button
        className="flex items-center gap-2 rounded bg-gray-950 px-4 py-3 text-2sm leading-5 text-white"
        ref={menu.refs.setReference}
        {...menu.getReferenceProps()}
      >
        {label}
        <SvgChevronDown2Sm className="stroke-white" />
      </button>
      {items && menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            className={menuClassName}
            style={menu.floatingStyles}
            items={items}
            menuItemClassName="w-full"
            multiColumnMenu={multiColumnMenu}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
