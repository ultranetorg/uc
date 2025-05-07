import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { SiteCard } from "./SiteCard"

export type SitesListProps = {
  isFetching: boolean
  items?: SiteBase[]
  itemsCount: number
  error?: Error
}

export const SitesList = memo(({ isFetching, items, itemsCount }: SitesListProps) => (
  <div className="flex flex-col gap-3">
    {isFetching || !items ? (
      <div>LOADING</div>
    ) : itemsCount === 0 ? (
      <div>🚫 NO SITES</div>
    ) : (
      <div className="flex justify-center">
        <div className="flex h-full w-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
          {items.map(x => (
            <Link key={x.id} to={`/${x.id}`}>
              <SiteCard title={x.title} description={x.description} />
            </Link>
          ))}
        </div>
      </div>
    )}
  </div>
))
