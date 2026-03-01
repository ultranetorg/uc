import { memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { FloatingPortal } from "@floating-ui/react"

import { SvgChevronDown2Sm } from "assets"
import { useSubmenu } from "hooks"
import { PropsWithClassName } from "types"

import { SimpleMenu, SimpleMenuItem } from "./SimpleMenu"

type ComboButtonBaseProps = {
  items: SimpleMenuItem[]
  label: string
  onButtonClick?: () => void
}

export type ComboButtonProps = PropsWithClassName & ComboButtonBaseProps

export const ComboButton = memo(({ items, label, onButtonClick }: ComboButtonProps) => {
  const menu = useSubmenu({ placement: "bottom-end", offset: 4, setFloatSizeAsReference: items && items.length < 8 })

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  return (
    <>
      <div
        className={twMerge("group flex gap-0.5", onButtonClick ? "cursor-pointer" : "cursor-default")}
        ref={menu.refs.setReference}
        {...menu.getReferenceProps()}
      >
        <button
          className={twMerge(
            "rounded-s bg-gray-800 px-4 py-3 text-2sm leading-5 text-white",
            onButtonClick ? "cursor-pointer" : "cursor-default",
          )}
        >
          {label}
        </button>
        <div className="flex size-11 cursor-pointer items-center justify-center rounded-e bg-gray-800 group-hover:bg-gray-950">
          <SvgChevronDown2Sm className="stroke-white" />
        </div>
      </div>
      {items && menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            style={menu.floatingStyles}
            items={items}
            menuItemClassName="h-11 w-full leading-7"
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
