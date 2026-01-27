import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { OptionCollapse } from "./OptionCollapse"

export type OptionsCollapsesListItem = {
  expanded?: boolean
  title: string
  description: string
  value: string | number
  votePercents: number
  voted?: boolean
  votesCount: number
}

type OptionsCollapsesListBaseProps = {
  disabled?: boolean
  items: OptionsCollapsesListItem[]
  showResults?: boolean
  showVoteButton?: boolean
  votesText: string
  onExpand?: (value: string | number, expanded: boolean) => void
  onVoteClick?: (value: string | number) => void
}

export type OptionsCollapsesListProps = PropsWithClassName & OptionsCollapsesListBaseProps

export const OptionsCollapsesList = ({
  className,
  disabled = false,
  items,
  showResults,
  showVoteButton,
  votesText,
  onExpand,
  onVoteClick,
}: OptionsCollapsesListProps) => (
  <div className={twMerge("flex flex-col gap-3", className)}>
    {items?.map(x => (
      <OptionCollapse
        {...x}
        key={x.value}
        disabled={disabled}
        expanded={!!x.expanded || (items.length === 1 ? true : undefined)}
        showResults={showResults}
        showVoteButton={showVoteButton}
        onExpand={expanded => onExpand?.(x.value, expanded)}
        onVoteClick={() => onVoteClick?.(x.value)}
        votesText={votesText}
      />
    ))}
  </div>
)
