import { forwardRef, memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgPlusCircleMd } from "assets"
import { UserBase, PropsWithStyle } from "types"

import { AccountItem } from "./AccountItem"

export type AccountMenuItem = Omit<UserBase, "id">

export interface AccountMenuBaseProps {
  selectedUserName?: string
  items: AccountMenuItem[]
  avatarVersion?: number
  onAdd: () => void
  onRemove: (userName: string) => void
  onSelect: (userName: string) => void
}

export type AccountMenuProps = PropsWithStyle & AccountMenuBaseProps

export const AccountMenu = memo(
  forwardRef<HTMLDivElement, AccountMenuProps>(
    ({ style, selectedUserName, items, avatarVersion, onAdd, onRemove, onSelect }: AccountMenuProps, ref) => {
      const { t } = useTranslation("currentAccount")

      return (
        <div
          className="z-10 w-65 cursor-pointer divide-y divide-gray-700 overflow-hidden rounded-lg border border-gray-700 bg-gray-600 shadow-md"
          ref={ref}
          style={style}
        >
          <div>
            {items.map(x => (
              <AccountItem
                key={x.nickname}
                selected={x.nickname === selectedUserName}
                avatarVersion={x.nickname === selectedUserName ? avatarVersion : undefined}
                onSelect={() => onSelect(x.nickname)}
                onRemove={() => onRemove(x.nickname)}
                {...x}
              />
            ))}
          </div>
          <div
            className="flex h-10 cursor-pointer select-none items-center gap-2 px-4 text-2sm leading-4.25 text-white hover:bg-gray-550"
            onClick={onAdd}
          >
            <SvgPlusCircleMd className="fill-white" /> {t("addUser")}
          </div>
        </div>
      )
    },
  ),
)
