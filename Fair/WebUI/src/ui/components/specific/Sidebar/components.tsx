import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { CheckCircleFillSvg, PersonSquareSvg, StarSvg, StoresSvg } from "assets"
import { AccountBase } from "types"

import personSquareColoredImg from "./person-square-colored.png"

type AccountBaseProps = {
  addressShort: string
  selected?: boolean
}

export type AccountProps = AccountBase & AccountBaseProps

export const Account = memo(({ nickname, address, addressShort, selected }: AccountProps) => (
  <div className="flex select-none items-center gap-2 px-4 py-2 hover:bg-gray-100">
    <div className="h-8 w-8 rounded-full" title={nickname ?? address}>
      <img src={personSquareColoredImg} />
    </div>
    <div className="flex w-39 flex-col gap-1">
      {nickname && (
        <span
          className="overflow-hidden text-ellipsis whitespace-nowrap text-sm font-medium leading-4.25 text-gray-900"
          title={nickname}
        >
          {nickname}
        </span>
      )}
      <span
        className="overflow-hidden text-ellipsis whitespace-nowrap text-xs leading-3.75 text-gray-900"
        title={address}
      >
        {addressShort}
      </span>
    </div>
    {selected && <CheckCircleFillSvg className="fill-[#292D32]" />}
  </div>
))

export type AllSitesButtonProps = {
  title: string
}

export const AllSitesButton = memo(({ title }: AllSitesButtonProps) => (
  <div className="group flex items-center gap-3">
    <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-950">
      <StoresSvg className="fill-white" />
    </div>
    <span className="w-36 flex-grow overflow-hidden text-ellipsis whitespace-nowrap text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
      {title}
    </span>
  </div>
))

export type CurrentAccountButtonProps = Omit<AccountBase, "id">

export const CurrentAccountButton = memo(
  forwardRef<HTMLDivElement, CurrentAccountButtonProps>(({ nickname, address, ...rest }, ref) => (
    <div
      className="sticky bottom-2 z-20 flex cursor-pointer select-none gap-3 rounded-lg p-2 hover:bg-gray-100"
      ref={ref}
      {...rest}
    >
      <div className="h-10 w-10 rounded-full" title={nickname ?? address}>
        <img src={personSquareColoredImg} />
      </div>
      <div className="flex h-10 w-44 flex-col justify-between">
        <span className="overflow-hidden text-ellipsis text-nowrap text-2sm leading-4.5 text-gray-800" title={nickname}>
          {nickname}
        </span>
        <span
          className="overflow-hidden text-ellipsis text-nowrap text-xs uppercase leading-3.75 text-gray-500"
          title={address}
        >
          {address}
        </span>
      </div>
    </div>
  )),
)

export type MenuButtonProps = {
  label: string
}

export const MenuButton = memo(
  forwardRef<HTMLDivElement, MenuButtonProps>(({ label, ...rest }, ref) => (
    <div
      className="flex h-12 w-full cursor-pointer items-center gap-2 rounded border border-gray-300 bg-gray-100 px-4 py-3 text-sm leading-4.25 text-gray-950 hover:bg-gray-200"
      ref={ref}
      {...rest}
    >
      <PersonSquareSvg fill="#0C0E22" />
      {label}
    </div>
  )),
)

export type SiteProps = {
  title: string
  isStarred?: boolean
}

export const Site = memo(({ title, isStarred }: SiteProps) => (
  <div className="group flex items-center gap-3">
    <div className="h-10 w-10 rounded-lg bg-gray-700" />
    <span className="w-36 flex-grow overflow-hidden text-ellipsis whitespace-nowrap text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
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
