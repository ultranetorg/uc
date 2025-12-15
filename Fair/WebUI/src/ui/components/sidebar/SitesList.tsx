import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { Site } from "./Site"
import { SitesListEmptyState } from "./SitesListEmptyState"

export type SitesListProps = {
  isStarred?: boolean
  title: string
  items?: SiteBase[]
  emptyStateMessage: string
  onFavoriteClick: (item: SiteBase) => void
}

export const SitesList = memo(({ isStarred, title, items, emptyStateMessage, onFavoriteClick }: SitesListProps) =>
  !items ? null : (
    <div className="flex flex-col gap-4">
      <span className="text-xs uppercase leading-3.75 tracking-tight-048 text-gray-500">{title}</span>
      {items && items.length > 0 ? (
        items.map(x => (
          <Link key={x.id} to={`${x.id}`}>
            <Site
              title={x.title}
              imageFileId={x.imageFileId}
              onFavoriteClick={() => onFavoriteClick(x)}
              isStarred={isStarred}
            />
          </Link>
        ))
      ) : (
        <SitesListEmptyState message={emptyStateMessage} />
      )}
    </div>
  ),
)
