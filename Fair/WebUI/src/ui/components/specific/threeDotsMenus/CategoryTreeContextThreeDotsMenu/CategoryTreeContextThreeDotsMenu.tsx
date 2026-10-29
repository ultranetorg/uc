import { memo, useCallback, useState } from "react"
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

import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"
import { PropsWithClassName } from "types"

import { useCategoryContextThreeDotsMenu } from "./useCategoryTreeContextThreeDotsMenu"

type CategoryTreeContextThreeDotsMenuBaseProps = {
  categoryId: string
  categoryTitle: string
}

export type CategoryTreeContextThreeDotsMenuProps = PropsWithClassName & CategoryTreeContextThreeDotsMenuBaseProps

export const CategoryTreeContextThreeDotsMenu = memo(
  ({ className, categoryId, categoryTitle }: CategoryTreeContextThreeDotsMenuProps) => {
    const menuItems = useCategoryContextThreeDotsMenu(categoryId, categoryTitle)

    const [isExpanded, setExpanded] = useState(false)

    useScrollOrResize(() => setExpanded(false), isExpanded)

    const { context, floatingStyles, refs } = useFloating({
      middleware: [offset(4)],
      open: isExpanded,
      placement: "bottom-end",
      onOpenChange: setExpanded,
    })
    const dismiss = useDismiss(context)
    const hover = useHover(context, { handleClose: safePolygon() })
    const role = useRole(context)
    const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

    const handleMenuClick = useCallback(() => setExpanded(false), [])

    if (menuItems.length === 0) {
      return null
    }

    return (
      <>
        <div className={className} ref={refs.setReference} {...getReferenceProps()}>
          <SvgThreeDotsSm className="size-5 cursor-pointer rounded bg-white fill-gray-500 opacity-50 hover:fill-gray-800 hover:opacity-80" />
        </div>
        {isExpanded && (
          <FloatingPortal>
            <SimpleMenu
              ref={refs.setFloating}
              menuItemClassName="w-auto"
              items={menuItems}
              style={floatingStyles}
              onClick={handleMenuClick}
              {...getFloatingProps()}
            />
          </FloatingPortal>
        )}
      </>
    )
  },
)
