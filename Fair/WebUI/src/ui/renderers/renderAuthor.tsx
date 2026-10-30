import avatarFallback from "assets/fallback/author-16.png"
import { AuthorBaseAvatar } from "types"
import { MemberInfo } from "ui/components"
import { buildFileUrl } from "utils"

export const renderAuthor2 = (authorTitle: string, authorLogoId?: string) => (
  <MemberInfo title={authorTitle} fallbackSrc={avatarFallback} avatarSrc={buildFileUrl(authorLogoId)} />
)

export const renderAuthor = (author: AuthorBaseAvatar) => (
  <MemberInfo
    title={
      author.title && author.name ? `${author.title} (${author.name})` : (author.title ?? author.name ?? author.id)
    }
    fallbackSrc={avatarFallback}
    avatarSrc={buildFileUrl(author.avatarId)}
  />
)
