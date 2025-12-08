import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { SiteCard } from "./SiteCard"

export type SitesGridProps = {
  items: SiteBase[]
}

export const SitesGrid = memo(({ items }: SitesGridProps) => (
  <div className="flex flex-col gap-3">
    <div className="flex justify-center">
      <div className="flex size-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
        {items.map(x => (
          <Link key={x.id} to={`/${x.id}`}>
            <SiteCard title={x.title} description={x.description} imageFileId={x.imageFileId} />
          </Link>
        ))}
      </div>
    </div>
  </div>
))
