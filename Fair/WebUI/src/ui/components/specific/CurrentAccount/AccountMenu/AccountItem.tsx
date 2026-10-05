import { memo, MouseEvent } from "react"

import { SvgCheckCircleFill, SvgXSm } from "assets"
import avatarFallbackXl from "assets/fallback/user-8.png"
import { UserBase } from "types"
import { buildUserAvatarByNameUrl, MakeOptional } from "utils"

type AccountItemBaseProps = {
  selected?: boolean
  onSelect: () => void
  onRemove: () => void
}

export type AccountItemProps = MakeOptional<UserBase, "id"> & AccountItemBaseProps & { avatarVersion?: number }

export const AccountItem = memo(
  ({ nickname, address, selected, avatarVersion, onSelect, onRemove }: AccountItemProps) => {
    const handleRemove = (e: MouseEvent<SVGSVGElement>) => {
      e.stopPropagation()
      onRemove?.()
    }

    return (
      <div
        className="flex h-10 select-none items-center gap-2 overflow-hidden px-4 hover:bg-gray-550"
        onClick={onSelect}
      >
        <div className="size-8 shrink-0 overflow-hidden rounded-full" title={nickname ?? address}>
          <img
            src={nickname ? `${buildUserAvatarByNameUrl(nickname)}?v=${avatarVersion ?? 0}` : avatarFallbackXl}
            className="size-full object-cover object-center"
            loading="lazy"
            onError={e => {
              e.currentTarget.onerror = null
              e.currentTarget.src = avatarFallbackXl
            }}
          />
        </div>
        <div className="flex w-39 flex-col gap-1">
          {nickname && (
            <span className="truncate text-2sm leading-4.25 text-white" title={nickname}>
              {nickname}
            </span>
          )}
        </div>
        <div className="ml-auto flex items-center gap-2">
          {selected && <SvgCheckCircleFill className="fill-white" />}
          <SvgXSm className="fill-gray-300 hover:fill-gray-0" onClick={handleRemove} />
        </div>
      </div>
    )
  },
)
