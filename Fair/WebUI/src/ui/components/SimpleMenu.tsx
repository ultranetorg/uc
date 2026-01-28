import { forwardRef, memo, MouseEvent, useCallback, useMemo } from "react"
import { Link, To } from "react-router-dom"
import { twMerge } from "tailwind-merge"

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
  menuItemClassName?: string
  onClick?: () => void
}

export type SimpleMenuProps = PropsWithStyle & SimpleMenuBaseProps

export const SimpleMenu = memo(
  forwardRef<HTMLDivElement, SimpleMenuProps>(({ style, items, menuItemClassName, onClick }, ref) => {
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
          <div key={i} style={{ width: `calc(100% / ${chunks.length})` }} className="flex flex-col">
            {chunk.map(({ label, to, onClick }) =>
              onClick ? (
                <div
                  key={i + label}
                  onClick={onClick}
                  className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                  title={label}
                >
                  {label}
                </div>
              ) : (
                <Link
                  key={i + to.toString()}
                  to={to}
                  className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
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
