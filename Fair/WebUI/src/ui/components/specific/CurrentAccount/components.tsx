import { forwardRef, memo, MouseEvent } from "react"
import { twMerge } from "tailwind-merge"

import { CheckCircleFillSvg, StarSvg, SvgXSm } from "assets"
import avatarFallbackXl from "assets/fallback/account-avatar-xl.png"
import avatarFallback3xl from "assets/fallback/account-avatar-3xl.png"
import { AccountBase, PropsWithClassName } from "types"
import { buildAccountAvatarUrl, MakeOptional, shortenAddress } from "utils"

type AccountBaseProps = {
  addressShort: string
  selected?: boolean
  onSelect: () => void
  onRemove: () => void
}

export type AccountProps = MakeOptional<AccountBase, "id"> & AccountBaseProps

export const Account = memo(({ id, nickname, address, addressShort, selected, onSelect, onRemove }: AccountProps) => {
  const handleRemove = (e: MouseEvent<SVGSVGElement>) => {
    e.stopPropagation()
    onRemove?.()
  }

  return (
    <div className="flex select-none items-center gap-2 overflow-hidden px-4 py-2 hover:bg-gray-100" onClick={onSelect}>
      <div className="size-8 overflow-hidden rounded-full" title={nickname ?? address}>
        <img
          src={id ? buildAccountAvatarUrl(id) : avatarFallbackXl}
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
})

export type CurrentAccountButtonProps = PropsWithClassName & MakeOptional<AccountBase, "id">

export const CurrentAccountButton = memo(
  forwardRef<HTMLDivElement, CurrentAccountButtonProps>(({ className, id, address, nickname }, ref) => (
    <div
      className={twMerge("flex cursor-pointer select-none gap-3 rounded-lg p-2 hover:bg-gray-100", className)}
      ref={ref}
    >
      <div className="size-10 overflow-hidden rounded-full" title={nickname ?? id}>
        <img
          className="size-full object-cover object-center"
          src={id ? buildAccountAvatarUrl(id) : avatarFallback3xl}
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

export type SiteProps = {
  title: string
  isStarred?: boolean
}

export const Site = memo(({ title, isStarred }: SiteProps) => (
  <div className="group flex items-center gap-3">
    <div className="size-10 rounded-lg bg-gray-700" />
    <span className="w-36 grow truncate text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
      {title}
    </span>
    <StarSvg
      className={twMerge(
        "invisible h-5 w-5 group-hover:visible",
        isStarred !== true ? "stroke-gray-400" : "fill-favorite stroke-favorite",
      )}
    />
  </div>
))
