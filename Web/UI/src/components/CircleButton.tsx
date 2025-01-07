import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgArrowUp } from "assets"
import { PropsWithClassName } from "types"

type CircleButtonBaseProps = {
  onClick?: () => void
}

export type CircleButtonProps = PropsWithClassName & CircleButtonBaseProps

export const CircleButton = memo(({ className, onClick }: CircleButtonProps) => (
  <div
    className={twMerge(
      "box-border flex h-14 w-14 cursor-pointer items-center justify-center rounded-full border border-dark-alpha-100 bg-dark-blue-200 hover:bg-dark-blue-100",
      className,
    )}
    onClick={onClick}
  >
    <SvgArrowUp className="stroke-gray-200" />
  </div>
))
