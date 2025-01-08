import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { Hint } from "components"

import { ListRow } from "./types"

type ListBodyRowBaseProps = Pick<ListRow, "label" | "description"> & {
  hint?: string
}

export type ListBodyRowProps = PropsWithChildren & ListBodyRowBaseProps

export const ListBodyRow = ({ children, label, description, hint }: ListBodyRowProps) => (
  <div className="flex h-[52px] items-center gap-5 overflow-hidden text-sm leading-[17px]">
    <div className="min-w-[25%]">
      <div className="flex flex-col gap-1">
        <div className="overflow-hidden text-ellipsis whitespace-nowrap font-medium">{label}</div>
        {description && (
          <div className="overflow-hidden text-ellipsis whitespace-nowrap text-xs text-gray-500">{description}</div>
        )}
      </div>
    </div>
    <div className={twMerge("overflow-hidden text-ellipsis", !!hint && "flex items-center gap-2")}>
      {children} {hint && <Hint text={hint} />}
    </div>
  </div>
)
