import { useState } from "react"
import {
  offset,
  Placement,
  safePolygon,
  size,
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
}

export const useSubmenu = (options?: UseSubmenuProps) => {
  const [isOpen, setOpen] = useState(false)

  const nodeId = useFloatingNodeId(options?.customParentId)

  const { context, floatingStyles, refs } = useFloating({
    nodeId,
    middleware: [
      offset(options?.offset ?? 8),
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
  const hover = useHover(context, { handleClose: safePolygon({ requireIntent: true }) })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

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
