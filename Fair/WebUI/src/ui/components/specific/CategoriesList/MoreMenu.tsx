import { forwardRef, memo, useMemo } from "react"
import { Link } from "react-router-dom"

import { CategoryBase, PropsWithStyle } from "types"
import { chunkArray } from "utils"

type MoreMenuBaseProps = {
  siteId: string
  items: CategoryBase[]
}

export type MoreMenuProps = PropsWithStyle & MoreMenuBaseProps

export const MoreMenu = memo(
  forwardRef<HTMLDivElement, MoreMenuProps>(({ style, siteId, items }, ref) => {
    const chunks = useMemo(() => chunkArray(items, 8), [items])

    return (
      <div ref={ref} style={style} className="flex flex-wrap rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md">
        {chunks.map((chunk, i) => (
          <div key={i} className="flex flex-col">
            {chunk.map(x => (
              <Link
                key={x.id}
                to={`/${siteId}/c/${x.id}`}
                className="w-40 overflow-hidden text-ellipsis whitespace-nowrap rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100"
                title={x.title}
              >
                {x.title}
              </Link>
            ))}
          </div>
        ))}
      </div>
    )
  }),
)
