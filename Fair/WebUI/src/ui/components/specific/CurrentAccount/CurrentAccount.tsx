import { useCallback, useState } from "react"
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

import { useAccountsContext } from "app"
import { useScrollOrResize } from "hooks"

import { AccountMenu } from "./AccountMenu"
import { CurrentAccountButton } from "./components"

export const CurrentAccount = () => {
  const [isOpen, setIsOpen] = useState(false)

  useScrollOrResize(() => setIsOpen(false))

  const { currentAccount, authenticate } = useAccountsContext()

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

  const handleClose = useCallback(() => setIsOpen(false), [])

  return (
    <>
      {currentAccount?.address ? (
        <CurrentAccountButton
          nickname={currentAccount?.nickname}
          id={currentAccount?.id}
          address={currentAccount?.address}
          ref={refs.setReference}
          {...getReferenceProps()}
        />
      ) : (
        <div className="cursor-pointer" onClick={() => authenticate()}>
          LOGIN
        </div>
      )}
      {isOpen && (
        <AccountMenu
          ref={refs.setFloating}
          style={floatingStyles}
          accountId={currentAccount?.id}
          nickname={currentAccount?.nickname}
          address={currentAccount!.address!}
          onMenuClose={handleClose}
          {...getFloatingProps()}
        />
      )}
    </>
  )
}
