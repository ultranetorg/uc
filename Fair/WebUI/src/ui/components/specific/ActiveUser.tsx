import { twMerge } from "tailwind-merge"

import avatarFallback3xl from "assets/fallback/account-avatar-3xl.png"
import { ImageFallback } from "ui/components"
import { buildUserAvatarUrl } from "utils"

export type ActiveUserProps = {
  id: string
  name: string
  disabled?: boolean
  onClick: () => void
}

export const ActiveUser = ({ id, name, disabled = false, onClick }: ActiveUserProps) => (
  <div
    className={twMerge("flex cursor-pointer gap-3 rounded-lg bg-gray-100 p-2", disabled && "cursor-not-allowed")}
    onClick={!disabled ? onClick : undefined}
  >
    <ImageFallback src={buildUserAvatarUrl(name)} fallbackSrc={avatarFallback3xl} className="size-10" />
    <div className="flex flex-col gap-1">
      <span className="text-2sm font-medium leading-4.5">{name}</span>
      <span className="text-xs leading-3.75 text-gray-500">{id}</span>
    </div>
  </div>
)
