import { memo, useEffect, useState } from "react"
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

import { SvgSiteLogo } from "assets/fallback"
import { ChevronDownButton, ImageFallback, SimpleMenu } from "ui/components"
import { buildFileUrl } from "utils"
import { useLogoDropdownMenuItems } from "./useLogoDropdownMenuItems"

export type LogoDropdownButtonProps = {
  siteId: string
  title: string
  imageFileId?: string
}

export const LogoDropdownButton = memo(({ siteId, title, imageFileId }: LogoDropdownButtonProps) => {
  const [isExpanded, setExpanded] = useState(false)

  const { menuItems } = useLogoDropdownMenuItems(siteId)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(4)],
    open: isExpanded,
    placement: "bottom-end",
    onOpenChange: setExpanded,
  })

  const dismiss = useDismiss(context)
  const role = useRole(context)
  const hover = useHover(context, { handleClose: safePolygon() })
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, role, hover])

  useEffect(() => {
    const handler = () => setExpanded(false)
    window.addEventListener("resize", handler)
    window.addEventListener("scroll", handler, true)
    return () => {
      window.removeEventListener("resize", handler)
      window.removeEventListener("scroll", handler, true)
    }
  }, [])

  return (
    <div
      ref={refs.setReference}
      className="flex cursor-pointer items-center rounded-xl p-1 hover:bg-gray-100"
      title={title}
      {...getReferenceProps()}
    >
      <div className="flex select-none items-center gap-3">
        <div className="size-10 overflow-hidden rounded-lg">
          <ImageFallback src={buildFileUrl(imageFileId)} fallback={<SvgSiteLogo className="size-10" />} />
        </div>
        <span className="w-21.5 truncate text-2base font-medium leading-5.25 text-gray-800">{title}</span>
      </div>
      <ChevronDownButton expanded={isExpanded} />
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu items={menuItems} ref={refs.setFloating} style={floatingStyles} {...getFloatingProps()} />
        </FloatingPortal>
      )}
    </div>
  )
})
