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
import { useTranslation } from "react-i18next"

import { twMerge } from "tailwind-merge"
import { useUserContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"
import { PropsWithClassName } from "types"

type ContextMenuButtonSize = "medium" | "large"

type ModeratorPublicationContextMenuBaseProps = {
  publicationId: string
  size?: ContextMenuButtonSize
}

export type ModeratorPublicationContextMenuProps = PropsWithClassName & ModeratorPublicationContextMenuBaseProps

export const ModeratorPublicationContextMenu = memo(
  ({ className, publicationId, size = "medium" }: ModeratorPublicationContextMenuProps) => {
    const { t } = useTranslation("moderatorPublicationContextMenu")
    const { isModerator } = useUserContext()

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

    const handleRemovePublication = useCallback(() => alert("Remove publication" + publicationId), [publicationId])

    const menuItems = useMemo(
      () => [
        {
          onClick: handleRemovePublication,
          label: t("removePublication"),
        },
      ],
      [handleRemovePublication, t],
    )

    const handleMenuClick = useCallback(() => setExpanded(false), [])

    if (!isModerator) {
      return null
    }

    return (
      <>
        <div className={className} ref={refs.setReference} {...getReferenceProps()}>
          <SvgThreeDotsSm
            className={twMerge(
              "hove:opacity-80 size-5 cursor-pointer rounded bg-white fill-gray-500 opacity-50 hover:fill-gray-800",
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
