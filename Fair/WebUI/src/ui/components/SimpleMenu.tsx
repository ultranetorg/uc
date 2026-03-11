import { forwardRef, memo, MouseEvent, useCallback, useMemo } from "react"
import { Link, To } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName, PropsWithStyle } from "types"
import { chunkArray, makeKey } from "utils"

const MENU_ITEM_CLASSNAME =
  "w-40 truncate rounded-sm p-2 text-2sm leading-4.5 text-gray-900 hover:bg-gray-100 cursor-pointer first-letter:uppercase"

const MENU_FLYOUT_CLASSNAME = "z-20 flex flex-col rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md"

export type SimpleMenuChildItem =
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

export type SimpleMenuItem =
  | {
      label: string
      to: To
      onClick?: never
      separator?: never
      children?: never
    }
  | {
      label: string
      onClick: (e: MouseEvent<HTMLDivElement>) => void
      to?: never
      separator?: never
      children?: never
    }
  | {
      separator: boolean
      label?: never
      to?: never
      onClick?: never
      children?: never
    }
  | {
      label: string
      children: SimpleMenuChildItem[]
      to?: never
      onClick?: never
      separator?: never
    }

type SimpleMenuBaseProps = {
  items: SimpleMenuItem[]
  menuItemClassName?: string
  multiColumnMenu?: boolean
  submenuSide?: "left" | "right"
  onClick?: () => void
}

export type SimpleMenuProps = PropsWithStyle & PropsWithClassName & SimpleMenuBaseProps

export const SimpleMenu = memo(
  forwardRef<HTMLDivElement, SimpleMenuProps>(
    ({ style, className, items, menuItemClassName, multiColumnMenu = true, submenuSide = "left", onClick }, ref) => {
      const chunks = useMemo(() => chunkArray(items, multiColumnMenu ? 8 : items.length), [items, multiColumnMenu])

      const handleClick = useCallback(
        (e: MouseEvent<HTMLDivElement>) => {
          e.stopPropagation()
          onClick?.()
        },
        [onClick],
      )

      const flyoutPositionClassName = submenuSide === "left" ? "right-full" : "left-full"

      return (
        <div
          ref={ref}
          style={style}
          className={twMerge(
            "z-10 flex flex-wrap rounded-lg border border-gray-300 bg-gray-0 p-1 shadow-md",
            className,
          )}
          onClick={handleClick}
        >
          {chunks.map((chunk, i) => (
            <div key={i} style={{ width: `calc(100% / ${chunks.length})` }} className="flex flex-col">
              {chunk.map((item, j) =>
                item.onClick ? (
                  <div
                    key={makeKey(i, j, item.label)}
                    onClick={item.onClick}
                    className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                    title={item.label}
                  >
                    {item.label}
                  </div>
                ) : item.to ? (
                  <Link
                    key={makeKey(i, j, item.to)}
                    to={item.to}
                    className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                    title={item.label}
                  >
                    {item.label}
                  </Link>
                ) : item.children ? (
                  <div key={makeKey(i, j, item.label)} className="group/sub relative">
                    <div
                      className={twMerge(
                        MENU_ITEM_CLASSNAME,
                        "flex items-center justify-between gap-2",
                        menuItemClassName,
                      )}
                      title={item.label}
                    >
                      <span className="first-letter:uppercase">{item.label}</span>
                      <span className="text-gray-400">{submenuSide === "left" ? "‹" : "›"}</span>
                    </div>

                    <div
                      className={twMerge(
                        MENU_FLYOUT_CLASSNAME,
                        "absolute top-0 hidden group-hover/sub:flex",
                        flyoutPositionClassName,
                      )}
                    >
                      {item.children.map((child, k) =>
                        child.onClick ? (
                          <div
                            key={makeKey(i, j, k)}
                            onClick={child.onClick}
                            className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                            title={child.label}
                          >
                            {child.label}
                          </div>
                        ) : (
                          <Link
                            key={makeKey(i, j, k)}
                            to={child.to}
                            className={twMerge(MENU_ITEM_CLASSNAME, menuItemClassName)}
                            title={child.label}
                          >
                            {child.label}
                          </Link>
                        ),
                      )}
                    </div>
                  </div>
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
