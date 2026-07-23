import { forwardRef, memo, MouseEvent } from "react"
import { twMerge } from "tailwind-merge"

import { CheckCircleFillSvg, SvgXSm } from "assets"
import avatarFallbackXl from "assets/fallback/user-8.png"
import avatarFallback3xl from "assets/fallback/user-10.png"
import { UserBase, PropsWithClassName } from "types"
import { buildUserAvatarByNameUrl, MakeOptional, shortenAddress } from "utils"

export interface MenuItemProps {
  text: string
  onClick: () => void
}

export const MenuItem = memo(({ text, onClick }: MenuItemProps) => {
  return (
    <div
      className="flex h-13 select-none items-center overflow-hidden truncate px-4 py-2 text-2sm leading-4.25 text-gray-800 hover:bg-gray-100"
      onClick={onClick}
    >
      {text}
    </div>
  )
})

type AccountBaseProps = {
  addressShort: string
  selected?: boolean
  onSelect: () => void
  onRemove: () => void
}

export type AccountProps = MakeOptional<UserBase, "id"> & AccountBaseProps & { avatarVersion?: number }

export const Account = memo(
  ({ nickname, address, addressShort, selected, avatarVersion, onSelect, onRemove }: AccountProps) => {
    const handleRemove = (e: MouseEvent<SVGSVGElement>) => {
      e.stopPropagation()
      onRemove?.()
    }

    return (
      <div
        className="flex select-none items-center gap-2 overflow-hidden px-4 py-2 hover:bg-gray-100"
        onClick={onSelect}
      >
        <div className="size-8 overflow-hidden rounded-full" title={nickname ?? address}>
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
            <span className="truncate text-2sm leading-4.25 text-gray-800" title={nickname}>
              {nickname}
            </span>
          )}
          <span className="truncate text-2xs leading-3.75 text-gray-500" title={address}>
            {addressShort}
          </span>
        </div>
        <div className="ml-auto flex items-center gap-2">
          {selected && <CheckCircleFillSvg className="fill-[#292D32]" />}
          <SvgXSm className="fill-gray-500 hover:fill-gray-800" onClick={handleRemove} />
        </div>
      </div>
    )
  },
)

export type CurrentAccountButtonProps = PropsWithClassName & MakeOptional<UserBase, "id"> & { avatarVersion?: number }

export const CurrentAccountButton = memo(
  forwardRef<HTMLDivElement, CurrentAccountButtonProps>(({ className, id, address, nickname, avatarVersion }, ref) => (
    <div
      className={twMerge("flex cursor-pointer select-none gap-3 rounded-lg p-2 hover:bg-gray-100", className)}
      ref={ref}
    >
      <div className="size-10 overflow-hidden rounded-full" title={nickname ?? id}>
        <img
          className="size-full object-cover object-center"
          src={id ? `${buildUserAvatarByNameUrl(nickname)}?v=${avatarVersion ?? 0}` : avatarFallback3xl}
          onError={e => {
            e.currentTarget.onerror = null
            e.currentTarget.src = avatarFallback3xl
          }}
        />
      </div>
      <div className={twMerge("flex h-10 w-44 flex-col", nickname ? "justify-between" : "justify-center")}>
        {nickname ? (
          <>
            <span
              className="overflow-hidden text-ellipsis text-nowrap text-2sm font-medium leading-4.5 text-gray-800"
              title={nickname}
            >
              {nickname}
            </span>
            <span
              className="overflow-hidden text-ellipsis text-nowrap text-xs leading-3.75 text-gray-500"
              title={address}
            >
              {shortenAddress(address)}
            </span>
          </>
        ) : (
          <span
            className="overflow-hidden text-ellipsis text-nowrap text-2sm font-medium leading-4.5 text-gray-800"
            title={address}
          >
            {shortenAddress(address)}
          </span>
        )}
      </div>
    </div>
  )),
)
