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
import { useTranslation } from "react-i18next"

import { SvgBoxArrowRight } from "assets"
import { useAccountsContext } from "app"
import { useScrollOrResize } from "hooks"

import { ButtonPrimary } from "ui/components/ButtonPrimary"
import { AccountMenu } from "./AccountMenu"
import { CurrentAccountButton } from "./components"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const { t } = useTranslation("currentAccount")

  const [isOpen, setIsOpen] = useState(false)

  useScrollOrResize(() => setIsOpen(false))

  const { accounts, currentAccount, authenticate } = useAccountsContext()

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

  const handleNicknameCreate = useCallback(() => alert("handleNicknameCreate"), [])

  return (
    <>
      {!accounts.length ? (
        <ButtonPrimary
          iconBefore={<SvgBoxArrowRight className="fill-white" />}
          className={STICKY_CLASSNAME}
          label={t("login")}
          onClick={() => authenticate()}
        />
      ) : (
        currentAccount?.address && (
          <CurrentAccountButton
            className={STICKY_CLASSNAME}
            nickname={currentAccount?.nickname}
            id={currentAccount?.id}
            address={currentAccount?.address}
            ref={refs.setReference}
            {...getReferenceProps()}
          />
        )
      )}
      {isOpen && (
        <AccountMenu
          ref={refs.setFloating}
          style={floatingStyles}
          accountId={currentAccount?.id}
          nickname={currentAccount?.nickname}
          address={currentAccount!.address!}
          onMenuClose={handleClose}
          onNicknameCreate={handleNicknameCreate}
          {...getFloatingProps()}
        />
      )}
    </>
  )
}
