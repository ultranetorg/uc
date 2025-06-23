import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { ArrowUpSvg } from "assets"
import { PropsWithClassName } from "types"

type ScrollToTopButtonBaseProps = {
  onClick: () => void
}

export type ScrollToTopButtonProps = PropsWithClassName & ScrollToTopButtonBaseProps

export const ScrollToTopButton = memo(({ className, onClick }: ScrollToTopButtonProps) => (
  <div
    className={twMerge(
      "transition-base flex h-13 w-13 cursor-pointer items-center justify-center rounded-full bg-gray-800 hover:bg-gray-950",
      className,
    )}
    onClick={onClick}
  >
    <ArrowUpSvg className="stroke-gray-100" />
  </div>
))
