import { memo } from "react"
import { Tooltip } from "components"

type TagInternalProps = {
  label: string
}

const TagInternal = memo(({ label }: TagInternalProps) => (
  <div
    className={
      "w-fit select-none whitespace-nowrap rounded-lg border border-transparent bg-dark-alpha-100 px-2 py-[6px] font-medium leading-[18px] text-gray-400 backdrop-blur-[5px]"
    }
  >
    {label}
  </div>
))

export type TagBaseProps = {
  tooltipText?: string
}

export type TagProps = TagInternalProps & TagBaseProps

export const Tag = memo((props: TagProps) => {
  const { tooltipText, ...rest } = props

  return tooltipText ? (
    <Tooltip text={tooltipText} place="top">
      <TagInternal {...rest} />
    </Tooltip>
  ) : (
    <TagInternal {...rest} />
  )
})
