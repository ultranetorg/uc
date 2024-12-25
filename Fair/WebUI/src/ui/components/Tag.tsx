import { SvgCheckCircle, SvgExclamationCircle } from "assets"
import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type TagProps = {
  title?: string
} & PropsWithClassName

export const Tag = ({ className, children, title }: PropsWithChildren<TagProps>) => (
  <div title={title} className={twMerge("w-fit rounded-md bg-[#242427] px-3 py-2 text-[#3DC1F2]", className)}>
    {children}
  </div>
)

type CustomTagProps = {
  hideLabel?: boolean
} & TagProps

export const PositiveTag = (props: CustomTagProps) => (
  <Tag className="flex select-none items-center gap-2 bg-[#5A2A2A] text-[#FF5656]" title="Positive">
    <SvgExclamationCircle /> {props.hideLabel !== true && <>Positive</>}
  </Tag>
)

export const NegativeTag = (props: CustomTagProps) => (
  <Tag className="flex select-none items-center gap-2 bg-[#18281A] text-[#56FF71]" title="Negative">
    <SvgCheckCircle /> {props.hideLabel !== true && <>Negative</>}
  </Tag>
)
