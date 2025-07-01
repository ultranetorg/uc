import { SvgChevronRight } from "assets"
import { forwardRef, memo, useMemo } from "react"
import { Link, To } from "react-router-dom"

import { PropsWithStyle } from "types"
import { chunkArray } from "utils"

export type HeadingsMenuItem = {
  to: To
  label: string
}

export type HeadingsMenuHeadingItem = {
  items?: HeadingsMenuItem[]
} & HeadingsMenuItem

type HeadingsMenuBaseProps = {
  items: HeadingsMenuHeadingItem[]
  onClick?: () => void
}

export type HeadingsMenuProps = PropsWithStyle & HeadingsMenuBaseProps

export const HeadingsMenu = memo(
  forwardRef<HTMLDivElement, HeadingsMenuProps>(({ style, items, onClick }, ref) => {
    const chunks = useMemo(() => chunkArray(items, 4), [items])

    return (
      <div
        ref={ref}
        style={style}
        className="rounded-lg border border-gray-300 bg-gray-0 p-4 text-gray-800 shadow-md"
        onClick={onClick}
      >
        <div className="flex max-w-[952px] flex-wrap gap-6">
          {chunks.map((chunk, i) => (
            <>
              {chunk.map(x => (
                <div className="flex w-55 flex-col">
                  <Link
                    key={x.to.toString() + i}
                    to={x.to}
                    className="overflow-hidden text-ellipsis whitespace-nowrap rounded-sm p-2 text-2sm font-semibold leading-4.5 hover:bg-gray-100"
                    title={x.label}
                  >
                    {x.label}
                  </Link>
                  {x.items?.map(s => (
                    <Link
                      to={s.to}
                      className="overflow-hidden text-ellipsis whitespace-nowrap rounded-sm px-2 py-1.5 text-2xs leading-4 hover:bg-gray-100"
                      title={s.label}
                    >
                      {s.label}
                    </Link>
                  ))}
                  <Link to="" className="flex items-center rounded-sm px-2 py-1.5 text-2xs leading-4 hover:bg-gray-100">
                    View All <SvgChevronRight className="stroke-gray-500" />
                  </Link>
                </div>
              ))}
            </>
          ))}
        </div>
      </div>
    )
  }),
)
