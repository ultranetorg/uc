import {
  // FloatingPortal,
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { memo, useEffect, useState } from "react"

import { TEST_LOGO_SRC } from "testConfig"
import { buildSrc } from "utils"

export type LogoDropdownButtonProps = {
  title: string
  avatar?: string
}

export const LogoDropdownButton = memo(({ title, avatar }: LogoDropdownButtonProps) => {
  const [isExpanded, setExpanded] = useState(false)

  const { context, /* floatingStyles,*/ refs } = useFloating({
    middleware: [offset(4)],
    open: isExpanded,
    placement: "bottom-end",
    onOpenChange: setExpanded,
  })

  const dismiss = useDismiss(context)
  const role = useRole(context)
  const hover = useHover(context, { handleClose: safePolygon() })
  const { getReferenceProps /* getFloatingProps */ } = useInteractions([dismiss, role, hover])

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
        <div className="h-10 w-10 overflow-hidden rounded-lg">
          <img src={buildSrc(avatar, TEST_LOGO_SRC)} alt="Logo" className="h-full w-full object-contain" />
        </div>
        <span className="w-21.5 overflow-hidden text-ellipsis whitespace-nowrap text-2base font-medium leading-5.25 text-gray-800">
          {title}
        </span>
      </div>
      {/* <DropdownButton expanded={isExpanded} />
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu
            items={[
              { label: "abc", to: "/abc" },
              { label: "cde", to: "/cde" },
            ]}
            ref={refs.setFloating}
            style={floatingStyles}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )} */}
    </div>
  )
})
