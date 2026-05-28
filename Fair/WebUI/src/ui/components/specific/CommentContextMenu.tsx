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

import { useModerationContext, useUserContext } from "app"
import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize } from "hooks"
import { CommentContextMenuProps, SimpleMenu, SimpleMenuItem } from "ui/components"

import { useModeratorUserMenuItems } from "./ModeratorUserMenu/useModeratorUserMenuItems"

type CommentContextMenuBaseProps = {
  onEditReview: (id: string, text: string) => void
}

export type CommentContextMenu = CommentContextMenuProps & CommentContextMenuBaseProps

export const CommentContextMenu = memo(({ id, text, reviewerId, reviewerName, onEditReview }: CommentContextMenu) => {
  const { isModerator } = useModerationContext()
  const { user } = useUserContext()
  const { menuItems: moderatorMenuItems } = useModeratorUserMenuItems(reviewerId, reviewerName)

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

  const canEdit = user && user.id === reviewerId

  const menuItems = useMemo<SimpleMenuItem[]>(
    () => [
      ...(canEdit
        ? [
            {
              onClick: handleEditClick,
              label: t("common:edit"),
            },
          ]
        : []),
      ...(isModerator ? [...(canEdit ? [{ separator: true }] : []), ...moderatorMenuItems] : []),
    ],
    [canEdit, handleEditClick, isModerator, moderatorMenuItems],
  )

  const handleMenuClick = useCallback(() => setExpanded(false), [])

  if (!menuItems.length) return null

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
