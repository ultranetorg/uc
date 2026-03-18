import { ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { OptionCollapse } from "./OptionCollapse"

export type OptionsCollapsesListItem = {
  expanded?: boolean
  title: string
  description: ReactNode
  value: string | number
  votePercents: number
  voted?: boolean
  votesCount: number
}

type OptionsCollapsesListBaseProps = {
  items: OptionsCollapsesListItem[]
  showResults?: boolean
  showVoteButton?: boolean
  votesText: string
  votedValue?: number
  onExpand?: (value: string | number, expanded: boolean) => void
  onVoteClick?: (value: number) => void
}

export type OptionsCollapsesListProps = PropsWithClassName & OptionsCollapsesListBaseProps

export const OptionsCollapsesList = ({
  className,
  items,
  showResults,
  showVoteButton,
  votesText,
  votedValue,
  onExpand,
  onVoteClick,
}: OptionsCollapsesListProps) => (
  <div className={twMerge("flex flex-col gap-3", className)}>
    {items?.map(x => (
      <OptionCollapse
        {...x}
        key={x.value}
        disabled={votedValue !== undefined && x.value !== votedValue}
        loading={votedValue !== undefined && x.value === votedValue}
        expanded={!!x.expanded || (items.length === 1 ? true : undefined)}
        showResults={showResults}
        showVoteButton={showVoteButton}
        onExpand={expanded => onExpand?.(x.value, expanded)}
        onVoteClick={() => onVoteClick?.(x.value as number)}
        votesText={votesText}
        voted={x.voted}
      />
    ))}
  </div>
)
