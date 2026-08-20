import {
  autoUpdate,
  FloatingPortal,
  offset,
  safePolygon,
  useClick,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { memo, useCallback, useState } from "react"
import { twMerge } from "tailwind-merge"

import { SvgChevronDown2Sm } from "assets"
import { useCloseOnScrollOrResize } from "hooks"
import { PropsWithClassName } from "types"

export type SearchInputSelectItem<T extends string> = {
  label: string
  value: T
}

type SearchInputSelectBaseProps<T extends string> = {
  items: SearchInputSelectItem<T>[]
  value?: T
  /** Horizontal shift of the menu relative to the trigger, negative moves it left. */
  menuOffsetX?: number
  onChange: (value: T) => void
}

export type SearchInputSelectProps<T extends string> = PropsWithClassName & SearchInputSelectBaseProps<T>

const SearchInputSelectInner = <T extends string>({
  className,
  items,
  value,
  menuOffsetX = 0,
  onChange,
}: SearchInputSelectProps<T>) => {
  const [isOpen, setOpen] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset({ mainAxis: 16, crossAxis: menuOffsetX })],
    open: isOpen,
    placement: "bottom-start",
    whileElementsMounted: autoUpdate,
    onOpenChange: setOpen,
  })

  const dismiss = useDismiss(context)
  const click = useClick(context, { toggle: true })
  const hover = useHover(context, { handleClose: safePolygon() })
  const role = useRole(context, { role: "listbox" })
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, click, hover, role])

  const handleClose = useCallback(() => setOpen(false), [])

  useCloseOnScrollOrResize(handleClose)

  const selected = items.find(x => x.value === value) ?? items[0]

  const handleSelect = useCallback(
    (item: SearchInputSelectItem<T>) => {
      onChange(item.value)
      setOpen(false)
    },
    [onChange],
  )

  return (
    <>
      <div
        ref={refs.setReference}
        className={twMerge(
          "flex shrink-0 cursor-pointer select-none items-center gap-1 text-gray-500 hover:text-gray-950",
          isOpen && "text-gray-950",
          className,
        )}
        {...getReferenceProps()}
      >
        {/* Every label is laid out in the same grid cell, so the trigger keeps the widest one's width. */}
        <span className="grid">
          {items.map(item => (
            <span
              key={item.value}
              className={twMerge("col-start-1 row-start-1 truncate", item.value !== selected?.value && "invisible")}
            >
              {item.label}
            </span>
          ))}
        </span>
        <SvgChevronDown2Sm className="shrink-0 stroke-current" />
      </div>

      {isOpen && (
        <FloatingPortal>
          <div
            ref={refs.setFloating}
            style={floatingStyles}
            className="z-20 flex min-w-30 flex-col rounded border border-gray-300 bg-gray-0 p-1 shadow-md"
            {...getFloatingProps()}
          >
            {items.map(item => (
              <div
                key={item.value}
                className={twMerge(
                  "cursor-pointer truncate rounded-sm px-3 py-2 text-2sm leading-5 text-gray-800 hover:bg-gray-100",
                  item.value === selected?.value && "text-gray-950",
                )}
                onClick={() => handleSelect(item)}
              >
                {item.label}
              </div>
            ))}
          </div>
        </FloatingPortal>
      )}
    </>
  )
}

export const SearchInputSelect = memo(SearchInputSelectInner) as <T extends string>(
  props: SearchInputSelectProps<T>,
) => JSX.Element
