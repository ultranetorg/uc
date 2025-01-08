import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { Tooltip } from "components"

type TagInternalProps = {
  label: string
}

const TagInternal = ({ className, label }: PropsWithClassName & TagInternalProps) => (
  <div
    className={twMerge(
      "w-fit select-none whitespace-nowrap rounded-lg border border-transparent bg-dark-alpha-100 px-2 py-[6px] font-medium leading-[18px] text-gray-400 backdrop-blur-[5px]",
      className,
    )}
  >
    {label}
  </div>
)

export type TagBaseProps = {
  label: string
  tooltipText?: string
}

export type TagProps = PropsWithClassName & TagBaseProps

export const Tag = (props: TagProps) => {
  const { tooltipText, ...rest } = props

  return tooltipText ? (
    <Tooltip text={tooltipText} place="top">
      <TagInternal {...rest} />
    </Tooltip>
  ) : (
    <TagInternal {...rest} />
  )
}
