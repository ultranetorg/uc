import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { OptionCollapse } from "./OptionCollapse"

type OptionsCollapsesListBaseProps = {
  items: { title: string; description: string; votePercents: number; voted?: boolean; votesCount: number }[]
  showResults?: boolean
  votesText: string
}

export type OptionsCollapsesListProps = PropsWithClassName & OptionsCollapsesListBaseProps

export const OptionsCollapsesList = ({ className, items, showResults, votesText }: OptionsCollapsesListProps) => {
  return (
    <div className={twMerge("flex flex-col gap-3", className)}>
      {items.map((x, index) => (
        <OptionCollapse
          {...x}
          expanded={items.length === 1 ? true : undefined}
          key={index}
          showResults={showResults}
          onVoteClick={() => console.log("onVoteClick")}
          votesText={votesText}
        />
      ))}
    </div>
  )
}
