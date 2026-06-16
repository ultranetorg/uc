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
import { twMerge } from "tailwind-merge"

import { useSiteRolesContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { PropsWithClassName } from "types"
import { SimpleMenu } from "ui/components"

import { useModeratorPublicationMenuItems } from "./useModeratorPublicationMenuItems"

type ContextMenuButtonSize = "medium" | "large"

type ModeratorPublicationContextMenuBaseProps = {
  publicationId: string
  publicationTitle?: string
  size?: ContextMenuButtonSize
}

export type ModeratorPublicationContextMenuProps = PropsWithClassName & ModeratorPublicationContextMenuBaseProps

export const ModeratorPublicationContextMenu = memo(
  ({ className, publicationId, publicationTitle, size = "medium" }: ModeratorPublicationContextMenuProps) => {
    const { isModerator } = useSiteRolesContext()
    const { menuItems } = useModeratorPublicationMenuItems(publicationId, publicationTitle, true)

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

    if (!isModerator) {
      return null
    }

    return (
      <>
        <div className={className} ref={refs.setReference} {...getReferenceProps()}>
          <SvgThreeDotsSm
            className={twMerge(
              "size-5 cursor-pointer rounded bg-white fill-gray-500 opacity-50 hover:fill-gray-800 hover:opacity-80",
              size === "large" && "size-8",
            )}
          />
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
  },
)
