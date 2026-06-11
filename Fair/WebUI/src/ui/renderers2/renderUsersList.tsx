import { ReactNode } from "react"

import avatarFallback from "assets/fallback/user-16.png"
import { User } from "types"
import { MemberInfo } from "ui/components"
import { buildUserAvatarUrl } from "utils"

export const renderUsersList = (users: User[]): ReactNode => {
  const usersToRender = users.slice(0, 3)

  return (
    <div className="flex items-center gap-2 overflow-hidden">
      {usersToRender.map(x => (
        <MemberInfo key={x.id} title={x.name} fallbackSrc={avatarFallback} avatarSrc={buildUserAvatarUrl(x.id)} />
      ))}
      {users.length > 3 && <>...</>}
    </div>
  )
}
