import authorFallback from "assets/fallback/author-avatar-3xl.png"
import userFallback from "assets/fallback/user-avatar-3xl.png"
import { Creator } from "types"
import { MemberInfo } from "ui/components"
import { buildCreatorAvatarUrl } from "utils"

export const renderCreator = (creator: Creator) => (
  <MemberInfo
    title={creator.title}
    fallbackSrc={creator.as === "publisher" ? authorFallback : userFallback}
    avatarSrc={buildCreatorAvatarUrl(creator)}
  />
)
