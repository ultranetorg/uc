import { memo, useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
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

import { useModerationContext } from "app"
import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize } from "hooks"
import { SimpleMenu } from "ui/components"
import { PropsWithClassName } from "types"

type ModeratorOptionsMenuBaseProps = {
  publicationId: string
  publicationTitle?: string
}

export type ModeratorOptionsMenuProps = PropsWithClassName & ModeratorOptionsMenuBaseProps

export const ModeratorOptionsMenu = memo(
  ({ className, publicationId, publicationTitle }: ModeratorOptionsMenuProps) => {
    const { isModerator } = useModerationContext()
    const { siteId } = useParams()
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

    const menuItems = useMemo(
      () => [
        {
          label: t("unpublishPublication"),
          to: `/${siteId}/m/new`,
          state: {
            title: publicationTitle ? `Unpublish publication "${publicationTitle}"` : "Unpublish publication",
            type: "publication-unpublish",
            publicationId,
            parentBreadcrumbs: [
              { path: `/${siteId}/m/`, title: t("common:proposals") },
              { path: `/${siteId}/m/c/`, title: t("common:publications") },
            ],
            previousPath: `/${siteId}/m/c/`,
          },
        },
        {
          separator: true,
        },
        {
          label: t("removePublication"),
          to: `/${siteId}/m/new`,
          state: {
            title: publicationTitle ? `Remove publication "${publicationTitle}"` : "Remove publication",
            type: "publication-deletion",
            publicationId,
            parentBreadcrumbs: [
              { path: `/${siteId}/m/`, title: t("common:proposals") },
              { path: `/${siteId}/m/c/`, title: t("common:publications") },
            ],
            previousPath: `/${siteId}/m/c/`,
          },
        },
      ],
      [publicationId, publicationTitle, siteId, t],
    )

    const handleMenuClick = useCallback(() => setExpanded(false), [])

    if (!isModerator) {
      return null
    }

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
  },
)
