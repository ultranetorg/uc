import { memo, useCallback, useState } from "react"
import { twMerge } from "tailwind-merge"
import {
  FloatingPortal,
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

import { useCloseOnScrollOrResize } from "hooks"
import { ChevronDownButton, SimpleMenu, SimpleMenuItem } from "ui/components"
import { PropsWithClassName } from "types"

import { MENU_ITEM_STYLE } from "./styles"

type CategoriesDropdownButtonBaseProps = {
  label: string
  items?: SimpleMenuItem[]
}

export type CategoriesDropdownButtonProps = PropsWithClassName & CategoriesDropdownButtonBaseProps

export const CategoriesDropdownButton = memo(({ className, label, items }: CategoriesDropdownButtonProps) => {
  const [isExpanded, setExpanded] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(4)],
    open: isExpanded,
    placement: "bottom-start",
    onOpenChange: setExpanded,
  })

  const dismiss = useDismiss(context)
  const hover = useHover(context, { handleClose: safePolygon() })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

  const handleMenuClick = useCallback(() => setExpanded(false), [])

  useCloseOnScrollOrResize(() => setExpanded(false))

  return (
    <>
      <div
        ref={refs.setReference}
        className={twMerge(MENU_ITEM_STYLE, "flex items-center", className, isExpanded && "font-semibold")}
        {...getReferenceProps()}
      >
        {label} <ChevronDownButton />
      </div>
      {isExpanded && items && (
        <FloatingPortal>
          <SimpleMenu
            ref={refs.setFloating}
            items={items}
            style={floatingStyles}
            onClick={handleMenuClick}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
