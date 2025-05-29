import { forwardRef, memo } from "react"

import { AccountBase, PropsWithStyle } from "types"
import { shortenAddress } from "utils"

import { Account } from "./components"

export type AccountSwitcherBaseProps = {
  items: AccountBase[]
}

export type AccountSwitcherProps = PropsWithStyle & AccountSwitcherBaseProps

export const AccountSwitcher = memo(
  forwardRef<HTMLDivElement, AccountSwitcherProps>(({ style, items }: AccountSwitcherProps, ref) => (
    <div
      className="z-10 w-65 cursor-pointer divide-y divide-[#D9D9D9] rounded-lg border border-[#D2D8E4] bg-gray-0 py-1 shadow-[0_4px_14px_0_rgba(28,38,58,0.1)]"
      ref={ref}
      style={style}
    >
      <div>
        {items.map(x => (
          <Account key={x.id} addressShort={shortenAddress(x.address)} {...x} />
        ))}
      </div>
      <div className="cursor-pointer select-none px-4 py-3 text-sm font-medium leading-4.25 text-gray-900 hover:bg-gray-100">
        Manage accounts
      </div>
    </div>
  )),
)
