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

import { TEST_CATEGORIES_ITEMS } from "testConstants"
import { DropdownButton, HeadingsMenu } from "ui/components"
import { PropsWithClassName } from "types"

type ExtendedCategoriesDropdownButtonBaseProps = {
  label: string
}

export type ExtendedCategoriesDropdownButtonProps = PropsWithClassName & ExtendedCategoriesDropdownButtonBaseProps

export const ExtendedCategoriesDropdownButton = memo(({ className, label }: ExtendedCategoriesDropdownButtonProps) => {
  const [isExpanded, setExpanded] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [
      offset({
        mainAxis: 24,
        crossAxis: -202,
      }),
    ],
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
        )}
        {...getReferenceProps()}
      >
        {label} <DropdownButton />
      </div>
      {isExpanded && (
        <FloatingPortal>
          <HeadingsMenu
            items={TEST_CATEGORIES_ITEMS}
            ref={refs.setFloating}
            style={floatingStyles}
            onClick={handleMenuClick}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
