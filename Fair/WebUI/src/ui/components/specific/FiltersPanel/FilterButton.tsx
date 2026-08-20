import { FunctionComponent, memo, SVGProps } from "react"
import { twMerge } from "tailwind-merge"

export type FilterButtonProps = {
  checked?: boolean
  icon?: FunctionComponent<SVGProps<SVGSVGElement>>
  text: string
  onClick: () => void
}

export const FilterButton = memo(({ checked, icon: Icon, text, onClick }: FilterButtonProps) => (
  <div
    className={twMerge(
      "flex select-none items-center gap-2 rounded bg-gray-100 px-3 py-2 text-2sm leading-5",
      checked ? "bg-gray-950 text-white" : "cursor-pointer",
    )}
    onClick={onClick}
  >
    {Icon && <Icon className={twMerge("stroke-gray-800", checked && "stroke-white")} />}
    {text}
  </div>
))
