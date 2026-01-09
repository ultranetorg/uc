import avatarFallback3xl from "assets/fallback/account-avatar-3xl.png"
import { AccountBase } from "types"
import { ImageFallback } from "ui/components"
import { buildAccountAvatarUrl } from "utils"

type ActiveAccountBaseProps = {
  onClick: () => void
}

export type ActiveAccountProps = AccountBase & ActiveAccountBaseProps

export const ActiveAccount = ({ nickname, id, onClick }: ActiveAccountProps) => (
  <div className="flex cursor-pointer gap-3 rounded-lg bg-gray-100 p-2" onClick={onClick}>
    <ImageFallback src={buildAccountAvatarUrl(id)} fallbackSrc={avatarFallback3xl} className="size-10" />
    <div className="flex flex-col gap-1">
      <span className="text-2sm font-medium leading-4.5">{nickname}</span>
      <span className="text-xs leading-3.75 text-gray-500">{id}</span>
    </div>
  </div>
)
