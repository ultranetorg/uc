import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { Site } from "./Site"
import { SitesListEmptyState } from "./SitesListEmptyState"

export type SitesListProps = {
  title: string
  items?: SiteBase[]
  emptyStateMessage: string
}

export const SitesList = memo(({ title, items, emptyStateMessage }: SitesListProps) =>
  !items ? null : (
    <div className="flex flex-col gap-4">
      <span className="text-xs uppercase leading-3.75 tracking-tight-048 text-gray-500">{title}</span>
      {items && items.length > 0 ? (
        items.map(({ id, title, imageFileId }) => (
          <Link key={id} to={`${id}`}>
            <Site title={title} imageFileId={imageFileId} />
          </Link>
        ))
      ) : (
        <SitesListEmptyState message={emptyStateMessage} />
      )}
    </div>
  ),
)
