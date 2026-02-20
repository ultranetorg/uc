import { cloneElement, isValidElement, memo, ReactElement, useRef, useState } from "react"
import {
  arrow,
  autoUpdate,
  flip,
  FloatingArrow,
  FloatingPortal,
  offset,
  Placement,
  shift,
  useDismiss,
  useFloating,
  useFocus,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type TooltipBaseProps = {
  title?: string
  text: string
  placement?: Placement
  children: ReactElement
}

export type TooltipProps = PropsWithClassName & TooltipBaseProps

const ARROW_FILL = "#2a2932"

export const Tooltip = memo(({ children, title, text, placement = "top", className }: TooltipProps) => {
  const [open, onOpenChange] = useState(false)
  const arrowRef = useRef(null)

  const { refs, floatingStyles, context } = useFloating({
    open,
    onOpenChange,
    placement,
    whileElementsMounted: autoUpdate,
    middleware: [offset(10), flip(), shift({ padding: 5 }), arrow({ element: arrowRef })],
  })

  const hover = useHover(context, { move: false })
  const focus = useFocus(context)
  const dismiss = useDismiss(context)
  const role = useRole(context, { role: "tooltip" })

  const { getReferenceProps, getFloatingProps } = useInteractions([hover, focus, dismiss, role])

  if (!isValidElement(children)) return null

  return (
    <>
      {cloneElement(children, getReferenceProps({ ref: refs.setReference, ...(children.props as object) }))}
      {open && (
        <FloatingPortal>
          <div
            ref={refs.setFloating}
            style={floatingStyles}
            className={twMerge("z-62 max-w-62 flex flex-col gap-2 rounded-lg bg-gray-800 p-2 shadow-md", className)}
            {...getFloatingProps()}
          >
            <FloatingArrow ref={arrowRef} context={context} fill={ARROW_FILL} />
            {title && <span className="text-2sm font-medium leading-5 text-white">{title}</span>}
            <span className="text-2xs leading-4 text-gray-100">{text}</span>
          </div>
        </FloatingPortal>
      )}
    </>
  )
})
