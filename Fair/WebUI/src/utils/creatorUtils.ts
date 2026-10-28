import { Creator } from "types"

import { routes } from "./routes"
import { buildFileUrl, buildUserAvatarByIdUrl } from "./urlUtils"

export const buildCreatorAvatarUrl = (creator: Creator): string | undefined =>
  creator.as === "publisher" ? buildFileUrl(creator.avatarId) : buildUserAvatarByIdUrl(creator.avatarId)

export const buildCreatorLink = (storeId: string, creator: Creator): string =>
  creator.as === "publisher" ? routes.publisher(storeId, creator.id) : routes.reviewer(storeId, creator.id)
