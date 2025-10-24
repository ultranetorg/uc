import { memo, useEffect, useState } from "react"
import { twMerge } from "tailwind-merge"
import {
  autoUpdate,
  FloatingPortal,
  offset,
  safePolygon,
  useClick,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

import { SvgSliders } from "assets"

import { FiltersMenu } from "./FiltersMenu"

export type FiltersDropdownButtonProps = {
  label: string
  resetAllLabel: string
}

export const FiltersDropdownButton = memo(({ label, resetAllLabel }: FiltersDropdownButtonProps) => {
  const [isExpanded, setExpanded] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(8)],
    open: isExpanded,
    placement: "bottom-end",
    whileElementsMounted: autoUpdate,
    onOpenChange: setExpanded,
  })

  const dismiss = useDismiss(context)
  const click = useClick(context, { toggle: true })
  const hover = useHover(context, { handleClose: safePolygon() })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, click, hover, role])

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
    <>
      <div
        ref={refs.setReference}
        className={twMerge(
          "box-border flex h-10 w-35 cursor-pointer items-center gap-2 rounded border border-gray-300 bg-gray-100 px-4 py-3 hover:border-gray-400",
          isExpanded && "border-gray-400",
        )}
        {...getReferenceProps()}
      >
        <span className="w-21 text-2xs font-medium leading-4 text-gray-800">{label}</span>
        <SvgSliders className="stroke-gray-500" />
      </div>
      {isExpanded && (
        <FloatingPortal>
          <FiltersMenu
            ref={refs.setFloating}
            style={{ ...floatingStyles, pointerEvents: "auto" }}
            onResetClick={() => console.log("reset")}
            resetAllLabel={resetAllLabel}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
