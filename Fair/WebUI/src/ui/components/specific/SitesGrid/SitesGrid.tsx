import { memo } from "react"
import { Link } from "react-router-dom"

import { StoreBase } from "types"
import { routes } from "utils"

import { SiteCard, SiteCardProps } from "./SiteCard"

type SitesGridBaseProps = {
  items: StoreBase[]
}

export type SitesGridProps = Pick<SiteCardProps, "showStar"> & SitesGridBaseProps

export const SitesGrid = memo(({ items, showStar }: SitesGridProps) => (
  <div className="flex flex-col gap-3">
    <div className="flex justify-center">
      <div className="flex size-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
        {items.map(x => (
          <Link key={x.id} to={routes.store(x.id)}>
            <SiteCard title={x.title} description={x.description} imageFileId={x.imageFileId} showStar={showStar} />
          </Link>
        ))}
      </div>
    </div>
  </div>
))
