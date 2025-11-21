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

import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"
import { PropsWithClassName } from "types"

type ModeratorPublicationMenuBaseProps = {
  publicationId: string
}

export type ModeratorPublicationMenuProps = PropsWithClassName & ModeratorPublicationMenuBaseProps

export const ModeratorPublicationMenu = memo(({ className, publicationId }: ModeratorPublicationMenuProps) => {
  const { t } = useTranslation("moderatorPublicationMenu")

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

  return (
    <>
      <div className={className} ref={refs.setReference} {...getReferenceProps()}>
        <SvgThreeDotsSm className="hove:opacity-80 size-5 cursor-pointer rounded bg-white fill-gray-500 opacity-50 hover:fill-gray-800" />
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
