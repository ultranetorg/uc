import { memo } from "react"

import { SvgX } from "assets"
import { buildFileUrl } from "utils"

export type AccountsListItemProps = {
  id: string
  title: string
  avatarId: string
  onRemove?: () => void
}

export const AccountsListItem = memo(({ title, avatarId, onRemove }: AccountsListItemProps) => (
  <div className="flex items-center gap-2 rounded-full border border-gray-300 py-2 pl-2 pr-3">
    <div className="h-8 w-8 overflow-hidden rounded-full" title={title}>
      <img className="h-full w-full object-cover object-center" src={buildFileUrl(avatarId)} />
    </div>
    <span className="select-none text-2sm font-medium leading-4" title={title}>
      {title}
    </span>
    <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onRemove} />
  </div>
))
