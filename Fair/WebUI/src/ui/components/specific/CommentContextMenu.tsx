import { memo, useCallback, useMemo, useState } from "react"
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
import { t } from "i18next"

import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize } from "hooks"
import { CommentContextMenuProps, SimpleMenu } from "ui/components"

type CommentContextMenuBaseProps = {
  onEditReview: (id: string, text: string) => void
}

export type CommentContextMenu = CommentContextMenuProps & CommentContextMenuBaseProps

export const CommentContextMenu = memo(({ id, text, onEditReview }: CommentContextMenu) => {
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

  const handleEditClick = useCallback(() => {
    setExpanded(false)
    onEditReview(id, text)
  }, [id, onEditReview, text])

  const menuItems = useMemo(
    () => [
      {
        onClick: handleEditClick,
        label: t("common:edit"),
      },
    ],
    [handleEditClick],
  )

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
