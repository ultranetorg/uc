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

import { useSiteRolesContext, useUserContext } from "app"
import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize } from "hooks"
import { PropsWithClassName } from "types"
import { SimpleMenu } from "ui/components"

import { useModeratorUserMenuItems } from "./useModeratorUserMenuItems"

type ModeratorUserMenuBaseProps = {
  userId: string
  userName: string
}

export type ModeratorUserMenuProps = PropsWithClassName & ModeratorUserMenuBaseProps

export const ModeratorUserMenu = memo(({ userId, userName }: ModeratorUserMenuProps) => {
  const { isModerator } = useSiteRolesContext()
  const { menuItems } = useModeratorUserMenuItems(userId, userName)
  const { user } = useUserContext()

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

  if (!isModerator || userId === user!.id) {
    return null
  }

  return (
    <>
      <div ref={refs.setReference} {...getReferenceProps()}>
        <SvgThreeDotsVertical className="size-8 cursor-pointer rounded fill-gray-800 opacity-50 hover:opacity-100" />
      </div>
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu
            ref={refs.setFloating}
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
