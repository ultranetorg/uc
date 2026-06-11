import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { ImageFallback } from "./ImageFallback"

type MemberInfoBaseProps = {
  title: string
  fallbackSrc: string
  avatarSrc?: string
}

export type MemberInfoProps = PropsWithClassName & MemberInfoBaseProps

export const MemberInfo = memo(({ className, title, fallbackSrc, avatarSrc }: MemberInfoProps) => (
  <div className={twMerge("flex items-center gap-2", className)} title={title}>
    <div className="size-8 shrink-0 overflow-hidden rounded-full">
      <ImageFallback src={avatarSrc} fallbackSrc={fallbackSrc} />
    </div>
    <span className={"flex-1 truncate text-2sm font-medium leading-5"}>{title}</span>
  </div>
))
