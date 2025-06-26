import { memo, useCallback, useEffect, useState } from "react"
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

import { DropdownButton, SimpleMenu, SimpleMenuItem } from "ui/components"
import { PropsWithClassName } from "types"

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

  useEffect(() => {
    const handler = () => setExpanded(false)
    window.addEventListener("resize", handler)
    window.addEventListener("scroll", handler, true)
    return () => {
      window.removeEventListener("resize", handler)
      window.removeEventListener("scroll", handler, true)
    }
  }, [])

  return (
    <>
      <div
        ref={refs.setReference}
        className={twMerge(
          "flex cursor-pointer items-center text-2sm font-medium leading-6 text-gray-800 hover:font-semibold",
          className,
          isExpanded && "font-semibold",
        )}
        {...getReferenceProps()}
      >
        {label} <DropdownButton />
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
