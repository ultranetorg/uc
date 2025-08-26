import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { Radio } from "./Radio"

type RadioCardBaseProps = {
  title: string
  description: string
  checked: boolean
  onClick: () => void
}

export type RadioCardProps = PropsWithClassName & RadioCardBaseProps

export const RadioCard = ({ className, title, description, checked, onClick }: RadioCardProps) => (
  <div
    className={twMerge(
      "box-border flex h-18 cursor-pointer items-center gap-2.5 rounded-lg border-2 bg-[#F9F8F3] p-4 hover:border-gray-400 hover:bg-[#EDEBE2]",
      !checked ? "border-gray-300" : "border-gray-800",
      className,
    )}
    onClick={onClick}
  >
    <Radio className="h-5 w-5" type="radio" checked={checked} />
    <div className="flex flex-col gap-1">
      <span className="text-2sm leading-5 text-gray-800">{title}</span>
      <span className="text-2xs leading-4 text-gray-500">{description}</span>
    </div>
  </div>
)
