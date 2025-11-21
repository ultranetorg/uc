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

import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"
import { PropsWithClassName } from "types"

type ModeratorOptionsMenuBaseProps = {
  publicationId: string
}

export type ModeratorOptionsMenuProps = PropsWithClassName & ModeratorOptionsMenuBaseProps

export const ModeratorOptionsMenu = memo(({ className, publicationId }: ModeratorOptionsMenuProps) => {
  const { t } = useTranslation("moderatorOptionsMenu")

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
      <div
        className={twMerge(
          "flex cursor-pointer items-center gap-2 rounded border border-gray-300 bg-gray-100 px-3 py-2 hover:border-gray-400",
          className,
        )}
        ref={refs.setReference}
        {...getReferenceProps()}
      >
        <span className="text-2xs font-medium leading-4">{t("label")}</span>
        <SvgThreeDotsSm className="size-6 fill-gray-800" />
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
