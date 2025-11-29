import { useState } from "react"
import {
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useFloatingParentNodeId,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

import { useRootContext } from "app"

import { AccountMenu } from "./AccountMenu"
import { CurrentAccountButton } from "./components"

export const CurrentAccount = () => {
  const [isOpen, setIsOpen] = useState(false)

  const { accountAddress, id, nickname } = useRootContext()

  const nodeId = useFloatingParentNodeId()
  const { context, floatingStyles, refs } = useFloating({
    nodeId: nodeId!,
    middleware: [offset(8)],
    open: isOpen,
    placement: "top-start",
    onOpenChange: setIsOpen,
  })

  const dismiss = useDismiss(context)
  const hover = useHover(context, { handleClose: safePolygon({ requireIntent: true }) })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

  return (
    <>
      <CurrentAccountButton
        nickname={nickname ?? "This is very very long nickname"}
        id={id ?? "10000-0"}
        address={accountAddress ?? "0xf2884A04A0caB3fa166c85DF55Ab1Af8549dB936"}
        ref={refs.setReference}
        {...getReferenceProps()}
      />
      {isOpen && (
        <AccountMenu
          ref={refs.setFloating}
          style={floatingStyles}
          accountId={id}
          nickname={nickname ?? "This is very very long nickname nickname nickname nickname nickname nickname nickname"}
          address={accountAddress ?? "0xf2884A04A0caB3fa166c85DF55Ab1Af8549dB936"}
          {...getFloatingProps()}
        />
      )}
    </>
  )
}
