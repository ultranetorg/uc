import { useState } from "react"
import {
  flip,
  offset,
  Placement,
  safePolygon,
  size,
  useClick,
  useDismiss,
  useFloating,
  useFloatingNodeId,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

export type UseSubmenuProps = {
  customParentId?: string
  placement?: Placement
  offset?: number
  setFloatSizeAsReference?: boolean
  trigger?: "hover" | "click"
}

export const useSubmenu = (options?: UseSubmenuProps) => {
  const [isOpen, setOpen] = useState(false)

  const nodeId = useFloatingNodeId(options?.customParentId)

  const { context, floatingStyles, refs } = useFloating({
    nodeId,
    middleware: [
      offset(options?.offset ?? 8),
      flip(),
      ...(options?.setFloatSizeAsReference === true
        ? [
            size({
              apply({ rects, elements }) {
                Object.assign(elements.floating.style, {
                  width: `${rects.reference.width}px`,
                })
              },
            }),
          ]
        : []),
    ],
    open: isOpen,
    placement: options?.placement ?? "right-start",
    onOpenChange: setOpen,
  })

  const dismiss = useDismiss(context)
  const hover = useHover(context, {
    enabled: (options?.trigger ?? "hover") === "hover",
    handleClose: safePolygon({ requireIntent: true }),
  })
  const click = useClick(context, { enabled: options?.trigger === "click", toggle: true })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, click, role])

  return {
    nodeId,

    isOpen,
    setOpen,

    refs,
    floatingStyles,
    getReferenceProps,
    getFloatingProps,
  }
}
