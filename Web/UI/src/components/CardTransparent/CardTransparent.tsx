import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { CardTransparentRow, CardTransparentValueRenderer } from "./types"
import { getValue } from "./utils"

type CardTransparentBaseProps = {
  title?: string
  titleClassName?: string
  rows: CardTransparentRow[]
  [items: string]: any
  labelClassName?: string
  valueClassName?: string
  valueRenderer?: CardTransparentValueRenderer
}

export type CardTransparentProps = PropsWithClassName & CardTransparentBaseProps

export const CardTransparent = memo((props: CardTransparentProps) => {
  const { className, title, titleClassName, rows, items, labelClassName, valueClassName, valueRenderer } = props

  const elements = useMemo(
    () =>
      rows.map(row => {
        const value = getValue(row.accessor, items, row, valueRenderer)
        if (value === undefined) {
          return undefined
        }

        return (
          <div className="flex w-full items-center justify-between gap-4" key={row.accessor}>
            {row.fullRow !== true ? (
              <>
                <div className={labelClassName}>{row.label}</div>
                <div className={twMerge("w-[120px]", valueClassName)}>{value}</div>
              </>
            ) : (
              <>{value}</>
            )}
          </div>
        )
      }),
    [items, labelClassName, rows, valueClassName, valueRenderer],
  )

  return (
    <div className={twMerge("flex flex-col gap-2", className)}>
      {title && <div className={twMerge("text-base font-medium", titleClassName)}>{title}</div>}
      <div className="flex flex-col gap-2">{elements}</div>
    </div>
  )
})
