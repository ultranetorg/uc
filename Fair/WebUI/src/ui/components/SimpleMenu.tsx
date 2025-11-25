import { forwardRef, memo, MouseEvent, useCallback, useMemo } from "react"
import { Link, To } from "react-router-dom"

import { PropsWithStyle } from "types"
import { chunkArray } from "utils"

const MENU_ITEM_CLASSNAME = "w-40 truncate rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100"

export type SimpleMenuItem =
  | {
      label: string
      to: To
      onClick?: never
    }
  | {
      label: string
      onClick: (e: MouseEvent<HTMLDivElement>) => void
      to?: never
    }

type SimpleMenuBaseProps = {
  items: SimpleMenuItem[]
  onClick?: () => void
}

export type SimpleMenuProps = PropsWithStyle & SimpleMenuBaseProps

export const SimpleMenu = memo(
  forwardRef<HTMLDivElement, SimpleMenuProps>(({ style, items, onClick }, ref) => {
    const chunks = useMemo(() => chunkArray(items, 8), [items])

    const handleClick = useCallback(
      (e: MouseEvent<HTMLDivElement>) => {
        e.stopPropagation()
        onClick?.()
      },
      [onClick],
    )

    return (
      <div
        ref={ref}
        style={style}
        className="z-10 flex cursor-pointer flex-wrap rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md"
        onClick={handleClick}
      >
        {chunks.map((chunk, i) => (
          <div key={i} className="flex flex-col">
            {chunk.map(({ label, to, onClick }) =>
              onClick ? (
                <div key={i + label} onClick={onClick} className={MENU_ITEM_CLASSNAME} title={label}>
                  {label}
                </div>
              ) : (
                <Link
                  key={i + to.toString()}
                  to={to}
                  className="w-40 truncate rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100"
                  title={label}
                >
                  {label}
                </Link>
              ),
            )}
          </div>
        ))}
      </div>
    )
  }),
)
