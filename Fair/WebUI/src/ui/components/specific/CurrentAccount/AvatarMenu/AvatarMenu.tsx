import { forwardRef, memo } from "react"
import { TFunction } from "i18next"

import { UserBase, PropsWithStyle } from "types"

import { AvatarMenuItem } from "./AvatarMenuItem"

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
        className="z-10 w-65 cursor-pointer overflow-hidden rounded-lg border border-gray-700 bg-gray-600 shadow-md"
        ref={ref}
        style={style}
      >
        <AvatarMenuItem text={t("changeAvatar")} onClick={onChange} />
        <AvatarMenuItem text={t("deleteAvatar")} onClick={onDelete} />
      </div>
    )
  }),
)
