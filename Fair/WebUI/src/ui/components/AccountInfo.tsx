import { memo } from "react"

import avatarPlaceholder from "assets/images/account-avatar-placeholder-9xl.png"
import { buildSrc } from "utils"
import { twMerge } from "tailwind-merge"

export type AccountInfoProps = {
  title: string
  fullTitle: string
  avatar?: string
  titleClassName?: string
}

export const AccountInfo = memo(({ title, fullTitle, avatar, titleClassName }: AccountInfoProps) => (
  <div className="flex items-center gap-2" title={fullTitle}>
    <div className="h-8 w-8 shrink-0 overflow-hidden rounded-full">
      <img className="h-full w-full object-cover" src={buildSrc(avatar, avatarPlaceholder)} />
    </div>
    <span
      className={twMerge(
        "flex-1 overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5",
        titleClassName,
      )}
    >
      {title}
    </span>
  </div>
))
