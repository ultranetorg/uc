import { useState } from "react"
import { twMerge } from "tailwind-merge"

import { CheckCircleFillSvg, SvgCheckSquareSmSvg, SvgChevronDown2Sm } from "assets"
import { ButtonPrimary } from "ui/components"

export type OptionCollapseProps = {
  description: string
  expanded?: boolean
  showResults?: boolean
  title: string
  voted?: boolean
  votePercents: number
  votesCount: number
  onExpand?: (expanded: boolean) => void
  onVoteClick: () => void
  votesText: string
}

export const OptionCollapse = ({
  description,
  expanded = false,
  showResults,
  title,
  voted,
  votesCount,
  votePercents,
  onExpand,
  onVoteClick,
  votesText,
}: OptionCollapseProps) => {
  const [isExpanded, setExpanded] = useState<boolean>(expanded)

  const handleExpand = () => {
    const newState = !isExpanded
    setExpanded(newState)
    onExpand?.(newState)
  }

  return (
    <div
      className={twMerge(
        "flex flex-col overflow-hidden rounded-lg border border-gray-300 hover:bg-gray-200",
        isExpanded ? "bg-gray-200" : "bg-gray-100",
        showResults && voted && "border-2 border-gray-500",
      )}
    >
      <div
        className={twMerge(
          "relative box-border flex h-14 cursor-pointer items-center justify-between overflow-hidden p-4",
          isExpanded && "border-b border-b-gray-300",
        )}
        onClick={handleExpand}
      >
        <div className="flex items-center gap-2 overflow-hidden">
          {showResults && voted && <CheckCircleFillSvg className="z-10 fill-gray-800" />}
          <span className={twMerge("z-10 truncate leading-6", !showResults ? "text-2base font-semibold" : "text-2sm")}>
            {title}
          </span>
        </div>

        <div className="z-10 flex items-center gap-2">
          {showResults && (
            <div className="flex items-center gap-2 whitespace-nowrap text-2sm leading-5">
              <span>
                {votesCount} {votesText}
              </span>
              <span className="text-gray-500">({votePercents}%)</span>
            </div>
          )}
          <SvgChevronDown2Sm
            className={twMerge("z-10 flex-shrink-0 stroke-gray-400", isExpanded && "rotate-180 stroke-gray-800")}
          />
        </div>

        {showResults && (
          <div className={`absolute left-0 top-0 h-full bg-gray-300`} style={{ width: `${votePercents}%` }} />
        )}
      </div>

      {isExpanded && (
        <div className="flex flex-col gap-4 p-4">
          <span className="text-2sm leading-5">{description}</span>
          <hr className="h-px border-0 bg-gray-300" />
          <ButtonPrimary
            label="Vote for this"
            className="h-11 w-37.5 self-end"
            iconAfter={<SvgCheckSquareSmSvg className="fill-white" />}
            onClick={onVoteClick}
          />
        </div>
      )}
    </div>
  )
}
