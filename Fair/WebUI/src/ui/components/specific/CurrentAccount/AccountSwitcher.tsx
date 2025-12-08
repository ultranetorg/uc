import { forwardRef, memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgPlusCircleMd } from "assets"
import { AccountBase, PropsWithStyle } from "types"
import { MakeOptional, shortenAddress } from "utils"

import { Account } from "./components"

export type AccountSwitcherItem = MakeOptional<AccountBase, "id">

export interface AccountSwitcherBaseProps {
  selectedItemAddress?: string
  items: AccountSwitcherItem[]
  onAdd: () => void
  onRemove: (index: number) => void
  onSelect: (index: number) => void
}

export type AccountSwitcherProps = PropsWithStyle & AccountSwitcherBaseProps

export const AccountSwitcher = memo(
  forwardRef<HTMLDivElement, AccountSwitcherProps>(
    ({ style, selectedItemAddress, items, onAdd, onRemove, onSelect }: AccountSwitcherProps, ref) => {
      const { t } = useTranslation("currentAccount")

      return (
        <div
          className="z-10 w-70 cursor-pointer divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-0 py-1 shadow-md"
          ref={ref}
          style={style}
        >
          <div>
            {items.map((x, i) => (
              <Account
                key={x.address}
                addressShort={shortenAddress(x.address)}
                selected={x.address === selectedItemAddress}
                onSelect={() => onSelect(i)}
                onRemove={() => onRemove(i)}
                {...x}
              />
            ))}
          </div>
          <div
            className="flex cursor-pointer select-none items-center gap-2 px-4 py-3 text-2sm leading-4.25 text-gray-900 hover:bg-gray-100"
            onClick={onAdd}
          >
            <SvgPlusCircleMd className="fill-gray-800" /> {t("addAccount")}
          </div>
        </div>
      )
    },
  ),
)
