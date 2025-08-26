import { twMerge } from "tailwind-merge"

import { SvgCheckSquareSmSvg, SvgChevronDown2Sm } from "assets"
import { ButtonPrimary } from "ui/components"

export type OptionsCollapseProps = {
  title: string
  description: string
  expanded: boolean
  onExpand: () => void
  onVoteClick: () => void
}

export const OptionsCollapse = ({ title, description, expanded, onExpand, onVoteClick }: OptionsCollapseProps) => {
  return (
    <div
      className={twMerge(
        "flex flex-col overflow-hidden rounded-lg border border-gray-300 hover:bg-gray-200",
        expanded ? "bg-gray-200" : "bg-gray-100",
      )}
    >
      <div className="flex cursor-pointer items-center justify-between p-4" onClick={onExpand}>
        <span className="truncate text-2base font-semibold leading-6">{title}</span>
        <SvgChevronDown2Sm
          className={twMerge("flex-shrink-0 stroke-gray-400", expanded && "rotate-180 stroke-gray-800")}
        />
      </div>
      {expanded && (
        <div className="flex flex-col gap-4 px-4 pb-4">
          <span className="text-2sm leading-5">{description}</span>
          <hr className="h-px border-0 bg-gray-300" />
          <ButtonPrimary
            label="Vote for this"
            className="h-11 w-37.5 self-end"
            icon={<SvgCheckSquareSmSvg className="fill-white" />}
            onClick={onVoteClick}
          />
        </div>
      )}
    </div>
  )
}
