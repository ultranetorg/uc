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

import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"

import { useUserCommentMenuItems } from "./useUserCommentMenuItems"

type UserCommentContextMenuBaseProps = {
  commentId: string
}

export type UserCommentContextMenuProps = UserCommentContextMenuBaseProps

export const UserCommentContextMenu = memo(({ commentId }: UserCommentContextMenuProps) => {
  const { menuItems } = useUserCommentMenuItems(commentId)

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

  return (
    <>
      <div ref={refs.setReference} {...getReferenceProps()}>
        <SvgThreeDotsVertical className="cursor-pointer fill-gray-500 hover:fill-gray-800" />
      </div>
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu
            ref={refs.setFloating}
            className="w-auto"
            items={menuItems}
            style={floatingStyles}
            onClick={handleMenuClick}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
