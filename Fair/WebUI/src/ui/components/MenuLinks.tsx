import { forwardRef, memo, useMemo } from "react"
import { Link, To } from "react-router-dom"

import { PropsWithStyle } from "types"
import { chunkArray } from "utils"

export type MenuLinkItem = {
  to: To
  label: string
}

type MenuLinksBaseProps = {
  items: MenuLinkItem[]
}

export type MenuLinksProps = PropsWithStyle & MenuLinksBaseProps

export const MenuLinks = memo(
  forwardRef<HTMLDivElement, MenuLinksProps>(({ style, items }, ref) => {
    const chunks = useMemo(() => chunkArray(items, 8), [items])

    return (
      <div ref={ref} style={style} className="flex flex-wrap rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md">
        {chunks.map((chunk, i) => (
          <div key={i} className="flex flex-col">
            {chunk.map(x => (
              <Link
                key={i + x.to.toString()}
                to={x.to}
                className="w-40 overflow-hidden text-ellipsis whitespace-nowrap rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100"
                title={x.label}
              >
                {x.label}
              </Link>
            ))}
          </div>
        ))}
      </div>
    )
  }),
)
