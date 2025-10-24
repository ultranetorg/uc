import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgArrow90DegLeftMd, SvgPlusCircleMd, SvgX } from "assets"
import { AccountInfo } from "ui/components"

import { MembersListBehavior } from "./types"

export type MembersListItemProps = {
  avatar?: string
  behavior: MembersListBehavior
  changed?: boolean
  title: string
  onActionClick: () => void
}

export const MembersListItem = memo(({ avatar, behavior, changed, title, onActionClick }: MembersListItemProps) => (
  <div
    className={twMerge(
      "box-border flex h-12 items-center justify-between rounded-lg border p-2",
      !changed ? "border-transparent" : behavior === "remove-items" ? "border-gray-300/40" : "border-[#d9d9d9]",
    )}
  >
    <AccountInfo
      className={twMerge("w-full select-none", changed && behavior === "remove-items" && "opacity-40")}
      title={title}
      avatar={avatar}
    />

    <div className="cursor-pointer" onClick={onActionClick}>
      {behavior === "remove-items" && changed ? (
        <SvgArrow90DegLeftMd className="stroke-gray-500" />
      ) : behavior === "add-items" && !changed ? (
        <SvgPlusCircleMd className="fill-gray-500" />
      ) : (
        <SvgX className="stroke-gray-500" />
      )}
    </div>
  </div>
))
