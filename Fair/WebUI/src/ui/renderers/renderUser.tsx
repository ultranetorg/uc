import avatarFallback from "assets/fallback/user-16.png"
import { User } from "types"
import { MemberInfo } from "ui/components"
import { buildUserAvatarByIdUrl } from "utils"

export const renderUser = (userOrId: User | string, name?: string) => {
  const title = typeof userOrId === "string" ? name! : userOrId.name
  const id = typeof userOrId === "string" ? userOrId : userOrId.id
  return <MemberInfo title={title} fallbackSrc={avatarFallback} avatarSrc={buildUserAvatarByIdUrl(id)!} />
}
