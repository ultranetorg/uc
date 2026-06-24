import { memo } from "react"
import { AuthorBaseAvatar } from "types"
import { LinkFullscreen } from "ui/components"
import { routes } from "utils"

import { PublisherInfo } from "./PublisherInfo"

export type PublishersListProps = {
  authors: AuthorBaseAvatar[]
}

export const PublishersList = memo(({ authors }: PublishersListProps) => (
  <div className="flex flex-col gap-3">
    {authors.map(x => (
      <LinkFullscreen key={x.id} to={routes.publisher(x.id)} className="w-fit">
        <PublisherInfo title={x.title} avatarId={x.avatarId} />
      </LinkFullscreen>
    ))}
  </div>
))
