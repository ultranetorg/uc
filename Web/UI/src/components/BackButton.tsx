import { memo } from "react"

import { SvgChevronLeftBold } from "assets"

export type BackButtonProps = {
  onClick?: () => void
}

export const BackButton = memo(({ onClick }: BackButtonProps) => (
  <div
    className="box-border flex h-14 w-14 cursor-pointer items-center justify-center rounded-xl border border-dark-alpha-100 bg-dark-alpha-100 hover:border-dark-alpha-50 hover:bg-dark-alpha-50"
    onClick={onClick}
  >
    <SvgChevronLeftBold className="stroke-gray-200" />
  </div>
))
