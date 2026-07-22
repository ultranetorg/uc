import { memo } from "react"
import { Link } from "react-router-dom"

import { StoreBase } from "types"
import { routes } from "utils"

import { Store } from "./Store"
import { StoreSkeleton } from "./StoreSkeleton"
import { SitesListEmptyState } from "./SitesListEmptyState"

export type StoresListProps = {
  disabledFavorite?: boolean
  isStarred?: boolean
  title: string
  items?: StoreBase[]
  emptyStateMessage: string
  onFavoriteClick: (item: StoreBase) => void
  disabledIds?: string[]
  showPending?: boolean
}

export const StoresList = memo(
  ({
    disabledFavorite,
    isStarred,
    title,
    items,
    emptyStateMessage,
    onFavoriteClick,
    disabledIds,
    showPending,
  }: StoresListProps) =>
    !items ? null : (
      <div className="flex flex-col gap-4">
        <span className="text-xs uppercase leading-3.75 tracking-tight-048 text-gray-500">{title}</span>
        {(items && items.length > 0) || showPending === true ? (
          <>
            {items.map(x => (
              <Link key={x.id} to={routes.store(x.id)}>
                <Store
                  disabled={disabledIds?.includes(x.id)}
                  disabledFavorite={disabledFavorite}
                  title={x.title}
                  imageFileId={x.imageFileId}
                  onFavoriteClick={() => onFavoriteClick(x)}
                  isStarred={isStarred}
                />
              </Link>
            ))}
            {showPending && <StoreSkeleton />}
          </>
        ) : (
          <SitesListEmptyState message={emptyStateMessage} />
        )}
      </div>
    ),
)
