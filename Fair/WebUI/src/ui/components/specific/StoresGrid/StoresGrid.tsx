import { memo } from "react"
import { Link } from "react-router-dom"

import { StoreBase } from "types"
import { routes } from "utils"

import { StoreCard, StoreCardProps } from "./StoreCard"

type StoresGridBaseProps = {
  items: StoreBase[]
}

export type StoresGridProps = Pick<StoreCardProps, "showStar"> & StoresGridBaseProps

export const StoresGrid = memo(({ items, showStar }: StoresGridProps) => (
  <div className="flex flex-col gap-3">
    <div className="flex justify-center">
      <div className="flex size-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
        {items.map(x => (
          <Link key={x.id} to={routes.store(x.id)}>
            <StoreCard title={x.title} description={x.description} imageFileId={x.imageFileId} showStar={showStar} />
          </Link>
        ))}
      </div>
    </div>
  </div>
))
