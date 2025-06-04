import { FloatingPortal, offset, useClick, useDismiss, useFloating, useInteractions, useRole } from "@floating-ui/react"
import { memo, useEffect, useState } from "react"

import { CategoryBase } from "types"
import { MoreButton } from "ui/components"

import { MoreMenu } from "./MoreMenu"

export type MoreButtonProps = {
  siteId: string
  items: CategoryBase[]
}

export const MoreDropdownButton = memo(({ siteId, items }: MoreButtonProps) => {
  const [isOpen, setOpen] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(8)],
    open: isOpen,
    placement: "bottom-end",
    onOpenChange: setOpen,
  })

  const dismiss = useDismiss(context)
  const click = useClick(context)
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, click, role])

  useEffect(() => {
    const handler = () => setOpen(false)
    window.addEventListener("resize", handler)
    window.addEventListener("scroll", handler, true)
    return () => {
      window.removeEventListener("resize", handler)
      window.removeEventListener("scroll", handler, true)
    }
  }, [])

  return (
    <>
      <MoreButton ref={refs.setReference} className="min-h-8.5 min-w-8.5" {...getReferenceProps()} />
      {isOpen && (
        <FloatingPortal>
          <MoreMenu
            ref={refs.setFloating}
            style={floatingStyles}
            siteId={siteId}
            items={items}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
