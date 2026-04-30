import { memo } from "react"

import { SvgX } from "assets"
import avatarFallback from "assets/fallback/account-avatar-xl.png"
import { ImageFallback } from "ui/components"

export type MembersListItemProps = {
  id: string
  title: string
  avatarSrc?: string
  onRemove?: (id: string) => void
}

export const MembersListItem = memo(({ id, title, avatarSrc, onRemove }: MembersListItemProps) => (
  <div className="flex h-12 items-center gap-2 rounded-full border border-gray-300 py-2 pl-2 pr-3">
    <div className="size-8 overflow-hidden rounded-full" title={title}>
      <ImageFallback src={avatarSrc} fallbackSrc={avatarFallback} />
    </div>
    <span className="select-none text-2sm font-medium leading-4" title={title}>
      {title}
    </span>
    {onRemove && <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={() => onRemove(id)} />}
  </div>
))
