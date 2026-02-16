import { forwardRef, memo, MouseEvent, useCallback, useMemo } from "react"
import { Link, To } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName, PropsWithStyle } from "types"
import { chunkArray, makeKey } from "utils"

const MENU_ITEM_CLASSNAME =
  "w-40 truncate rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100 cursor-pointer"

export type SimpleMenuItem =
  | {
      label: string
      to: To
      onClick?: never
      separator?: never
    }
  | {
      label: string
      onClick: (e: MouseEvent<HTMLDivElement>) => void
      to?: never
      separator?: never
    }
  | {
      separator: boolean
      label?: never
      to?: never
      onClick?: never
    }

type SimpleMenuBaseProps = {
  items: SimpleMenuItem[]
  menuItemClassName?: string
  multiColumnMenu?: boolean
  onClick?: () => void
}

export type SimpleMenuProps = PropsWithStyle & PropsWithClassName & SimpleMenuBaseProps

export const SimpleMenu = memo(
  forwardRef<HTMLDivElement, SimpleMenuProps>(
    ({ style, className, items, menuItemClassName, multiColumnMenu = true, onClick }, ref) => {
      const chunks = useMemo(() => chunkArray(items, multiColumnMenu ? 8 : items.length), [items, multiColumnMenu])

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
          className={twMerge(
            "z-10 flex flex-wrap overflow-y-auto rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md",
            className,
          )}
          onClick={handleClick}
        >
          {chunks.map((chunk, i) => (
            <div key={i} style={{ width: `calc(100% / ${chunks.length})` }} className="flex flex-col">
              {chunk.map(({ label, to, onClick }, j) =>
                onClick ? (
                  <div
                    key={makeKey(i, j, label)}
                    onClick={onClick}
                    className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                    title={label}
                  >
                    {label}
                  </div>
                ) : to ? (
                  <Link
                    key={makeKey(i, j, to)}
                    to={to}
                    className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                    title={label}
                  >
                    {label}
                  </Link>
                ) : (
                  <hr key={makeKey(i, j, "separator")} className="my-2 border-t-gray-300" />
                ),
              )}
            </div>
          ))}
        </div>
      )
    },
  ),
)
