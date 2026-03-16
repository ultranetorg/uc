import { memo } from "react"
import { twMerge } from "tailwind-merge"

import avatarPlaceholder from "assets/images/account-avatar-placeholder-9xl.png"
import { buildFileUrl, buildSrc } from "utils"
import { PropsWithClassName } from "types"
import { ImageFallback } from "./ImageFallback"

type AccountInfoBaseProps =
  | {
      title: string
      fullTitle?: string
      avatar?: string
      avatarId?: never
      titleClassName?: string
    }
  | {
      title: string
      fullTitle?: string
      avatar?: never
      avatarId?: string
      titleClassName?: string
    }

export type AccountInfoProps = PropsWithClassName & AccountInfoBaseProps

export const AccountInfo = memo(
  ({ className, title, fullTitle, avatar, avatarId, titleClassName }: AccountInfoProps) => (
    <div className={twMerge("flex items-center gap-2", className)} title={fullTitle ?? title}>
      <div className="size-8 shrink-0 overflow-hidden rounded-full">
        <ImageFallback src={avatar ? buildSrc(avatar) : buildFileUrl(avatarId)} fallbackSrc={avatarPlaceholder} />
        {/* <img
          className="size-full object-cover"
          src={avatar ? buildSrc(avatar, avatarPlaceholder) : buildFileUrl(avatarId)}
        /> */}
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
  ),
)
