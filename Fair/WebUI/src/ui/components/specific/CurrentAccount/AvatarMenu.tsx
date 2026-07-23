import { forwardRef, memo } from "react"
import { TFunction } from "i18next"

import { UserBase, PropsWithStyle } from "types"

import { MenuItem } from "./components"

export type AccountSwitcherItem = Omit<UserBase, "id">

export interface AccountSwitcherBaseProps {
  t: TFunction
  onChange: () => void
  onDelete: () => void
}

export type AccountSwitcherProps = PropsWithStyle & AccountSwitcherBaseProps

export const AvatarMenu = memo(
  forwardRef<HTMLDivElement, AccountSwitcherProps>(({ t, style, onChange, onDelete }: AccountSwitcherProps, ref) => {
    return (
      <div
        className="z-10 w-70 cursor-pointer divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-0 py-1 shadow-md"
        ref={ref}
        style={style}
      >
        <MenuItem text={t("changeAvatar")} onClick={onChange} />
        <MenuItem text={t("deleteAvatar")} onClick={onDelete} />
      </div>
    )
  }),
)
