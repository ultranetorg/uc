import { memo } from "react"
import { AuthorBaseAvatar } from "types"
import { LinkFullscreen } from "ui/components"
import { routes } from "utils"

import { PublisherInfo } from "./PublisherInfo"

export type PublishersListProps = {
  siteId: string
  authors: AuthorBaseAvatar[]
}

export const PublishersList = memo(({ siteId, authors }: PublishersListProps) => (
  <div className="flex flex-col gap-3">
    {authors.map(x => (
      <LinkFullscreen key={x.id} to={routes.publisher(siteId, x.id)} className="w-fit">
        <PublisherInfo title={x.title} avatarId={x.avatarId} />
      </LinkFullscreen>
    ))}
  </div>
))
