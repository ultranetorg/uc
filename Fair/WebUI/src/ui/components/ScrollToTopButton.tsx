import { ArrowUpSvg } from "assets/arrow-up"
import { memo } from "react"

export type ScrollToTopButtonProps = {
  onClick: () => void
}

export const ScrollToTopButton = memo(({ onClick }: ScrollToTopButtonProps) => (
  <div
    className="w-13 h-13 flex items-center justify-center rounded-full bg-gray-800 hover:bg-gray-950"
    onClick={onClick}
  >
    <ArrowUpSvg className="stroke-gray-100" />
  </div>
))
