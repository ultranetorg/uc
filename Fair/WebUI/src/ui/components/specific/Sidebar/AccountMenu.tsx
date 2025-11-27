import { forwardRef, memo, useCallback, useState } from "react"
import {
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useFloatingNodeId,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { useCopyToClipboard } from "usehooks-ts"

import avatarFallback from "assets/fallback/account-avatar-11xl.png"
import { AccountBase, PropsWithStyle } from "types"
import { CopyButton } from "ui/components/CopyButton"

import { buildAccountAvatarUrl } from "utils"
import pngBackground from "./background.png"
import { AccountSwitcher } from "./AccountSwitcher"
import { MenuButton } from "./components"

const TEST_ACCOUNTS = [
  {
    id: "67465-1",
    nickname: "This is very very long nickname",
  },
  {
    id: "67465-2",
    nickname: "Short",
  },
  {
    id: "67465-3",
    address: "0x1234567890abcdef1234567890abcdef12345678",
  },
]

type AccountMenuBaseProps = {
  accountId: string // NOTE: Account should be passed as "accountId" not as an "id", because "id" property is already used by getFloatingProps() function of Floating UI.
}

export type AccountMenuProps = PropsWithStyle & Omit<AccountBase, "id" | "address"> & AccountMenuBaseProps

export const AccountMenu = memo(
  forwardRef<HTMLDivElement, AccountMenuProps>(({ style, accountId, nickname }, ref) => {
    const [copiedText, copy] = useCopyToClipboard()

    const [isOpen, setOpen] = useState(false)

    const nodeId = useFloatingNodeId()
    const { context, floatingStyles, refs } = useFloating({
      nodeId,
      middleware: [offset(8)],
      open: isOpen,
      placement: "right",
      onOpenChange: setOpen,
    })

    const dismiss = useDismiss(context)
    const hover = useHover(context, { handleClose: safePolygon({ requireIntent: true }) })
    const role = useRole(context)
    const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

    const handleCopyClick = useCallback(() => {
      copy(accountId)
      console.log(copiedText)
    }, [accountId, copiedText, copy])

    return (
      <>
        <div
          // TODO: should be uncommented later h-[392px]
          className="z-10 w-[340px] overflow-hidden rounded-lg border border-gray-300 bg-gray-75 shadow-[0_4px_14px_0_rgba(28,38,58,0.1)]"
          ref={ref}
          style={style}
        >
          <div className="relative h-[169px]">
            <div className="h-[120px] w-[340px] bg-[#2A2A2A]">
              <img src={pngBackground} alt="Background" className="size-full rounded-lg object-cover" />
            </div>
            <div className="absolute bottom-0 left-[20px] size-[98px] rounded-full bg-gray-75" />
            <div className="absolute bottom-[4px] left-[24px] size-[90px] rounded-full" title={nickname ?? accountId}>
              <img
                src={buildAccountAvatarUrl(accountId)}
                onError={e => {
                  e.currentTarget.onerror = null
                  e.currentTarget.src = avatarFallback
                }}
              />
            </div>
          </div>
          <div className="flex flex-col gap-2 px-6 py-2">
            <span
              className="overflow-hidden text-ellipsis text-nowrap text-xl font-semibold leading-6 text-gray-800"
              title={nickname}
            >
              {nickname}
            </span>
            <div className="flex items-center gap-1">
              <span
                className="overflow-hidden text-ellipsis text-nowrap text-2xs leading-3.5 text-gray-500"
                title={accountId}
              >
                {accountId}
              </span>
              <CopyButton onClick={handleCopyClick} />
            </div>
          </div>
          <div className="flex flex-col gap-4 p-6">
            {/*
              //TODO: should be uncommented later.
              <Link to={`/p/abc`} state={{ backgroundLocation: location }}>
                <MenuButton label="Profile" />
              </Link>
            */}
            <MenuButton label="Switch Accounts" ref={refs.setReference} {...getReferenceProps()} />
          </div>
        </div>
        {isOpen && (
          <AccountSwitcher
            ref={refs.setFloating}
            style={floatingStyles}
            items={TEST_ACCOUNTS}
            {...getFloatingProps()}
          />
        )}
      </>
    )
  }),
)
