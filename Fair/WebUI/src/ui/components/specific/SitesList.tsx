import { memo } from "react"
import { Link } from "react-router-dom"
import { PaginationResponse, SiteBase } from "types"

import { SiteCard } from "./SiteCard"

export type SitesListProps = {
  isPending: boolean
  sites?: PaginationResponse<SiteBase>
  error?: Error
}

export const SitesList = memo(({ isPending, sites, error }: SitesListProps) => (
  <div className="flex flex-col gap-3">
    {isPending || !sites ? (
      <div>LOADING</div>
    ) : sites.items.length === 0 ? (
      <div>🚫 NO SITES</div>
    ) : (
      <div className="flex justify-center">
        <div className="flex h-full w-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
          {sites.items.map(x => (
            <Link key={x.id} to={`/${x.id}`}>
              <SiteCard title={x.title} description={x.description} />
            </Link>
          ))}
        </div>
      </div>
    )}
  </div>
))
