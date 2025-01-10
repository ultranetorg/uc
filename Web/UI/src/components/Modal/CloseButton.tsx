import { memo } from "react"

import { SvgXLg } from "assets"

export type CloseButtonProps = {
  onClick: () => void
}

export const CloseButton = memo(({ onClick }: CloseButtonProps) => (
  <div
    className="group box-border flex h-9 w-9 cursor-pointer items-center justify-center rounded-md border border-solid border-dark-alpha-100 p-[6px] hover:bg-dark-alpha-50"
    onClick={onClick}
  >
    <SvgXLg className="stroke-gray-500 group-hover:stroke-gray-500" />
  </div>
))
