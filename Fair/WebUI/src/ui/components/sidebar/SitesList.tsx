import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { Site } from "./Site"
import { SiteSkeleton } from "./SiteSkeleton"
import { SitesListEmptyState } from "./SitesListEmptyState"

export type SitesListProps = {
  disabledFavorite?: boolean
  isStarred?: boolean
  title: string
  items?: SiteBase[]
  emptyStateMessage: string
  onFavoriteClick: (item: SiteBase) => void
  disabledIds?: string[]
  showPending?: boolean
}

export const SitesList = memo(
  ({
    disabledFavorite,
    isStarred,
    title,
    items,
    emptyStateMessage,
    onFavoriteClick,
    disabledIds,
    showPending,
  }: SitesListProps) =>
    !items ? null : (
      <div className="flex flex-col gap-4">
        <span className="text-xs uppercase leading-3.75 tracking-tight-048 text-gray-500">{title}</span>
        {(items && items.length > 0) || showPending === true ? (
          <>
            {items.map(x => (
              <Link key={x.id} to={`${x.id}`}>
                <Site
                  disabled={disabledIds?.includes(x.id)}
                  disabledFavorite={disabledFavorite}
                  title={x.title}
                  imageFileId={x.imageFileId}
                  onFavoriteClick={() => onFavoriteClick(x)}
                  isStarred={isStarred}
                />
              </Link>
            ))}
            {showPending && <SiteSkeleton />}
          </>
        ) : (
          <SitesListEmptyState message={emptyStateMessage} />
        )}
      </div>
    ),
)
